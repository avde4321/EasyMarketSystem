using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Client.Services.Facturacion;

public sealed class FacturacionApiClient
{
    private readonly HttpClient httpClient;

    public FacturacionApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<PagedResultResponse<PosClienteResponse>> SearchClientesAsync(string term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<PosClienteResponse>>(
            $"api/facturacion/clientes?term={encodedTerm}&skip={skip}&take={take}") ?? new PagedResultResponse<PosClienteResponse> { Skip = skip, Take = take };
    }

    public async Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<PosProductoResponse>>(
            $"api/facturacion/productos?term={encodedTerm}&skip={skip}&take={take}") ?? new PagedResultResponse<PosProductoResponse> { Skip = skip, Take = take };
    }

    public async Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PosPuntoEmisionResponse>>("api/facturacion/puntos-emision")
            ?? Array.Empty<PosPuntoEmisionResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage, FacturaEmissionResponse? Data)> EmitirFacturaAsync(EmitirFacturaRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/facturacion/facturas", request);

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<FacturaEmissionResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo emitir la factura.", null);
        }

        return (false, "No se pudo emitir la factura.", null);
    }

    public async Task<IReadOnlyCollection<FacturaMonitorResponse>> GetMonitorAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<FacturaMonitorResponse>>("api/facturacion/monitor")
            ?? Array.Empty<FacturaMonitorResponse>();
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetRidePdfAsync(Guid facturaId)
    {
        return GetFileAsync($"api/reporteria/facturas/{facturaId}/ride");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetXmlGeneradoAsync(Guid facturaId)
    {
        return GetFileAsync($"api/reporteria/facturas/{facturaId}/xml-generado");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetXmlFirmadoAsync(Guid facturaId)
    {
        return GetFileAsync($"api/reporteria/facturas/{facturaId}/xml-firmado");
    }

    private async Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetFileAsync(string url)
    {
        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadAsByteArrayAsync());
        }

        var errorMessage = "No se pudo descargar el documento.";

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            errorMessage = error?.Message ?? errorMessage;
        }
        catch
        {
        }

        return (false, errorMessage, null);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
