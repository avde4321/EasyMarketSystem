using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Integraciones;

namespace TestDeIa.Client.Services.Integraciones;

public sealed class WhatsAppApiClient(HttpClient httpClient)
{
    public async Task<(bool Succeeded, string? ErrorMessage, WhatsAppEnvioResponseDto? Data)> EnviarComprobanteAsync(EnviarWhatsAppDto request)
    {
        var response = await httpClient.PostAsJsonAsync("api/whatsapp/enviar-comprobante", request);
        return await ReadResponseAsync<WhatsAppEnvioResponseDto>(response, "No se pudo enviar el comprobante por WhatsApp.");
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
