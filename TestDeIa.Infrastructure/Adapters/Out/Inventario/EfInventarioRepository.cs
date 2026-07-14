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
    private const string PrincipalBodegaName = "Principal";
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

    public async Task<IReadOnlyCollection<Bodega>> GetBodegasAsync(CancellationToken cancellationToken = default)
    {
        var bodegas = await dbContext.Bodegas
            .AsNoTracking()
            .OrderByDescending(bodega => bodega.Nombre == PrincipalBodegaName)
            .ThenBy(bodega => bodega.Nombre)
            .ToListAsync(cancellationToken);

        return bodegas.Select(MapBodega).ToArray();
    }

    public async Task<Bodega?> GetBodegaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bodega = await dbContext.Bodegas
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return bodega is null ? null : MapBodega(bodega);
    }

    public Task<bool> ExistsBodegaNombreAsync(string nombre, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var normalizedNombre = nombre.Trim();

        return dbContext.Bodegas.AnyAsync(
            current => current.Nombre == normalizedNombre &&
                       (!excludedId.HasValue || current.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task<Bodega> CreateBodegaAsync(Bodega bodega, CancellationToken cancellationToken = default)
    {
        var entity = new BodegaEntity
        {
            Id = bodega.Id,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para la bodega."),
            Nombre = bodega.Nombre,
            Direccion = bodega.Direccion,
            IsActive = bodega.IsActive,
            CreatedAt = bodega.CreatedAt,
            UpdatedAt = bodega.UpdatedAt
        };

        dbContext.Bodegas.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapBodega(entity);
    }

    public async Task<Bodega?> UpdateBodegaAsync(Bodega bodega, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Bodegas
            .FirstOrDefaultAsync(current => current.Id == bodega.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Nombre = bodega.Nombre;
        entity.Direccion = bodega.Direccion;
        entity.IsActive = bodega.IsActive;
        entity.UpdatedAt = bodega.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapBodega(entity);
    }

    public async Task<IReadOnlyCollection<Producto>> GetProductosAsync(Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var productos = await dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.ProductosBodega)
            .OrderBy(producto => producto.Nombre)
            .ToListAsync(cancellationToken);

        return productos.Select(producto => MapProducto(producto, bodegaId)).ToArray();
    }

    public async Task<PagedResultResponse<Producto>> GetProductosPagedAsync(string? term, int skip, int take, Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(
            dbContext.Productos
                .AsNoTracking()
                .Include(producto => producto.ProductosBodega),
            term);

        var totalCount = await query.CountAsync(cancellationToken);
        var productos = await query
            .OrderBy(producto => producto.Nombre)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<Producto>
        {
            Items = productos.Select(producto => MapProducto(producto, bodegaId)).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<Producto?> GetProductoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await dbContext.Productos
            .AsNoTracking()
            .Include(current => current.ProductosBodega)
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

        if (producto.ControlaStock)
        {
            var principalBodega = await GetOrCreatePrincipalBodegaAsync(cancellationToken);
            var existenciaPrincipal = await GetOrCreateProductoBodegaAsync(entity, principalBodega, cancellationToken);

            if (stockInicial > 0)
            {
                RegistrarMovimientoInternal(
                    entity,
                    existenciaPrincipal,
                    principalBodega,
                    "Entrada",
                    "Stock inicial",
                    "INICIAL",
                    stockInicial,
                    costoInicial,
                    recalcularCostoPromedioEnEntrada: true);

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetProductoByIdAsync(entity.Id, cancellationToken) ?? MapProducto(entity);
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
        entity.ControlaStock = producto.ControlaStock;
        entity.AplicaComision = producto.AplicaComision;
        entity.TipoComision = producto.TipoComision;
        entity.ValorComision = producto.ValorComision;
        entity.UpdatedAt = producto.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetProductoByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<KardexMovimiento>> GetKardexAsync(Guid productoId, Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.KardexMovimientos
            .AsNoTracking()
            .Include(movimiento => movimiento.Bodega)
            .Where(movimiento => movimiento.ProductoId == productoId);

        if (bodegaId.HasValue && bodegaId.Value != Guid.Empty)
        {
            query = query.Where(movimiento => movimiento.BodegaId == bodegaId.Value);
        }

        var movimientos = await query
            .OrderByDescending(movimiento => movimiento.FechaMovimiento)
            .ToListAsync(cancellationToken);

        return movimientos.Select(MapKardex).ToArray();
    }

    public async Task<IReadOnlyCollection<StockAlerta>> GetAlertasStockAsync(CancellationToken cancellationToken = default)
    {
        var productos = await dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.ProductosBodega)
            .ThenInclude(productoBodega => productoBodega.Bodega)
            .Where(producto => producto.IsActive && producto.ControlaStock)
            .OrderBy(producto => producto.Nombre)
            .ToListAsync(cancellationToken);

        return productos
            .Select(producto =>
            {
                var bodegasActivas = producto.ProductosBodega
                    .Where(current => current.Bodega.IsActive)
                    .OrderBy(current => current.StockActual)
                    .ThenBy(current => current.Bodega.Nombre)
                    .Select(current => new StockAlertaBodega(
                        current.BodegaId,
                        current.Bodega.Nombre,
                        current.StockActual))
                    .ToArray();

                return new StockAlerta(
                    producto.Id,
                    producto.Codigo,
                    producto.Nombre,
                    producto.StockMinimo ?? 0,
                    bodegasActivas.Sum(current => current.StockActual),
                    bodegasActivas);
            })
            .Where(alerta => alerta.StockTotal <= alerta.StockMinimo)
            .ToArray();
    }

    public async Task<Producto?> RegistrarMovimientoAsync(
        Guid productoId,
        Guid? bodegaId,
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
                    .Include(current => current.ProductosBodega)
                    .FirstOrDefaultAsync(current => current.Id == productoId, token);

                if (producto is null)
                {
                    productoActualizado = null;
                    return;
                }

                var operationalBodega = await GetBodegaByIdOrPrincipalAsync(bodegaId, token);
                var existenciaPrincipal = await GetOrCreateProductoBodegaAsync(producto, operationalBodega, token);

                RegistrarMovimientoInternal(
                    producto,
                    existenciaPrincipal,
                    operationalBodega,
                    tipoMovimiento,
                    concepto,
                    referencia,
                    cantidad,
                    costoUnitario,
                    recalcularCostoPromedioEnEntrada: true,
                    stockInsuficienteMensaje: $"No existe stock suficiente en la bodega {operationalBodega.Nombre} para registrar la salida.");

                productoActualizado = MapProducto(producto);
            },
            $"ajuste manual de inventario para producto {productoId}",
            cancellationToken);

        return productoActualizado;
    }

    public async Task<Producto?> RegistrarCompraAsync(
        Guid productoId,
        Guid bodegaId,
        decimal cantidad,
        decimal costoUnitarioCompra,
        string? referencia,
        CancellationToken cancellationToken = default)
    {
        Producto? productoActualizado = null;
        await ExecuteWithConcurrencyRetryAsync(
            async token =>
            {
                var producto = await dbContext.Productos
                    .Include(current => current.ProductosBodega)
                    .FirstOrDefaultAsync(current => current.Id == productoId, token);

                if (producto is null)
                {
                    productoActualizado = null;
                    return;
                }

                EnsureProductoControlaStock(producto, "registrar una compra");

                var bodega = await GetBodegaEntityByIdAsync(bodegaId, token);
                var existencia = await GetOrCreateProductoBodegaAsync(producto, bodega, token);

                RegistrarMovimientoInternal(
                    producto,
                    existencia,
                    bodega,
                    "Entrada",
                    "INGRESO_COMPRA",
                    referencia,
                    cantidad,
                    costoUnitarioCompra,
                    recalcularCostoPromedioEnEntrada: true);

                productoActualizado = MapProducto(producto);
            },
            $"ingreso por compra del producto {productoId} en bodega {bodegaId}",
            cancellationToken);

        return productoActualizado;
    }

    public async Task<Producto?> RegistrarMermaAsync(
        Guid productoId,
        Guid bodegaId,
        decimal cantidad,
        string motivo,
        string? referencia,
        CancellationToken cancellationToken = default)
    {
        Producto? productoActualizado = null;
        await ExecuteWithConcurrencyRetryAsync(
            async token =>
            {
                var producto = await dbContext.Productos
                    .Include(current => current.ProductosBodega)
                    .FirstOrDefaultAsync(current => current.Id == productoId, token);

                if (producto is null)
                {
                    productoActualizado = null;
                    return;
                }

                EnsureProductoControlaStock(producto, "registrar una merma");

                var bodega = await GetBodegaEntityByIdAsync(bodegaId, token);
                var existencia = await GetOrCreateProductoBodegaAsync(producto, bodega, token);

                RegistrarMovimientoInternal(
                    producto,
                    existencia,
                    bodega,
                    "Salida",
                    "EGRESO_MERMA",
                    string.IsNullOrWhiteSpace(referencia) ? motivo : $"{motivo} | {referencia}",
                    cantidad,
                    producto.CostoPromedio,
                    recalcularCostoPromedioEnEntrada: false,
                    stockInsuficienteMensaje: $"No existe stock suficiente en la bodega {bodega.Nombre} para registrar la merma.");

                productoActualizado = MapProducto(producto);
            },
            $"egreso por merma del producto {productoId} en bodega {bodegaId}",
            cancellationToken);

        return productoActualizado;
    }

    public async Task<Producto?> TransferirStockAsync(
        Guid productoId,
        Guid bodegaOrigenId,
        Guid bodegaDestinoId,
        decimal cantidad,
        string? referencia,
        CancellationToken cancellationToken = default)
    {
        Producto? productoActualizado = null;
        await ExecuteWithConcurrencyRetryAsync(
            async token =>
            {
                var producto = await dbContext.Productos
                    .Include(current => current.ProductosBodega)
                    .FirstOrDefaultAsync(current => current.Id == productoId, token);

                if (producto is null)
                {
                    productoActualizado = null;
                    return;
                }

                EnsureProductoControlaStock(producto, "transferir inventario");

                var bodegaOrigen = await GetBodegaEntityByIdAsync(bodegaOrigenId, token);
                var bodegaDestino = await GetBodegaEntityByIdAsync(bodegaDestinoId, token);
                var existenciaOrigen = await GetOrCreateProductoBodegaAsync(producto, bodegaOrigen, token);
                var existenciaDestino = await GetOrCreateProductoBodegaAsync(producto, bodegaDestino, token);
                var referenciaTransferencia = string.IsNullOrWhiteSpace(referencia)
                    ? $"TRANSFERENCIA:{bodegaOrigen.Nombre}->{bodegaDestino.Nombre}"
                    : referencia;

                RegistrarMovimientoInternal(
                    producto,
                    existenciaOrigen,
                    bodegaOrigen,
                    "Salida",
                    "TRANSFERENCIA_SALIDA",
                    referenciaTransferencia,
                    cantidad,
                    producto.CostoPromedio,
                    recalcularCostoPromedioEnEntrada: false,
                    stockInsuficienteMensaje: $"No existe stock suficiente en la bodega {bodegaOrigen.Nombre} para transferir.");

                RegistrarMovimientoInternal(
                    producto,
                    existenciaDestino,
                    bodegaDestino,
                    "Entrada",
                    "TRANSFERENCIA_ENTRADA",
                    referenciaTransferencia,
                    cantidad,
                    producto.CostoPromedio,
                    recalcularCostoPromedioEnEntrada: false);

                productoActualizado = MapProducto(producto);
            },
            $"transferencia del producto {productoId} desde {bodegaOrigenId} hacia {bodegaDestinoId}",
            cancellationToken);

        return productoActualizado;
    }

    public async Task<TomaFisicaResultado> ProcesarTomaFisicaAsync(
        Guid bodegaId,
        string concepto,
        IReadOnlyCollection<(Guid ProductoId, decimal CantidadContada)> items,
        CancellationToken cancellationToken = default)
    {
        var resultado = new TomaFisicaResultado(bodegaId, string.Empty, 0, 0);

        await ExecuteWithConcurrencyRetryAsync(
            async token =>
            {
                var bodega = await GetBodegaEntityByIdAsync(bodegaId, token);
                var productos = await dbContext.Productos
                    .Include(current => current.ProductosBodega)
                    .Where(current => items.Select(item => item.ProductoId).Contains(current.Id))
                    .ToDictionaryAsync(current => current.Id, token);

                var productosProcesados = 0;
                var movimientosGenerados = 0;

                foreach (var item in items)
                {
                    if (!productos.TryGetValue(item.ProductoId, out var producto))
                    {
                        throw new InvalidOperationException("Uno de los productos enviados en la toma fisica no existe.");
                    }

                    if (!producto.ControlaStock)
                    {
                        logger.LogInformation(
                            "Se omite la toma fisica para el producto {ProductoId} porque no controla stock.",
                            producto.Id);
                        continue;
                    }

                    var existencia = await GetOrCreateProductoBodegaAsync(producto, bodega, token);
                    var diferencia = Math.Round(item.CantidadContada - existencia.StockActual, 4);
                    productosProcesados++;

                    if (diferencia == 0)
                    {
                        continue;
                    }

                    var isIngreso = diferencia > 0;
                    RegistrarMovimientoInternal(
                        producto,
                        existencia,
                        bodega,
                        isIngreso ? "Entrada" : "Salida",
                        isIngreso ? "INGRESO_AJUSTE" : "EGRESO_AJUSTE",
                        concepto,
                        Math.Abs(diferencia),
                        producto.CostoPromedio,
                        recalcularCostoPromedioEnEntrada: false,
                        stockInsuficienteMensaje: $"No existe stock suficiente en la bodega {bodega.Nombre} para conciliar la toma fisica.");

                    movimientosGenerados++;
                }

                resultado = new TomaFisicaResultado(
                    bodega.Id,
                    bodega.Nombre,
                    productosProcesados,
                    movimientosGenerados);

                logger.LogInformation(
                    "Toma fisica procesada para la bodega {BodegaId} ({BodegaNombre}). Productos: {ProductosProcesados}. Ajustes: {MovimientosGenerados}.",
                    bodega.Id,
                    bodega.Nombre,
                    productosProcesados,
                    movimientosGenerados);
            },
            $"toma fisica en bodega {bodegaId}",
            cancellationToken);

        return resultado;
    }

    public async Task DescontarStockPorFacturaAsync(
        Guid facturaId,
        Guid? bodegaId,
        string referenciaFactura,
        string concepto,
        IReadOnlyCollection<(Guid ProductoId, decimal Cantidad)> items,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithConcurrencyRetryAsync(
            async token =>
            {
                var operationalBodega = await GetBodegaByIdOrPrincipalAsync(bodegaId, token);

                foreach (var item in items)
                {
                    var producto = await dbContext.Productos
                        .Include(current => current.ProductosBodega)
                        .FirstOrDefaultAsync(current => current.Id == item.ProductoId, token)
                        ?? throw new InvalidOperationException("No se encontro uno de los productos de la factura.");

                    if (!producto.ControlaStock)
                    {
                        logger.LogInformation(
                            "Se omite kardex y descuento de stock para el producto {ProductoId} en la factura {FacturaId} porque ControlaStock es false.",
                            producto.Id,
                            facturaId);
                        continue;
                    }

                    var existenciaPrincipal = await GetOrCreateProductoBodegaAsync(producto, operationalBodega, token);

                    RegistrarMovimientoInternal(
                        producto,
                        existenciaPrincipal,
                        operationalBodega,
                        "Salida",
                        concepto,
                        referenciaFactura,
                        item.Cantidad,
                        producto.CostoPromedio,
                        recalcularCostoPromedioEnEntrada: false,
                        stockInsuficienteMensaje: $"No existe stock suficiente en la bodega {operationalBodega.Nombre} para facturar.");
                }

                logger.LogInformation(
                    "Kardex de salida aplicado para factura {FacturaId} con referencia {ReferenciaFactura} y {TotalItems} items sobre bodega {BodegaNombre}.",
                    facturaId,
                    referenciaFactura,
                    items.Count,
                    operationalBodega.Nombre);
            },
            $"descuento de stock por factura {referenciaFactura}",
            cancellationToken);
    }

    private void RegistrarMovimientoInternal(
        ProductoEntity producto,
        ProductoBodegaEntity productoBodega,
        BodegaEntity bodega,
        string tipoMovimiento,
        string concepto,
        string? referencia,
        decimal cantidad,
        decimal costoUnitario,
        bool recalcularCostoPromedioEnEntrada,
        string? stockInsuficienteMensaje = null)
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

        if (isSalida && productoBodega.StockActual < cantidad)
        {
            throw new InvalidOperationException(stockInsuficienteMensaje ?? $"No existe stock suficiente en la bodega {bodega.Nombre} para registrar la salida.");
        }

        var stockAnteriorBodega = productoBodega.StockActual;
        var stockGlobalAnterior = producto.ProductosBodega.Sum(current => current.StockActual);
        var costoAnterior = producto.CostoPromedio;
        var nuevoStockBodega = isEntrada ? stockAnteriorBodega + cantidad : stockAnteriorBodega - cantidad;
        var nuevoCostoPromedio = recalcularCostoPromedioEnEntrada && isEntrada
            ? CalculateCostoPromedioEntrada(
                stockGlobalAnterior,
                costoAnterior,
                cantidad,
                costoUnitario)
            : costoAnterior;

        productoBodega.StockActual = nuevoStockBodega;
        producto.CostoPromedio = isEntrada && recalcularCostoPromedioEnEntrada ? nuevoCostoPromedio : costoAnterior;
        producto.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.KardexMovimientos.Add(new KardexMovimientoEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para el movimiento de inventario."),
            ProductoId = producto.Id,
            BodegaId = bodega.Id,
            TipoMovimiento = isEntrada ? "Entrada" : "Salida",
            Concepto = concepto,
            Referencia = referencia,
            CantidadEntrada = isEntrada ? cantidad : 0,
            CantidadSalida = isSalida ? cantidad : 0,
            SaldoCantidad = productoBodega.StockActual,
            CostoUnitario = costoUnitario,
            CostoPromedio = producto.CostoPromedio,
            SaldoValor = productoBodega.StockActual * producto.CostoPromedio,
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

    private async Task<BodegaEntity> GetOrCreatePrincipalBodegaAsync(CancellationToken cancellationToken)
    {
        var empresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para inventario.");

        var principal = await dbContext.Bodegas
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Nombre == PrincipalBodegaName, cancellationToken);

        if (principal is not null)
        {
            return principal;
        }

        principal = new BodegaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nombre = PrincipalBodegaName,
            Direccion = null,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Bodegas.Add(principal);
        await dbContext.SaveChangesAsync(cancellationToken);
        return principal;
    }

    private async Task<BodegaEntity> GetBodegaByIdOrPrincipalAsync(Guid? bodegaId, CancellationToken cancellationToken)
    {
        if (bodegaId.HasValue && bodegaId.Value != Guid.Empty)
        {
            return await GetBodegaEntityByIdAsync(bodegaId.Value, cancellationToken);
        }

        return await GetOrCreatePrincipalBodegaAsync(cancellationToken);
    }

    private async Task<BodegaEntity> GetBodegaEntityByIdAsync(Guid bodegaId, CancellationToken cancellationToken)
    {
        var empresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para inventario.");
        var bodega = await dbContext.Bodegas
            .FirstOrDefaultAsync(current => current.Id == bodegaId && current.EmpresaId == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("La bodega seleccionada no pertenece a la empresa activa.");

        if (!bodega.IsActive)
        {
            throw new InvalidOperationException($"La bodega {bodega.Nombre} no se encuentra activa.");
        }

        return bodega;
    }

    private async Task<ProductoBodegaEntity> GetOrCreateProductoBodegaAsync(
        ProductoEntity producto,
        BodegaEntity bodega,
        CancellationToken cancellationToken)
    {
        var existencia = producto.ProductosBodega.FirstOrDefault(current => current.BodegaId == bodega.Id);
        if (existencia is not null)
        {
            return existencia;
        }

        existencia = await dbContext.ProductosBodega
            .FirstOrDefaultAsync(current => current.ProductoId == producto.Id && current.BodegaId == bodega.Id, cancellationToken);

        if (existencia is not null)
        {
            producto.ProductosBodega.Add(existencia);
            return existencia;
        }

        existencia = new ProductoBodegaEntity
        {
            ProductoId = producto.Id,
            BodegaId = bodega.Id,
            EmpresaId = bodega.EmpresaId,
            StockActual = 0
        };

        dbContext.ProductosBodega.Add(existencia);
        producto.ProductosBodega.Add(existencia);
        return existencia;
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

    private static decimal CalculateCostoPromedioEntrada(
        decimal stockGlobalAnterior,
        decimal costoAnterior,
        decimal cantidadEntrada,
        decimal costoUnitarioEntrada)
    {
        var nuevoStock = stockGlobalAnterior + cantidadEntrada;
        if (nuevoStock == 0)
        {
            return 0;
        }

        var valorAnterior = stockGlobalAnterior * costoAnterior;
        var valorEntrada = cantidadEntrada * costoUnitarioEntrada;

        return Math.Round((valorAnterior + valorEntrada) / nuevoStock, 6);
    }

    private static Producto MapProducto(ProductoEntity entity, Guid? bodegaId = null)
    {
        var stockActual = bodegaId.HasValue && bodegaId.Value != Guid.Empty
            ? entity.ProductosBodega
                .Where(current => current.BodegaId == bodegaId.Value)
                .Select(current => current.StockActual)
                .FirstOrDefault()
            : entity.ProductosBodega.Sum(current => current.StockActual);

        return new Producto(
            entity.Id,
            entity.Codigo,
            entity.Nombre,
            entity.Descripcion,
            entity.CodigoIva,
            entity.PorcentajeIva,
            entity.PrecioVenta,
            stockActual,
            entity.StockMinimo,
            entity.CostoPromedio,
            entity.ControlaStock,
            entity.AplicaComision,
            entity.TipoComision,
            entity.ValorComision,
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
            StockMinimo = producto.StockMinimo,
            CostoPromedio = producto.CostoPromedio,
            ControlaStock = producto.ControlaStock,
            AplicaComision = producto.AplicaComision,
            TipoComision = producto.TipoComision,
            ValorComision = producto.ValorComision,
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
            entity.BodegaId,
            entity.Bodega.Nombre,
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

    private static void EnsureProductoControlaStock(ProductoEntity producto, string operation)
    {
        if (!producto.ControlaStock)
        {
            throw new InvalidOperationException($"El producto {producto.Nombre} no controla stock, por lo que no puede {operation}.");
        }
    }

    private static Bodega MapBodega(BodegaEntity entity)
    {
        return new Bodega(
            entity.Id,
            entity.EmpresaId,
            entity.Nombre,
            entity.Direccion,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}

