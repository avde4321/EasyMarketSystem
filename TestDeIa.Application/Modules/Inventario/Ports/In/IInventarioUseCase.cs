using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Application.Modules.Inventario.Ports.In;

public interface IInventarioUseCase
{
    Task<IReadOnlyCollection<BodegaResponse>> GetBodegasAsync(CancellationToken cancellationToken = default);

    Task<BodegaResponse?> GetBodegaByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BodegaResponse> CreateBodegaAsync(BodegaRequest request, CancellationToken cancellationToken = default);

    Task<BodegaResponse?> UpdateBodegaAsync(Guid id, BodegaRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProductoResponse>> GetCatalogoAsync(Guid? bodegaId = null, CancellationToken cancellationToken = default);

    Task<PagedResultResponse<ProductoResponse>> GetCatalogoPagedAsync(string? term, int skip, int take, Guid? bodegaId = null, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> GetProductoByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductoResponse> CreateProductoAsync(ProductoRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> UpdateProductoAsync(Guid id, ProductoRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<KardexMovimientoResponse>> GetKardexAsync(Guid productoId, Guid? bodegaId = null, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> AjustarStockAsync(Guid productoId, AjusteStockRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> RegistrarCompraAsync(IngresoCompraRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> RegistrarMermaAsync(EgresoMermaRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> TransferirStockAsync(TransferenciaInventarioRequest request, CancellationToken cancellationToken = default);

    Task DescontarStockPorFacturaAsync(DescontarStockFacturaRequest request, CancellationToken cancellationToken = default);
}
