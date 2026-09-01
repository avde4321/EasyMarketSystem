using System.Text.Json;
using Microsoft.JSInterop;
using TestDeIa.Shared.Requests.OfflinePos;
using TestDeIa.Shared.Responses.OfflinePos;

namespace TestDeIa.Client.Services.OfflinePos;

public sealed class OfflinePosStorageService(IJSRuntime jsRuntime)
{
    private const string VentaQueueKey = "easymarket.pos.offline.ventas";
    private const string StockCacheKey = "easymarket.pos.offline.stock";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyCollection<VentaOfflineQueueDto>> GetVentasPendientesAsync()
    {
        return await GetAsync<List<VentaOfflineQueueDto>>(VentaQueueKey) ?? [];
    }

    public async Task EnqueueVentaAsync(VentaOfflineQueueDto venta)
    {
        var ventas = (await GetVentasPendientesAsync()).ToList();
        ventas.RemoveAll(current => current.LocalQueueId == venta.LocalQueueId);
        ventas.Add(venta);
        await SetAsync(VentaQueueKey, ventas);
    }

    public async Task RemoveVentaAsync(Guid localQueueId)
    {
        var ventas = (await GetVentasPendientesAsync()).Where(current => current.LocalQueueId != localQueueId).ToArray();
        await SetAsync(VentaQueueKey, ventas);
    }

    public async Task<IReadOnlyCollection<StockLocalCacheDto>> GetStockCacheAsync(Guid? bodegaId = null)
    {
        var cache = await GetAsync<List<StockLocalCacheDto>>(StockCacheKey) ?? [];
        return bodegaId.HasValue && bodegaId.Value != Guid.Empty
            ? cache.Where(current => current.BodegaId == bodegaId.Value).ToArray()
            : cache;
    }

    public Task SaveStockCacheAsync(IReadOnlyCollection<StockLocalCacheDto> cache)
    {
        return SetAsync(StockCacheKey, cache);
    }

    public async Task ClearAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", VentaQueueKey);
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StockCacheKey);
    }

    private async Task<T?> GetAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        return string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    private async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }
}
