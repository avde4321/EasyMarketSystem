using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Catalogos;
using TestDeIa.Shared.Responses.Catalogos;

namespace TestDeIa.Client.Services.Catalogos;

public sealed class CatalogosApiClient
{
    private readonly HttpClient httpClient;

    public CatalogosApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<CatalogoResponse>> GetCatalogosAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogoResponse>>("api/catalogos")
            ?? Array.Empty<CatalogoResponse>();
    }

    public async Task<IReadOnlyCollection<CatalogoItemResponse>> GetItemsAsync(string codigoCatalogo, bool onlyActive = false)
    {
        var codigo = Uri.EscapeDataString(codigoCatalogo);
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogoItemResponse>>($"api/catalogos/{codigo}/items?onlyActive={onlyActive.ToString().ToLowerInvariant()}")
            ?? Array.Empty<CatalogoItemResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CatalogoItemResponse? Data)> CreateItemAsync(CatalogoItemRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/catalogos/items", request);
        return await ParseResponseAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CatalogoItemResponse? Data)> UpdateItemAsync(Guid id, CatalogoItemRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/catalogos/items/{id}", request);
        return await ParseResponseAsync(response);
    }

    private static async Task<(bool Succeeded, string? ErrorMessage, CatalogoItemResponse? Data)> ParseResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<CatalogoItemResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo guardar el item del catalogo.", null);
        }

        return (false, "No se pudo guardar el item del catalogo.", null);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
