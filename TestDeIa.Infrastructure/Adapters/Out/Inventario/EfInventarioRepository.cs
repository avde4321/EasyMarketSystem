using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Domain.Modules.Inventario.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Inventario;

public sealed class EfInventarioRepository : IInventarioRepository
{
    private readonly TestDeIaDbContext dbContext;

    public EfInventarioRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Producto>> GetProductosAsync(CancellationToken cancellationToken = default)
    {
        var productos = await dbContext.Productos
            .AsNoTracking()
            .OrderBy(producto => producto.Nombre)
            .ToListAsync(cancellationToken);

        return productos.Select(MapProducto).ToArray();
    }

    public async Task<Producto?> GetProductoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await dbContext.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return producto is null ? null : MapProducto(producto);
    }

    public Task<bool> ExistsProductoCodigoAsync(
        string codigo,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCodigo = codigo.Trim();

        return dbContext.Productos.AnyAsync(
            producto =>
                producto.Codigo == normalizedCodigo &&
                (!excludedId.HasValue || producto.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task<Producto> CreateProductoAsync(
        Producto producto,
        decimal stockInicial,
        decimal costoInicial,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var entity = MapProductoEntity(producto);
        dbContext.Productos.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (stockInicial > 0)
        {
            await RegistrarMovimientoInternalAsync(
                entity,
                "Entrada",
                "Stock inicial",
                "INICIAL",
                stockInicial,
                costoInicial,
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return MapProducto(entity);
    }

    public async Task<Producto?> UpdateProductoAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Productos
            .FirstOrDefaultAsync(current => current.Id == producto.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Codigo = producto.Codigo;
        entity.Nombre = producto.Nombre;
        entity.Descripcion = producto.Descripcion;
        entity.CodigoIva = producto.CodigoIva;
        entity.PorcentajeIva = producto.PorcentajeIva;
        entity.PrecioVenta = producto.PrecioVenta;
        entity.StockMinimo = producto.StockMinimo;
        entity.IsActive = producto.IsActive;
        entity.UpdatedAt = producto.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapProducto(entity);
    }

    public async Task<IReadOnlyCollection<KardexMovimiento>> GetKardexAsync(Guid productoId, CancellationToken cancellationToken = default)
    {
        var movimientos = await dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(movimiento => movimiento.ProductoId == productoId)
            .OrderByDescending(movimiento => movimiento.FechaMovimiento)
            .ToListAsync(cancellationToken);

        return movimientos.Select(MapKardex).ToArray();
    }

    public async Task<Producto?> RegistrarMovimientoAsync(
        Guid productoId,
        string tipoMovimiento,
        string concepto,
        string? referencia,
        decimal cantidad,
        decimal costoUnitario,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var producto = await dbContext.Productos
            .FirstOrDefaultAsync(current => current.Id == productoId, cancellationToken);

        if (producto is null)
        {
            return null;
        }

        await RegistrarMovimientoInternalAsync(
            producto,
            tipoMovimiento,
            concepto,
            referencia,
            cantidad,
            costoUnitario,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return MapProducto(producto);
    }

    public async Task DescontarStockPorFacturaAsync(
        string referenciaFactura,
        IReadOnlyCollection<(Guid ProductoId, decimal Cantidad)> items,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var item in items)
        {
            var producto = await dbContext.Productos
                .FirstOrDefaultAsync(current => current.Id == item.ProductoId, cancellationToken)
                ?? throw new InvalidOperationException("No se encontro uno de los productos de la factura.");

            await RegistrarMovimientoInternalAsync(
                producto,
                "Salida",
                "Factura",
                referenciaFactura,
                item.Cantidad,
                producto.CostoPromedio,
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task RegistrarMovimientoInternalAsync(
        ProductoEntity producto,
        string tipoMovimiento,
        string concepto,
        string? referencia,
        decimal cantidad,
        decimal costoUnitario,
        CancellationToken cancellationToken)
    {
        var isEntrada = string.Equals(tipoMovimiento, "Entrada", StringComparison.OrdinalIgnoreCase);
        var isSalida = string.Equals(tipoMovimiento, "Salida", StringComparison.OrdinalIgnoreCase);

        if (!isEntrada && !isSalida)
        {
            throw new InvalidOperationException("El tipo de movimiento debe ser Entrada o Salida.");
        }

        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
        }

        if (costoUnitario < 0)
        {
            throw new InvalidOperationException("El costo unitario no puede ser negativo.");
        }

        if (isSalida && producto.StockActual < cantidad)
        {
            throw new InvalidOperationException("No existe stock suficiente para registrar la salida.");
        }

        var stockAnterior = producto.StockActual;
        var costoAnterior = producto.CostoPromedio;
        var nuevoStock = isEntrada ? stockAnterior + cantidad : stockAnterior - cantidad;
        var nuevoCostoPromedio = CalculateCostoPromedio(
            isEntrada,
            stockAnterior,
            costoAnterior,
            cantidad,
            costoUnitario);

        producto.StockActual = nuevoStock;
        producto.CostoPromedio = nuevoStock == 0 ? 0 : nuevoCostoPromedio;
        producto.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.KardexMovimientos.Add(new KardexMovimientoEntity
        {
            Id = Guid.NewGuid(),
            ProductoId = producto.Id,
            TipoMovimiento = isEntrada ? "Entrada" : "Salida",
            Concepto = concepto,
            Referencia = referencia,
            CantidadEntrada = isEntrada ? cantidad : 0,
            CantidadSalida = isSalida ? cantidad : 0,
            SaldoCantidad = producto.StockActual,
            CostoUnitario = costoUnitario,
            CostoPromedio = producto.CostoPromedio,
            SaldoValor = producto.StockActual * producto.CostoPromedio,
            FechaMovimiento = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static decimal CalculateCostoPromedio(
        bool isEntrada,
        decimal stockAnterior,
        decimal costoAnterior,
        decimal cantidad,
        decimal costoUnitario)
    {
        if (!isEntrada)
        {
            return costoAnterior;
        }

        var nuevoStock = stockAnterior + cantidad;
        if (nuevoStock == 0)
        {
            return 0;
        }

        var valorAnterior = stockAnterior * costoAnterior;
        var valorEntrada = cantidad * costoUnitario;

        return Math.Round((valorAnterior + valorEntrada) / nuevoStock, 6);
    }

    private static Producto MapProducto(ProductoEntity entity)
    {
        return new Producto(
            entity.Id,
            entity.Codigo,
            entity.Nombre,
            entity.Descripcion,
            entity.CodigoIva,
            entity.PorcentajeIva,
            entity.PrecioVenta,
            entity.StockActual,
            entity.StockMinimo,
            entity.CostoPromedio,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static ProductoEntity MapProductoEntity(Producto producto)
    {
        return new ProductoEntity
        {
            Id = producto.Id,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            CodigoIva = producto.CodigoIva,
            PorcentajeIva = producto.PorcentajeIva,
            PrecioVenta = producto.PrecioVenta,
            StockActual = producto.StockActual,
            StockMinimo = producto.StockMinimo,
            CostoPromedio = producto.CostoPromedio,
            IsActive = producto.IsActive,
            CreatedAt = producto.CreatedAt,
            UpdatedAt = producto.UpdatedAt
        };
    }

    private static KardexMovimiento MapKardex(KardexMovimientoEntity entity)
    {
        return new KardexMovimiento(
            entity.Id,
            entity.ProductoId,
            entity.TipoMovimiento,
            entity.Concepto,
            entity.Referencia,
            entity.CantidadEntrada,
            entity.CantidadSalida,
            entity.SaldoCantidad,
            entity.CostoUnitario,
            entity.CostoPromedio,
            entity.SaldoValor,
            entity.FechaMovimiento);
    }
}
