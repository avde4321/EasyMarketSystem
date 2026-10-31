using System.Net.Http.Json;
using TestDeIa.Shared.Requests.OfflinePos;
using TestDeIa.Shared.Responses.OfflinePos;

namespace TestDeIa.Client.Services.OfflinePos;

public sealed class PosOfflineApiClient(HttpClient httpClient)
{
    public async Task<PosOfflineCatalogoCacheResponse> GetCatalogoCacheAsync(Guid? bodegaId)
    {
        var bodegaQuery = bodegaId.HasValue && bodegaId.Value != Guid.Empty ? $"?bodegaId={bodegaId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<PosOfflineCatalogoCacheResponse>($"api/pos-offline/catalogo-cache{bodegaQuery}")
            ?? new PosOfflineCatalogoCacheResponse();
    }

    public async Task<OfflineSyncResultResponse> SincronizarAsync(IReadOnlyCollection<VentaOfflineQueueDto> ventas)
    {
        var response = await httpClient.PostAsJsonAsync("api/pos-offline/sincronizar", ventas);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfflineSyncResultResponse>()
            ?? new OfflineSyncResultResponse { Recibidas = ventas.Count };
    }
}
