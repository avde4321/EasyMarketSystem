using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Integraciones;

namespace TestDeIa.Client.Services.Integraciones;

public sealed class WhatsAppApiClient(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<(bool Succeeded, string? ErrorMessage, WhatsAppEnvioResponseDto? Data)> EnviarComprobanteAsync(EnviarWhatsAppDto request)
    {
        if (!TryResolveLink(request.LinkPdfRide, out var pdf) || !TryResolveLink(request.LinkXml, out var xml))
            return (false, "Configura Integraciones:PublicDocumentBaseUrl con la URL pública de documentos antes de enviar por WhatsApp. Las rutas locales de desarrollo no son accesibles para el cliente.", null);
        request.LinkPdfRide = pdf;
        request.LinkXml = xml;
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
            var validation = error?.Errors is null ? null : string.Join(" ", error.Errors.Values.SelectMany(values => values));
            return (false, error?.Message ?? error?.Detail ?? validation ?? error?.Title ?? fallback, default);
        }
        catch
        {
            return (false, fallback, default);
        }
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
        public string? Detail { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    private bool TryResolveLink(string? value, out string? resolved)
    {
        resolved = value;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute) && absolute.Scheme is "http" or "https")
            return !absolute.IsLoopback;
        var baseUrl = configuration["Integraciones:PublicDocumentBaseUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || baseUri.IsLoopback || baseUri.Scheme != "https") return false;
        resolved = new Uri(baseUri, value.TrimStart('/')).AbsoluteUri;
        return true;
    }
}
