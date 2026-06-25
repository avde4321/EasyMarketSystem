using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Domain.Modules.Inventario.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Infrastructure.Adapters.Out.Inventario;

public sealed class EfInventarioRepository : IInventarioRepository
{
    private const int MaxConcurrencyRetries = 3;
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;
    private readonly ILogger<EfInventarioRepository> logger;

    public EfInventarioRepository(
        TestDeIaDbContext dbContext,
        ITenantContextAccessor tenantContextAccessor,
        ILogger<EfInventarioRepository> logger)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
        this.logger = logger;
    }

    public async Task<IReadOnlyCollection<Producto>> GetProductosAsync(CancellationToken cancellationToken = default)
    {
        var productos = await dbContext.Productos
            .AsNoTracking()
            .OrderBy(producto => producto.Nombre)
            .ToListAsync(cancellationToken);

        return productos.Select(MapProducto).ToArray();
    }

    public async Task<PagedResultResponse<Producto>> GetProductosPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(dbContext.Productos.AsNoTracking(), term);
        var totalCount = await query.CountAsync(cancellationToken);
        var productos = await query
            .OrderBy(producto => producto.Nombre)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<Producto>
        {
            Items = productos.Select(MapProducto).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
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
            RegistrarMovimientoInternalAsync(
                entity,
                "Entrada",
                "Stock inicial",
                "INICIAL",
                stockInicial,
                costoInicial,
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
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
        Producto? productoActualizado = null;
        await ExecuteWithConcurrencyRetryAsync(
            async token =>
            {
                var producto = await dbContext.Productos
                    .FirstOrDefaultAsync(current => current.Id == productoId, token);

                if (producto is null)
                {
                    productoActualizado = null;
                    return;
                }

                RegistrarMovimientoInternalAsync(
                    producto,
                    tipoMovimiento,
                    concepto,
                    referencia,
                    cantidad,
                    costoUnitario,
                    token);

                productoActualizado = MapProducto(producto);
            },
            $"ajuste manual de inventario para producto {productoId}",
            cancellationToken);

        return productoActualizado;
    }

    public async Task DescontarStockPorFacturaAsync(
        Guid facturaId,
        string referenciaFactura,
        string concepto,
        IReadOnlyCollection<(Guid ProductoId, decimal Cantidad)> items,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithConcurrencyRetryAsync(
            async token =>
            {
                foreach (var item in items)
                {
                    var producto = await dbContext.Productos
                        .FirstOrDefaultAsync(current => current.Id == item.ProductoId, token)
                        ?? throw new InvalidOperationException("No se encontro uno de los productos de la factura.");

                    RegistrarMovimientoInternalAsync(
                        producto,
                        "Salida",
                        concepto,
                        referenciaFactura,
                        item.Cantidad,
                        producto.CostoPromedio,
                        token);
                }

                logger.LogInformation(
                    "Kardex de salida aplicado para factura {FacturaId} con referencia {ReferenciaFactura} y {TotalItems} items.",
                    facturaId,
                    referenciaFactura,
                    items.Count);
            },
            $"descuento de stock por factura {referenciaFactura}",
            cancellationToken);
    }

    private void RegistrarMovimientoInternalAsync(
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
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para el movimiento de inventario."),
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
    }

    private async Task ExecuteWithConcurrencyRetryAsync(
        Func<CancellationToken, Task> work,
        string operationName,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
        {
            var ownsTransaction = dbContext.Database.CurrentTransaction is null;
            await using var transaction = ownsTransaction
                ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            try
            {
                await work(cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return;
            }
            catch (DbUpdateConcurrencyException exception) when (attempt < MaxConcurrencyRetries)
            {
                logger.LogWarning(
                    exception,
                    "Colision de concurrencia durante {Operation}. Reintento {Attempt} de {MaxRetries}.",
                    operationName,
                    attempt,
                    MaxConcurrencyRetries);

                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                dbContext.ChangeTracker.Clear();
                await Task.Delay(TimeSpan.FromMilliseconds(60 * attempt), cancellationToken);
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                dbContext.ChangeTracker.Clear();
                throw;
            }
        }

        throw new InvalidOperationException($"No se pudo completar {operationName} por concurrencia luego de {MaxConcurrencyRetries} intentos.");
    }

    private static IQueryable<ProductoEntity> ApplyFilter(IQueryable<ProductoEntity> query, string? term)
    {
        var normalizedTerm = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTerm))
        {
            return query;
        }

        return query.Where(producto =>
            producto.Codigo.Contains(normalizedTerm) ||
            producto.Nombre.Contains(normalizedTerm) ||
            (producto.Descripcion != null && producto.Descripcion.Contains(normalizedTerm)) ||
            producto.CodigoIva.Contains(normalizedTerm));
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

    private ProductoEntity MapProductoEntity(Producto producto)
    {
        return new ProductoEntity
        {
            Id = producto.Id,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para el producto."),
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
