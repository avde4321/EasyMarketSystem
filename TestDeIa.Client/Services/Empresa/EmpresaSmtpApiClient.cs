using System.Net.Http.Json;
using System.Text.Json;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Client.Services.Empresa;

public sealed class EmpresaSmtpApiClient(HttpClient httpClient)
{
    public async Task<EmpresaSmtpConfiguracionResponse?> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/empresa/smtp", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(await ReadErrorAsync(response, "No se pudo consultar la configuración SMTP."), null, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<EmpresaSmtpConfiguracionResponse>(cancellationToken: cancellationToken);
    }

    public async Task GuardarAsync(EmpresaSmtpConfiguracionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync("api/empresa/smtp", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(await ReadErrorAsync(response, "No se pudo guardar la configuración SMTP."), null, response.StatusCode);
    }

    public async Task ProbarAsync(string destinatario, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/empresa/smtp/probar", new EmpresaSmtpPruebaRequest { Destinatario = destinatario }, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(await ReadErrorAsync(response, "No se pudo enviar el correo de prueba."), null, response.StatusCode);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallback)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
            return fallback;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            foreach (var propertyName in new[] { "detail", "message", "title" })
            {
                if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
                    return value.GetString() ?? fallback;
            }
        }
        catch (JsonException)
        {
            // Respuestas no JSON se sustituyen por un mensaje de UI controlado.
        }

        return fallback;
    }
}
