using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.XmlSri;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Responses.XmlSri;

namespace TestDeIa.Client.Services.XmlSri;

public sealed class XmlComprasApiClient(HttpClient httpClient)
{
    public async Task<XmlSriCargaMasivaResponse> CargarMasivoAsync(IReadOnlyCollection<BrowserFilePayload> files)
    {
        using var content = new MultipartFormDataContent();
        foreach (var file in files)
        {
            content.Add(new ByteArrayContent(file.Content), "archivos", file.FileName);
        }

        var response = await httpClient.PostAsync("api/xml-compras/cargar-masivo", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<XmlSriCargaMasivaResponse>() ?? new XmlSriCargaMasivaResponse();
    }

    public async Task<PagedResultResponse<FacturaCompraXmlLogResponse>> GetPendientesAsync(int skip, int take)
    {
        return await httpClient.GetFromJsonAsync<PagedResultResponse<FacturaCompraXmlLogResponse>>($"api/xml-compras/pendientes?skip={skip}&take={take}")
            ?? new PagedResultResponse<FacturaCompraXmlLogResponse> { Skip = skip, Take = take };
    }

    public async Task<XmlSriParserResultDto?> GetParseadoAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<XmlSriParserResultDto>($"api/xml-compras/{id}/parseado");
    }

    public async Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetXmlOriginalAsync(Guid id)
    {
        var response = await httpClient.GetAsync($"api/xml-compras/{id}/xml-original");
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadAsByteArrayAsync());
        }

        return (false, await ReadErrorAsync(response, "No se pudo descargar el XML original."), null);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CompraResponse? Data)> ConvertirCompraAsync(Guid id, ConvertirXmlACompraRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/xml-compras/{id}/convertir-compra", request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<CompraResponse>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo convertir el XML a compra."), null);
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

public sealed record BrowserFilePayload(string FileName, byte[] Content);
