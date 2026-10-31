using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Sri;

namespace TestDeIa.Client.Services.Sri;

public sealed class SriCatalogosApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<SriCatalogoErrorResponse>> GetErroresAsync(string? term = null)
    {
        var url = string.IsNullOrWhiteSpace(term)
            ? "api/sri/catalogos/errores"
            : $"api/sri/catalogos/errores?term={Uri.EscapeDataString(term.Trim())}";

        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<SriCatalogoErrorResponse>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SriCatalogoErrorResponse>>()
            ?? Array.Empty<SriCatalogoErrorResponse>();
    }

    public async Task<IReadOnlyCollection<SriEstadoComprobanteResponse>> GetEstadosAsync()
    {
        var response = await httpClient.GetAsync("api/sri/catalogos/estados-comprobante");
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<SriEstadoComprobanteResponse>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SriEstadoComprobanteResponse>>()
            ?? Array.Empty<SriEstadoComprobanteResponse>();
    }
}
