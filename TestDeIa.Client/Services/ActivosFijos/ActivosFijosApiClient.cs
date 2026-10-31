using System.Net.Http.Json;
using TestDeIa.Shared.ActivosFijos;
using TestDeIa.Shared.Requests.ActivosFijos;
using TestDeIa.Shared.Responses.ActivosFijos;

namespace TestDeIa.Client.Services.ActivosFijos;

public sealed class ActivosFijosApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<ActivoFijoResponse>> GetAsync(CategoriaSriActivoFijo? categoria, string? custodio, EstadoActivoFijo? estado)
    {
        var query = new List<string>();
        if (categoria.HasValue)
        {
            query.Add($"categoria={categoria.Value}");
        }

        if (!string.IsNullOrWhiteSpace(custodio))
        {
            query.Add($"custodio={Uri.EscapeDataString(custodio.Trim())}");
        }

        if (estado.HasValue)
        {
            query.Add($"estado={estado.Value}");
        }

        var url = query.Count == 0 ? "api/activosfijos" : $"api/activosfijos?{string.Join("&", query)}";
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<ActivoFijoResponse>>(url) ?? Array.Empty<ActivoFijoResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ActivoFijoResponse? Data)> CreateAsync(ActivoFijoRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/activosfijos", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ActivoFijoResponse? Data)> UpdateAsync(Guid id, ActivoFijoRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/activosfijos/{id}", request);
        return await BuildResultAsync(response);
    }

    private static async Task<(bool Succeeded, string? ErrorMessage, ActivoFijoResponse? Data)> BuildResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<ActivoFijoResponse>());
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        return (false, error?.Message ?? "No se pudo procesar el activo fijo.", null);
    }

    private sealed class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
