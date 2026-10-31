using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Shared.Responses.OfflinePos;

public sealed class PosOfflineCatalogoCacheResponse
{
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    public IReadOnlyCollection<StockLocalCacheDto> Productos { get; set; } = Array.Empty<StockLocalCacheDto>();

    public IReadOnlyCollection<PosClienteResponse> Clientes { get; set; } = Array.Empty<PosClienteResponse>();
}
