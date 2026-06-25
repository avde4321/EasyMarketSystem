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

    public async Task<IReadOnlyCollection<ProductoResponse>> GetCatalogoAsync(CancellationToken cancellationToken = default)
    {
        var productos = await inventarioRepository.GetProductosAsync(cancellationToken);
        return productos.Select(MapProducto).ToArray();
    }

    public async Task<PagedResultResponse<ProductoResponse>> GetCatalogoPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var page = await inventarioRepository.GetProductosPagedAsync(term, skip, take, cancellationToken);
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
            request.StockMinimo,
            0,
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapProducto(await inventarioRepository.CreateProductoAsync(
            producto,
            request.StockInicial,
            request.CostoInicial,
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
            request.StockMinimo,
            current.CostoPromedio,
            request.IsActive,
            current.CreatedAt,
            DateTimeOffset.UtcNow);

        var updated = await inventarioRepository.UpdateProductoAsync(producto, cancellationToken);
        return updated is null ? null : MapProducto(updated);
    }

    public async Task<IReadOnlyCollection<KardexMovimientoResponse>> GetKardexAsync(Guid productoId, CancellationToken cancellationToken = default)
    {
        var movimientos = await inventarioRepository.GetKardexAsync(productoId, cancellationToken);
        return movimientos.Select(MapKardex).ToArray();
    }

    public async Task<ProductoResponse?> AjustarStockAsync(Guid productoId, AjusteStockRequest request, CancellationToken cancellationToken = default)
    {
        ValidateTipoMovimiento(request.TipoMovimiento);
        ValidateMovimiento(request.Concepto, request.Cantidad, request.CostoUnitario);

        var producto = await inventarioRepository.RegistrarMovimientoAsync(
            productoId,
            request.TipoMovimiento.Trim(),
            request.Concepto.Trim(),
            NormalizeOptional(request.Referencia),
            request.Cantidad,
            request.CostoUnitario,
            cancellationToken);

        return producto is null ? null : MapProducto(producto);
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
            CostoPromedio = producto.CostoPromedio,
            IsActive = producto.IsActive
        };
    }

    private static KardexMovimientoResponse MapKardex(KardexMovimiento movimiento)
    {
        return new KardexMovimientoResponse
        {
            Id = movimiento.Id,
            ProductoId = movimiento.ProductoId,
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

        if (request.PrecioVenta < 0 || request.StockMinimo < 0 || request.StockInicial < 0 || request.CostoInicial < 0)
        {
            throw new InvalidOperationException("Los valores de inventario no pueden ser negativos.");
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

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
