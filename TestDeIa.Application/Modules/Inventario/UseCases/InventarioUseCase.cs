using TestDeIa.Application.Modules.Inventario.Ports.In;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Domain.Modules.Inventario;
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

        if (await inventarioRepository.ExistsBodegaCodigoAsync(request.Codigo, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una bodega con ese codigo.");
        }

        var bodega = new Bodega(
            Guid.NewGuid(),
            Guid.Empty,
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Direccion),
            request.EsPrincipal,
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

        if (await inventarioRepository.ExistsBodegaCodigoAsync(request.Codigo, id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otra bodega con ese codigo.");
        }

        var current = await inventarioRepository.GetBodegaByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var bodega = new Bodega(
            id,
            current.EmpresaId,
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Direccion),
            request.EsPrincipal,
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

        var naturalezaItem = NormalizeNaturalezaItem(request.NaturalezaItem);
        var controlaStock = ShouldControlStock(naturalezaItem) && request.ControlaStock;
        var aplicaComision = string.Equals(naturalezaItem, "Servicio", StringComparison.OrdinalIgnoreCase) && request.AplicaComision;

        var producto = new Producto(
            Guid.NewGuid(),
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Descripcion),
            request.CategoriaId,
            NormalizeUnidadMedida(request.UnidadMedida),
            naturalezaItem,
            request.CodigoIva.Trim(),
            request.PorcentajeIva,
            request.PrecioVenta,
            request.CostoReferencial,
            0,
            controlaStock ? request.StockMinimo ?? 0 : null,
            0,
            controlaStock,
            aplicaComision,
            aplicaComision ? NormalizeTipoComision(request.TipoComision) : null,
            aplicaComision ? request.ValorComision : null,
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapProducto(await inventarioRepository.CreateProductoAsync(
            producto,
            controlaStock ? request.StockInicial : 0,
            controlaStock ? request.CostoInicial : 0,
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

        var naturalezaItem = NormalizeNaturalezaItem(request.NaturalezaItem);
        var controlaStock = ShouldControlStock(naturalezaItem) && request.ControlaStock;
        var aplicaComision = string.Equals(naturalezaItem, "Servicio", StringComparison.OrdinalIgnoreCase) && request.AplicaComision;

        var producto = new Producto(
            id,
            request.Codigo.Trim(),
            request.Nombre.Trim(),
            NormalizeOptional(request.Descripcion),
            request.CategoriaId,
            NormalizeUnidadMedida(request.UnidadMedida),
            naturalezaItem,
            request.CodigoIva.Trim(),
            request.PorcentajeIva,
            request.PrecioVenta,
            request.CostoReferencial,
            current.StockActual,
            controlaStock ? request.StockMinimo ?? 0 : null,
            current.CostoPromedio,
            controlaStock,
            aplicaComision,
            aplicaComision ? NormalizeTipoComision(request.TipoComision) : null,
            aplicaComision ? request.ValorComision : null,
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

    public Task<IReadOnlyCollection<StockDisponibleBodegaResponse>> GetDisponibilidadEnOtrasBodegasAsync(
        Guid productoId,
        Guid? bodegaActualId = null,
        CancellationToken cancellationToken = default)
    {
        if (productoId == Guid.Empty)
        {
            throw new InvalidOperationException("El producto es obligatorio para consultar disponibilidad.");
        }

        return inventarioRepository.GetDisponibilidadEnOtrasBodegasAsync(productoId, bodegaActualId, cancellationToken);
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

    public Task<IReadOnlyCollection<TransferenciaInventarioResponse>> GetTransferenciasAsync(
        string? estado = null,
        Guid? bodegaOrigenId = null,
        Guid? bodegaDestinoId = null,
        CancellationToken cancellationToken = default)
    {
        return inventarioRepository.GetTransferenciasAsync(estado, bodegaOrigenId, bodegaDestinoId, cancellationToken);
    }

    public Task<TransferenciaInventarioResponse?> GetTransferenciaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return inventarioRepository.GetTransferenciaByIdAsync(id, cancellationToken);
    }

    public Task<TransferenciaInventarioResponse> CreateTransferenciaAsync(
        TransferenciaInventarioFormalRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateTransferenciaFormalRequest(request);
        return inventarioRepository.CreateTransferenciaAsync(request, cancellationToken);
    }

    public Task<TransferenciaInventarioResponse?> DespacharTransferenciaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidOperationException("La transferencia es obligatoria.");
        }

        return inventarioRepository.DespacharTransferenciaAsync(id, cancellationToken);
    }

    public Task<TransferenciaInventarioResponse?> RecibirTransferenciaAsync(
        Guid id,
        RecepcionTransferenciaInventarioRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidOperationException("La transferencia es obligatoria.");
        }

        if (request.Detalles.Count == 0)
        {
            throw new InvalidOperationException("Debe registrar al menos una cantidad recibida.");
        }

        return inventarioRepository.RecibirTransferenciaAsync(id, request, cancellationToken);
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
            CategoriaId = producto.CategoriaId,
            UnidadMedida = producto.UnidadMedida,
            NaturalezaItem = producto.NaturalezaItem,
            CodigoIva = producto.CodigoIva,
            PorcentajeIva = producto.PorcentajeIva,
            PrecioVenta = producto.PrecioVenta,
            CostoReferencial = producto.CostoReferencial,
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
            Codigo = bodega.Codigo,
            Nombre = bodega.Nombre,
            Direccion = bodega.Direccion,
            EsPrincipal = bodega.EsPrincipal,
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
            !string.Equals(tipoMovimiento, "Salida", StringComparison.OrdinalIgnoreCase) &&
            !TipoMovimientoInventario.EsEntrada(tipoMovimiento) &&
            !TipoMovimientoInventario.EsSalida(tipoMovimiento))
        {
            throw new InvalidOperationException("El tipo de movimiento debe representar una entrada o salida de inventario.");
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

        if (request.CostoReferencial < 0)
        {
            throw new InvalidOperationException("El costo referencial no puede ser negativo.");
        }

        var naturalezaItem = NormalizeNaturalezaItem(request.NaturalezaItem);
        var controlaStock = ShouldControlStock(naturalezaItem) && request.ControlaStock;

        if (controlaStock && request.StockMinimo is null)
        {
            throw new InvalidOperationException("El stock minimo es obligatorio para items inventariables.");
        }

        if (request.StockMinimo.HasValue && request.StockMinimo.Value < 0)
        {
            throw new InvalidOperationException("El stock minimo no puede ser negativo.");
        }

        if (!string.Equals(naturalezaItem, "Servicio", StringComparison.OrdinalIgnoreCase) && request.AplicaComision)
        {
            throw new InvalidOperationException("Las comisiones solo aplican para servicios.");
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

    private static string NormalizeNaturalezaItem(string? naturalezaItem)
    {
        if (string.IsNullOrWhiteSpace(naturalezaItem))
        {
            return "Mercaderia";
        }

        return naturalezaItem.Trim() switch
        {
            "Mercaderia" => "Mercaderia",
            "Servicio" => "Servicio",
            "ActivoFijo" => "ActivoFijo",
            _ => throw new InvalidOperationException("La naturaleza del item debe ser Mercaderia, Servicio o ActivoFijo.")
        };
    }

    private static string NormalizeUnidadMedida(string? unidadMedida) =>
        string.IsNullOrWhiteSpace(unidadMedida) ? "Unidad" : unidadMedida.Trim();

    private static bool ShouldControlStock(string naturalezaItem) =>
        string.Equals(naturalezaItem, "Mercaderia", StringComparison.OrdinalIgnoreCase);

    private static void ValidateBodegaRequest(BodegaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new InvalidOperationException("El nombre de la bodega es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Codigo) || request.Codigo.Trim().Length != 3 || !request.Codigo.Trim().All(char.IsDigit))
        {
            throw new InvalidOperationException("El codigo de la bodega debe tener exactamente 3 digitos.");
        }
    }

    private static void ValidateTransferenciaFormalRequest(TransferenciaInventarioFormalRequest request)
    {
        if (request.BodegaOrigenId == Guid.Empty || request.BodegaDestinoId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe seleccionar bodega origen y destino.");
        }

        if (request.BodegaOrigenId == request.BodegaDestinoId)
        {
            throw new InvalidOperationException("La bodega origen y destino deben ser diferentes.");
        }

        if (request.Detalles.Count == 0)
        {
            throw new InvalidOperationException("Debe agregar al menos un producto a la transferencia.");
        }

        if (request.Detalles.Any(detalle => detalle.ProductoId == Guid.Empty || detalle.Cantidad <= 0))
        {
            throw new InvalidOperationException("Todos los productos de la transferencia deben tener cantidad mayor a cero.");
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

