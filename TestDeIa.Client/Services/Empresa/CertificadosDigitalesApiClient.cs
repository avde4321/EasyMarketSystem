using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Client.Services.Empresa;

public sealed class CertificadosDigitalesApiClient(HttpClient httpClient)
{
    public async Task<PagedResultResponse<CertificadoDigitalEmpresaResponse>> GetPagedAsync(
        string? term,
        Guid? empresaId,
        bool soloAlertas,
        int skip,
        int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        var empresaQuery = empresaId.HasValue ? $"&empresaId={empresaId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<PagedResultResponse<CertificadoDigitalEmpresaResponse>>(
            $"api/certificados-digitales?term={encodedTerm}{empresaQuery}&soloAlertas={soloAlertas}&skip={skip}&take={take}")
            ?? new PagedResultResponse<CertificadoDigitalEmpresaResponse> { Skip = skip, Take = take };
    }

    public async Task<IReadOnlyCollection<CertificadoDigitalEmpresaResponse>> GetAlertasAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<CertificadoDigitalEmpresaResponse>>("api/certificados-digitales/alertas")
            ?? Array.Empty<CertificadoDigitalEmpresaResponse>();
    }

    public Task<(bool Succeeded, string? ErrorMessage, CertificadoDigitalEmpresaResponse? Data)> CreateAsync(CertificadoDigitalEmpresaRequest request)
    {
        return SendAsync(() => httpClient.PostAsJsonAsync("api/certificados-digitales", request), "No se pudo guardar el certificado.");
    }

    public Task<(bool Succeeded, string? ErrorMessage, CertificadoDigitalEmpresaResponse? Data)> ActivarAsync(Guid id)
    {
        return SendAsync(() => httpClient.PostAsync($"api/certificados-digitales/{id}/activar", null), "No se pudo activar el certificado.");
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> DesactivarAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/certificados-digitales/{id}");
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await ReadErrorAsync(response, "No se pudo desactivar el certificado."));
    }

    private static async Task<(bool Succeeded, string? ErrorMessage, CertificadoDigitalEmpresaResponse? Data)> SendAsync(
        Func<Task<HttpResponseMessage>> send,
        string fallbackMessage)
    {
        var response = await send();
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<CertificadoDigitalEmpresaResponse>());
        }

        return (false, await ReadErrorAsync(response, fallbackMessage), null);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallbackMessage)
    {
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return error?.Message ?? fallbackMessage;
        }

        return fallbackMessage;
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
