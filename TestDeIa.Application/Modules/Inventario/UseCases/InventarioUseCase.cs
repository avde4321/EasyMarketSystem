using TestDeIa.Application.Modules.Inventario.Ports.In;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Domain.Modules.Inventario.Entities;
using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Application.Modules.Inventario.UseCases;

public sealed class InventarioUseCase : IInventarioUseCase
{
    private readonly IInventarioRepository inventarioRepository;

    public InventarioUseCase(IInventarioRepository inventarioRepository)
    {
        this.inventarioRepository = inventarioRepository;
    }

    public async Task<IReadOnlyCollection<BodegaResponse>> GetBodegasAsync(CancellationToken cancellationToken = default)
    {
        var bodegas = await inventarioRepository.GetBodegasAsync(cancellationToken);
        return bodegas.Select(MapBodega).ToArray();
    }

    public async Task<BodegaResponse?> GetBodegaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bodega = await inventarioRepository.GetBodegaByIdAsync(id, cancellationToken);
        return bodega is null ? null : MapBodega(bodega);
    }

    public async Task<BodegaResponse> CreateBodegaAsync(BodegaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateBodegaRequest(request);

        if (await inventarioRepository.ExistsBodegaNombreAsync(request.Nombre, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una bodega con ese nombre.");
        }

        var bodega = new Bodega(
            Guid.NewGuid(),
            Guid.Empty,
            request.Nombre.Trim(),
            NormalizeOptional(request.Direccion),
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapBodega(await inventarioRepository.CreateBodegaAsync(bodega, cancellationToken));
    }

    public async Task<BodegaResponse?> UpdateBodegaAsync(Guid id, BodegaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateBodegaRequest(request);

        if (await inventarioRepository.ExistsBodegaNombreAsync(request.Nombre, id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otra bodega con ese nombre.");
        }

        var current = await inventarioRepository.GetBodegaByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var bodega = new Bodega(
            id,
            current.EmpresaId,
            request.Nombre.Trim(),
            NormalizeOptional(request.Direccion),
            request.IsActive,
            current.CreatedAt,
            DateTimeOffset.UtcNow);

        var updated = await inventarioRepository.UpdateBodegaAsync(bodega, cancellationToken);
        return updated is null ? null : MapBodega(updated);
    }

    public async Task<IReadOnlyCollection<ProductoResponse>> GetCatalogoAsync(Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var productos = await inventarioRepository.GetProductosAsync(bodegaId, cancellationToken);
        return productos.Select(MapProducto).ToArray();
    }

    public async Task<PagedResultResponse<ProductoResponse>> GetCatalogoPagedAsync(string? term, int skip, int take, Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var page = await inventarioRepository.GetProductosPagedAsync(term, skip, take, bodegaId, cancellationToken);
        return new PagedResultResponse<ProductoResponse>
        {
            Items = page.Items.Select(MapProducto).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<ProductoResponse?> GetProductoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await inventarioRepository.GetProductoByIdAsync(id, cancellationToken);
        return producto is null ? null : MapProducto(producto);
    }

    public async Task<ProductoResponse> CreateProductoAsync(ProductoRequest request, CancellationToken cancellationToken = default)
    {
        ValidateProductoRequest(request);

        if (await inventarioRepository.ExistsProductoCodigoAsync(request.Codigo, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un producto con ese codigo.");
        }

        var producto = new Producto(
            Guid.NewGuid(),
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Descripcion),
            request.CodigoIva.Trim(),
            request.PorcentajeIva,
            request.PrecioVenta,
            0,
            request.ControlaStock ? request.StockMinimo ?? 0 : null,
            0,
            request.ControlaStock,
            !request.ControlaStock && request.AplicaComision,
            !request.ControlaStock && request.AplicaComision ? NormalizeTipoComision(request.TipoComision) : null,
            !request.ControlaStock && request.AplicaComision ? request.ValorComision : null,
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapProducto(await inventarioRepository.CreateProductoAsync(
            producto,
            request.ControlaStock ? request.StockInicial : 0,
            request.ControlaStock ? request.CostoInicial : 0,
            cancellationToken));
    }

    public async Task<ProductoResponse?> UpdateProductoAsync(Guid id, ProductoRequest request, CancellationToken cancellationToken = default)
    {
        ValidateProductoRequest(request);

        if (await inventarioRepository.ExistsProductoCodigoAsync(request.Codigo, id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro producto con ese codigo.");
        }

        var current = await inventarioRepository.GetProductoByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var producto = new Producto(
            id,
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Descripcion),
            request.CodigoIva.Trim(),
            request.PorcentajeIva,
            request.PrecioVenta,
            current.StockActual,
            request.ControlaStock ? request.StockMinimo ?? 0 : null,
            current.CostoPromedio,
            request.ControlaStock,
            !request.ControlaStock && request.AplicaComision,
            !request.ControlaStock && request.AplicaComision ? NormalizeTipoComision(request.TipoComision) : null,
            !request.ControlaStock && request.AplicaComision ? request.ValorComision : null,
            request.IsActive,
            current.CreatedAt,
            DateTimeOffset.UtcNow);

        var updated = await inventarioRepository.UpdateProductoAsync(producto, cancellationToken);
        return updated is null ? null : MapProducto(updated);
    }

    public async Task<IReadOnlyCollection<KardexMovimientoResponse>> GetKardexAsync(Guid productoId, Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var movimientos = await inventarioRepository.GetKardexAsync(productoId, bodegaId, cancellationToken);
        return movimientos.Select(MapKardex).ToArray();
    }

    public async Task<IReadOnlyCollection<StockAlertaResponse>> GetAlertasStockAsync(CancellationToken cancellationToken = default)
    {
        var alertas = await inventarioRepository.GetAlertasStockAsync(cancellationToken);
        return alertas.Select(MapAlerta).ToArray();
    }

    public async Task<ProductoResponse?> AjustarStockAsync(Guid productoId, AjusteStockRequest request, CancellationToken cancellationToken = default)
    {
        ValidateTipoMovimiento(request.TipoMovimiento);
        ValidateMovimiento(request.Concepto, request.Cantidad, request.CostoUnitario);

        var producto = await inventarioRepository.RegistrarMovimientoAsync(
            productoId,
            request.BodegaId,
            request.TipoMovimiento.Trim(),
            request.Concepto.Trim(),
            NormalizeOptional(request.Referencia),
            request.Cantidad,
            request.CostoUnitario,
            cancellationToken);

        return producto is null ? null : MapProducto(producto);
    }

    public async Task<ProductoResponse?> RegistrarCompraAsync(IngresoCompraRequest request, CancellationToken cancellationToken = default)
    {
        ValidateBodegaMovimientoIds(request.ProductoId, request.BodegaId);
        ValidateCantidad(request.Cantidad);

        if (request.CostoUnitarioCompra <= 0)
        {
            throw new InvalidOperationException("El costo unitario de compra debe ser mayor a cero.");
        }

        var producto = await inventarioRepository.RegistrarCompraAsync(
            request.ProductoId,
            request.BodegaId,
            request.Cantidad,
            request.CostoUnitarioCompra,
            NormalizeOptional(request.Referencia),
            cancellationToken);

        return producto is null ? null : MapProducto(producto);
    }

    public async Task<ProductoResponse?> RegistrarMermaAsync(EgresoMermaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateBodegaMovimientoIds(request.ProductoId, request.BodegaId);
        ValidateCantidad(request.Cantidad);

        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new InvalidOperationException("El motivo de merma es obligatorio.");
        }

        var producto = await inventarioRepository.RegistrarMermaAsync(
            request.ProductoId,
            request.BodegaId,
            request.Cantidad,
            request.Motivo.Trim(),
            NormalizeOptional(request.Referencia),
            cancellationToken);

        return producto is null ? null : MapProducto(producto);
    }

    public async Task<ProductoResponse?> TransferirStockAsync(TransferenciaInventarioRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ProductoId == Guid.Empty || request.BodegaOrigenId == Guid.Empty || request.BodegaDestinoId == Guid.Empty)
        {
            throw new InvalidOperationException("Producto, bodega origen y bodega destino son obligatorios.");
        }

        if (request.BodegaOrigenId == request.BodegaDestinoId)
        {
            throw new InvalidOperationException("La bodega origen y la bodega destino deben ser diferentes.");
        }

        ValidateCantidad(request.Cantidad);

        var producto = await inventarioRepository.TransferirStockAsync(
            request.ProductoId,
            request.BodegaOrigenId,
            request.BodegaDestinoId,
            request.Cantidad,
            NormalizeOptional(request.Referencia),
            cancellationToken);

        return producto is null ? null : MapProducto(producto);
    }

    public async Task<TomaFisicaResultadoResponse> ProcesarTomaFisicaAsync(TomaFisicaInventarioRequest request, CancellationToken cancellationToken = default)
    {
        if (request.BodegaId == Guid.Empty)
        {
            throw new InvalidOperationException("La bodega es obligatoria para procesar la toma fisica.");
        }

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Debes registrar al menos un producto contado en la toma fisica.");
        }

        if (request.Items.Any(item => item.ProductoId == Guid.Empty || item.CantidadContada < 0))
        {
            throw new InvalidOperationException("La toma fisica contiene productos o cantidades invalidas.");
        }

        var items = request.Items
            .GroupBy(item => item.ProductoId)
            .Select(group => (ProductoId: group.Key, CantidadContada: group.Last().CantidadContada))
            .ToArray();

        var resultado = await inventarioRepository.ProcesarTomaFisicaAsync(
            request.BodegaId,
            string.IsNullOrWhiteSpace(request.Concepto) ? "Toma fisica" : request.Concepto.Trim(),
            items,
            cancellationToken);

        return new TomaFisicaResultadoResponse
        {
            BodegaId = resultado.BodegaId,
            BodegaNombre = resultado.BodegaNombre,
            ProductosProcesados = resultado.ProductosProcesados,
            MovimientosGenerados = resultado.MovimientosGenerados
        };
    }

    public Task DescontarStockPorFacturaAsync(DescontarStockFacturaRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ReferenciaFactura))
        {
            throw new InvalidOperationException("La referencia de la factura es obligatoria.");
        }

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("La factura no contiene productos para descontar.");
        }

        if (request.Items.Any(item => item.ProductoId == Guid.Empty || item.Cantidad <= 0))
        {
            throw new InvalidOperationException("La factura contiene productos invalidos para descontar.");
        }

        return inventarioRepository.DescontarStockPorFacturaAsync(
            Guid.NewGuid(),
            request.BodegaId,
            request.ReferenciaFactura.Trim(),
            "Factura",
            request.Items.Select(item => (item.ProductoId, item.Cantidad)).ToArray(),
            cancellationToken);
    }

    private static ProductoResponse MapProducto(Producto producto)
    {
        return new ProductoResponse
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
            ControlaStock = producto.ControlaStock,
            AplicaComision = producto.AplicaComision,
            TipoComision = producto.TipoComision,
            ValorComision = producto.ValorComision,
            CostoPromedio = producto.CostoPromedio,
            IsActive = producto.IsActive
        };
    }

    private static BodegaResponse MapBodega(Bodega bodega)
    {
        return new BodegaResponse
        {
            Id = bodega.Id,
            Nombre = bodega.Nombre,
            Direccion = bodega.Direccion,
            IsActive = bodega.IsActive,
            CreatedAt = bodega.CreatedAt,
            UpdatedAt = bodega.UpdatedAt
        };
    }

    private static KardexMovimientoResponse MapKardex(KardexMovimiento movimiento)
    {
        return new KardexMovimientoResponse
        {
            Id = movimiento.Id,
            ProductoId = movimiento.ProductoId,
            BodegaId = movimiento.BodegaId,
            BodegaNombre = movimiento.BodegaNombre,
            TipoMovimiento = movimiento.TipoMovimiento,
            Concepto = movimiento.Concepto,
            Referencia = movimiento.Referencia,
            CantidadEntrada = movimiento.CantidadEntrada,
            CantidadSalida = movimiento.CantidadSalida,
            SaldoCantidad = movimiento.SaldoCantidad,
            CostoUnitario = movimiento.CostoUnitario,
            CostoPromedio = movimiento.CostoPromedio,
            SaldoValor = movimiento.SaldoValor,
            FechaMovimiento = movimiento.FechaMovimiento
        };
    }

    private static StockAlertaResponse MapAlerta(StockAlerta alerta)
    {
        return new StockAlertaResponse
        {
            ProductoId = alerta.ProductoId,
            Codigo = alerta.Codigo,
            Nombre = alerta.Nombre,
            StockMinimo = alerta.StockMinimo,
            StockTotal = alerta.StockTotal,
            BodegasComprometidas = alerta.BodegasComprometidas
                .Select(bodega => new StockAlertaBodegaResponse
                {
                    BodegaId = bodega.BodegaId,
                    BodegaNombre = bodega.BodegaNombre,
                    StockActual = bodega.StockActual
                })
                .ToArray()
        };
    }

    private static void ValidateTipoMovimiento(string tipoMovimiento)
    {
        if (!string.Equals(tipoMovimiento, "Entrada", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(tipoMovimiento, "Salida", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El tipo de movimiento debe ser Entrada o Salida.");
        }
    }

    private static void ValidateProductoRequest(ProductoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            throw new InvalidOperationException("El codigo del producto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.CodigoIva))
        {
            throw new InvalidOperationException("El codigo fiscal de IVA es obligatorio.");
        }

        if (request.PorcentajeIva < 0 || request.PorcentajeIva > 100)
        {
            throw new InvalidOperationException("El porcentaje de IVA debe estar entre 0 y 100.");
        }

        if (request.PrecioVenta < 0 || request.StockInicial < 0 || request.CostoInicial < 0)
        {
            throw new InvalidOperationException("Los valores de inventario no pueden ser negativos.");
        }

        if (request.ControlaStock && request.StockMinimo is null)
        {
            throw new InvalidOperationException("El stock minimo es obligatorio para items inventariables.");
        }

        if (request.StockMinimo.HasValue && request.StockMinimo.Value < 0)
        {
            throw new InvalidOperationException("El stock minimo no puede ser negativo.");
        }

        if (request.ControlaStock && request.AplicaComision)
        {
            throw new InvalidOperationException("Las comisiones solo aplican para servicios sin control de stock.");
        }

        if (request.AplicaComision)
        {
            var tipoComision = NormalizeTipoComision(request.TipoComision);
            if (tipoComision is null)
            {
                throw new InvalidOperationException("Debe seleccionar el tipo de comision del servicio.");
            }

            if (request.ValorComision is null or < 0)
            {
                throw new InvalidOperationException("Debe ingresar un valor de comision valido.");
            }
        }
    }

    private static string? NormalizeTipoComision(string? tipoComision)
    {
        if (string.IsNullOrWhiteSpace(tipoComision))
        {
            return null;
        }

        return tipoComision.Trim() switch
        {
            "Porcentaje" => "Porcentaje",
            "ValorFijo" => "ValorFijo",
            _ => null
        };
    }

    private static void ValidateBodegaRequest(BodegaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new InvalidOperationException("El nombre de la bodega es obligatorio.");
        }
    }

    private static void ValidateMovimiento(string concepto, decimal cantidad, decimal costoUnitario)
    {
        if (string.IsNullOrWhiteSpace(concepto))
        {
            throw new InvalidOperationException("El concepto del movimiento es obligatorio.");
        }

        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
        }

        if (costoUnitario < 0)
        {
            throw new InvalidOperationException("El costo unitario no puede ser negativo.");
        }
    }

    private static void ValidateBodegaMovimientoIds(Guid productoId, Guid bodegaId)
    {
        if (productoId == Guid.Empty || bodegaId == Guid.Empty)
        {
            throw new InvalidOperationException("Producto y bodega son obligatorios.");
        }
    }

    private static void ValidateCantidad(decimal cantidad)
    {
        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

