using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Services.Inventario;

public sealed class InventarioApiClient
{
    private readonly HttpClient httpClient;

    public InventarioApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<BodegaResponse>> GetBodegasAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<BodegaResponse>>("api/inventario/bodegas")
            ?? Array.Empty<BodegaResponse>();
    }

    public async Task<PagedResultResponse<ProductoResponse>> GetProductosAsync(string? term, int skip, int take, Guid? bodegaId = null)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        var bodegaQuery = bodegaId.HasValue && bodegaId.Value != Guid.Empty ? $"&bodegaId={bodegaId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<PagedResultResponse<ProductoResponse>>($"api/inventario/productos?term={encodedTerm}&skip={skip}&take={take}{bodegaQuery}")
            ?? new PagedResultResponse<ProductoResponse> { Skip = skip, Take = take };
    }

    public async Task<IReadOnlyCollection<KardexMovimientoResponse>> GetKardexAsync(Guid productoId, Guid? bodegaId = null)
    {
        var bodegaQuery = bodegaId.HasValue && bodegaId.Value != Guid.Empty ? $"?bodegaId={bodegaId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<KardexMovimientoResponse>>(
            $"api/inventario/productos/{productoId}/kardex{bodegaQuery}") ?? Array.Empty<KardexMovimientoResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateProductoAsync(ProductoRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/productos", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateProductoAsync(Guid id, ProductoRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/inventario/productos/{id}", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> AjustarStockAsync(Guid productoId, AjusteStockRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/inventario/productos/{productoId}/ajustes", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> RegistrarMermaAsync(EgresoMermaRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/movimientos/merma", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> TransferirStockAsync(TransferenciaInventarioRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/inventario/movimientos/transferencia", request);
        return await BuildResultAsync(response);
    }

    private static async Task<(bool Succeeded, string? ErrorMessage)> BuildResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo completar la operacion.");
        }

        return (false, "No se pudo completar la operacion.");
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
