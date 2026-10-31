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

    Task<IReadOnlyCollection<StockDisponibleBodegaResponse>> GetDisponibilidadEnOtrasBodegasAsync(
        Guid productoId,
        Guid? bodegaActualId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockAlertaResponse>> GetAlertasStockAsync(CancellationToken cancellationToken = default);

    Task<ProductoResponse?> AjustarStockAsync(Guid productoId, AjusteStockRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> RegistrarCompraAsync(IngresoCompraRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> RegistrarMermaAsync(EgresoMermaRequest request, CancellationToken cancellationToken = default);

    Task<ProductoResponse?> TransferirStockAsync(TransferenciaInventarioRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TransferenciaInventarioResponse>> GetTransferenciasAsync(
        string? estado = null,
        Guid? bodegaOrigenId = null,
        Guid? bodegaDestinoId = null,
        CancellationToken cancellationToken = default);

    Task<TransferenciaInventarioResponse?> GetTransferenciaByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransferenciaInventarioResponse> CreateTransferenciaAsync(
        TransferenciaInventarioFormalRequest request,
        CancellationToken cancellationToken = default);

    Task<TransferenciaInventarioResponse?> DespacharTransferenciaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransferenciaInventarioResponse?> RecibirTransferenciaAsync(
        Guid id,
        RecepcionTransferenciaInventarioRequest request,
        CancellationToken cancellationToken = default);

    Task<TomaFisicaResultadoResponse> ProcesarTomaFisicaAsync(TomaFisicaInventarioRequest request, CancellationToken cancellationToken = default);

    Task DescontarStockPorFacturaAsync(DescontarStockFacturaRequest request, CancellationToken cancellationToken = default);
}
