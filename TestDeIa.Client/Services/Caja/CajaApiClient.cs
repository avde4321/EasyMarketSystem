using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Caja;
using TestDeIa.Shared.Responses.Caja;

namespace TestDeIa.Client.Services.Caja;

public sealed class CajaApiClient(HttpClient httpClient)
{
    public async Task<CajaSesionResponse?> GetActivaAsync()
    {
        var response = await httpClient.GetAsync("api/caja/activa");
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CajaSesionResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CajaSesionResponse? Data)> AbrirAsync(AbrirCajaRequest request)
    {
        return await SendAsync("api/caja/abrir", request);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CajaSesionResponse? Data)> CerrarAsync(CerrarCajaRequest request)
    {
        return await SendAsync("api/caja/cerrar", request);
    }

    private async Task<(bool Succeeded, string? ErrorMessage, CajaSesionResponse? Data)> SendAsync<TRequest>(string url, TRequest request)
    {
        var response = await httpClient.PostAsJsonAsync(url, request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<CajaSesionResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo procesar la caja.", null);
        }

        return (false, "No se pudo procesar la caja.", null);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
