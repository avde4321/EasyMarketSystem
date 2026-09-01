using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Retenciones;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Retenciones;

namespace TestDeIa.Client.Services.Retenciones;

public sealed class RetencionesApiClient(HttpClient httpClient)
{
    public async Task<PagedResultResponse<ComprobanteRetencionResponse>> GetPagedAsync(string? term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<ComprobanteRetencionResponse>>(
            $"api/retenciones?term={encodedTerm}&skip={skip}&take={take}")
            ?? new PagedResultResponse<ComprobanteRetencionResponse> { Skip = skip, Take = take };
    }

    public async Task<ComprobanteRetencionResponse?> GetByIdAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<ComprobanteRetencionResponse>($"api/retenciones/{id}");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ComprobanteRetencionResponse? Data)> CreateAsync(ComprobanteRetencionRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/retenciones", request);
        return await ReadResponseAsync<ComprobanteRetencionResponse>(response, "No se pudo guardar la retencion.");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ComprobanteRetencionResponse? Data)> CrearDesdeCompraAsync(Guid compraId)
    {
        var response = await httpClient.PostAsync($"api/retenciones/desde-compra/{compraId}", null);
        return await ReadResponseAsync<ComprobanteRetencionResponse>(response, "No se pudo generar la retencion desde la compra.");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ComprobanteRetencionResponse? Data)> ProcesarSriAsync(Guid id)
    {
        var response = await httpClient.PostAsync($"api/retenciones/{id}/procesar-sri", null);
        return await ReadResponseAsync<ComprobanteRetencionResponse>(response, "No se pudo procesar la retencion en el SRI.");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ComprobanteRetencionResponse? Data)> ReintentarAutorizacionAsync(Guid id)
    {
        var response = await httpClient.PostAsync($"api/retenciones/{id}/reintentar-autorizacion", null);
        return await ReadResponseAsync<ComprobanteRetencionResponse>(response, "No se pudo reintentar la autorizacion.");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetRidePdfAsync(Guid id)
    {
        return GetFileAsync($"api/reporteria/retenciones/{id}/ride");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetXmlGeneradoAsync(Guid id)
    {
        return GetFileAsync($"api/reporteria/retenciones/{id}/xml-generado");
    }

    private async Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetFileAsync(string url)
    {
        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadAsByteArrayAsync());
        }

        return (false, await ReadErrorAsync(response, "No se pudo descargar el documento."), null);
    }

    private static async Task<(bool Succeeded, string? ErrorMessage, T? Data)> ReadResponseAsync<T>(HttpResponseMessage response, string fallback)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<T>());
        }

        return (false, await ReadErrorAsync(response, fallback), default);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallback)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return "No se encontro el registro solicitado.";
        }

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return error?.Message ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
