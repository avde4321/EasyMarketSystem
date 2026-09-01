using TestDeIa.Shared.Requests.OfflinePos;
using TestDeIa.Shared.Responses.OfflinePos;

namespace TestDeIa.Application.Modules.OfflinePos.Ports.In;

public interface IOfflineSyncService
{
    Task<OfflineSyncResultResponse> SincronizarVentasOfflineAsync(
        IReadOnlyCollection<VentaOfflineQueueDto> ventas,
        CancellationToken cancellationToken = default);

    Task<PosOfflineCatalogoCacheResponse> ObtenerCatalogoPosOfflineAsync(
        Guid? bodegaId = null,
        CancellationToken cancellationToken = default);
}
