using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Proformas;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Proformas;

namespace TestDeIa.Client.Services.Proformas;

public sealed class ProformasApiClient(HttpClient httpClient)
{
    public async Task<PagedResultResponse<ProformaResponse>> GetPagedAsync(string? term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<ProformaResponse>>(
            $"api/proformas?term={encodedTerm}&skip={skip}&take={take}")
            ?? new PagedResultResponse<ProformaResponse> { Skip = skip, Take = take };
    }

    public async Task<ProformaResponse?> GetByIdAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<ProformaResponse>($"api/proformas/{id}");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ProformaResponse? Data)> CreateAsync(ProformaRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/proformas", request);
        return await ReadResponseAsync<ProformaResponse>(response, "No se pudo guardar la proforma.");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ProformaResponse? Data)> UpdateAsync(Guid id, ProformaRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/proformas/{id}", request);
        return await ReadResponseAsync<ProformaResponse>(response, "No se pudo actualizar la proforma.");
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> AnularAsync(Guid id)
    {
        var response = await httpClient.PostAsync($"api/proformas/{id}/anular", null);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await ReadErrorAsync(response, "No se pudo anular la proforma."));
    }

    public async Task<(bool Succeeded, string? ErrorMessage, Guid? FacturaId)> ConvertirAFacturaAsync(Guid id, ConvertirProformaAFacturaDto request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/proformas/{id}/convertir-a-factura", request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<Guid>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo convertir la proforma a factura."), null);
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetRidePdfAsync(Guid id)
    {
        return GetFileAsync($"api/reporteria/proformas/{id}/ride");
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
