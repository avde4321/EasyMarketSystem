using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Integraciones;
using TestDeIa.Shared.Responses.Integraciones;

namespace TestDeIa.Client.Services.Integraciones;

public sealed class PagosDigitalesApiClient(HttpClient httpClient)
{
    public async Task<(bool Succeeded, string? ErrorMessage, RespuestaPagoDigitalDto? Data)> GenerarLinkAsync(GenerarLinkPagoDto request)
    {
        var response = await httpClient.PostAsJsonAsync("api/pagos-digitales/generar-link", request);
        return await ReadResponseAsync<RespuestaPagoDigitalDto>(response, "No se pudo generar el QR de cobro.");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, RespuestaPagoDigitalDto? Data)> ConsultarEstadoAsync(string transactionId)
    {
        var response = await httpClient.GetAsync($"api/pagos-digitales/{Uri.EscapeDataString(transactionId)}/estado");
        return await ReadResponseAsync<RespuestaPagoDigitalDto>(response, "No se pudo consultar el estado del pago.");
    }

    private static async Task<(bool Succeeded, string? ErrorMessage, T? Data)> ReadResponseAsync<T>(HttpResponseMessage response, string fallback)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<T>());
        }

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? fallback, default);
        }
        catch
        {
            return (false, fallback, default);
        }
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
