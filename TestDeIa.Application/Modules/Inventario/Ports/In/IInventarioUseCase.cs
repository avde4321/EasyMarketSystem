using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Application.Modules.Inventario.Ports.In;

public interface IInventarioUseCase
{
    Task<IReadOnlyCollection<ProductoResponse>> GetCatalogoAsync(CancellationToken cancellationToken = default);

    Task<ProductoResponse?> GetProductoByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductoResponse> CreateProductoAsync(ProductoRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> UpdateProductoAsync(Guid id, ProductoRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<KardexMovimientoResponse>> GetKardexAsync(Guid productoId, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> AjustarStockAsync(Guid productoId, AjusteStockRequest request, CancellationToken cancellationToken = default);

    Task DescontarStockPorFacturaAsync(DescontarStockFacturaRequest request, CancellationToken cancellationToken = default);
}
