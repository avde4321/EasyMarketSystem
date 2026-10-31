using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Geografia;

namespace TestDeIa.Client.Services.Geografia;

public sealed class GeografiaApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<GeoItemResponse>> GetRegionesAsync()
    {
        return await GetAsync("api/geografia/regiones");
    }

    public async Task<IReadOnlyCollection<GeoItemResponse>> GetProvinciasAsync()
    {
        return await GetAsync("api/geografia/provincias");
    }

    public async Task<IReadOnlyCollection<GeoItemResponse>> GetCiudadesAsync()
    {
        return await GetAsync("api/geografia/ciudades");
    }

    public async Task<IReadOnlyCollection<GeoItemResponse>> GetSectoresAsync()
    {
        return await GetAsync("api/geografia/sectores");
    }

    private async Task<IReadOnlyCollection<GeoItemResponse>> GetAsync(string url)
    {
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<GeoItemResponse>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<GeoItemResponse>>()
            ?? Array.Empty<GeoItemResponse>();
    }
}
