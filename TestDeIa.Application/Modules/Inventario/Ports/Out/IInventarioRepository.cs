using TestDeIa.Shared.Responses.Common;
using TestDeIa.Domain.Modules.Inventario.Entities;

namespace TestDeIa.Application.Modules.Inventario.Ports.Out;

public interface IInventarioRepository
{
    Task<IReadOnlyCollection<Bodega>> GetBodegasAsync(CancellationToken cancellationToken = default);

    Task<Bodega?> GetBodegaByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsBodegaNombreAsync(string nombre, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<Bodega> CreateBodegaAsync(Bodega bodega, CancellationToken cancellationToken = default);

    Task<Bodega?> UpdateBodegaAsync(Bodega bodega, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Producto>> GetProductosAsync(Guid? bodegaId = null, CancellationToken cancellationToken = default);

    Task<PagedResultResponse<Producto>> GetProductosPagedAsync(string? term, int skip, int take, Guid? bodegaId = null, CancellationToken cancellationToken = default);

    Task<Producto?> GetProductoByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsProductoCodigoAsync(string codigo, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<Producto> CreateProductoAsync(Producto producto, decimal stockInicial, decimal costoInicial, CancellationToken cancellationToken = default);

    Task<Producto?> UpdateProductoAsync(Producto producto, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<KardexMovimiento>> GetKardexAsync(Guid productoId, Guid? bodegaId = null, CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarMovimientoAsync(
        Guid productoId,
        Guid? bodegaId,
        string tipoMovimiento,
        string concepto,
        string? referencia,
        decimal cantidad,
        decimal costoUnitario,
        CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarCompraAsync(
        Guid productoId,
        Guid bodegaId,
        decimal cantidad,
        decimal costoUnitarioCompra,
        string? referencia,
        CancellationToken cancellationToken = default);

    Task<Producto?> RegistrarMermaAsync(
        Guid productoId,
        Guid bodegaId,
        decimal cantidad,
        string motivo,
        string? referencia,
        CancellationToken cancellationToken = default);

    Task<Producto?> TransferirStockAsync(
        Guid productoId,
        Guid bodegaOrigenId,
        Guid bodegaDestinoId,
        decimal cantidad,
        string? referencia,
        CancellationToken cancellationToken = default);

    Task DescontarStockPorFacturaAsync(
        Guid facturaId,
        Guid? bodegaId,
        string referenciaFactura,
        string concepto,
        IReadOnlyCollection<(Guid ProductoId, decimal Cantidad)> items,
        CancellationToken cancellationToken = default);
}
