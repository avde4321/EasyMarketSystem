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

    public async Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take, Guid? bodegaId = null)
    {
        var encodedTerm = Uri.EscapeDataString(term);
        var bodegaQuery = bodegaId.HasValue && bodegaId.Value != Guid.Empty ? $"&bodegaId={bodegaId.Value}" : string.Empty;
        return await httpClient.GetFromJsonAsync<PagedResultResponse<PosProductoResponse>>(
            $"api/facturacion/productos?term={encodedTerm}&skip={skip}&take={take}{bodegaQuery}") ?? new PagedResultResponse<PosProductoResponse> { Skip = skip, Take = take };
    }

    public async Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PosPuntoEmisionResponse>>("api/facturacion/puntos-emision")
            ?? Array.Empty<PosPuntoEmisionResponse>();
    }

    public async Task<IReadOnlyCollection<PosOperadorResponse>> GetOperadoresAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PosOperadorResponse>>("api/facturacion/operadores")
            ?? Array.Empty<PosOperadorResponse>();
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

    public async Task<PagedResultResponse<FacturaMonitorResponse>> GetMonitorAsync(string? term, string? tipoDocumentoId, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        var tipoDocumentoQuery = string.IsNullOrWhiteSpace(tipoDocumentoId) ? string.Empty : $"&tipoDocumentoId={Uri.EscapeDataString(tipoDocumentoId)}";
        return await httpClient.GetFromJsonAsync<PagedResultResponse<FacturaMonitorResponse>>($"api/facturacion/monitor?term={encodedTerm}&skip={skip}&take={take}{tipoDocumentoQuery}")
            ?? new PagedResultResponse<FacturaMonitorResponse> { Skip = skip, Take = take };
    }

    public async Task<(bool Succeeded, string? ErrorMessage, NotaCreditoOrigenResponseDto? Data)> GetNotaCreditoOrigenAsync(Guid facturaId)
    {
        var response = await httpClient.GetAsync($"api/facturacion/facturas/{facturaId}/nota-credito-origen");

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<NotaCreditoOrigenResponseDto>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo cargar la factura origen."), null);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, NotaCreditoResponseDto? Data)> CrearNotaCreditoAsync(NotaCreditoRequestDto request)
    {
        var response = await httpClient.PostAsJsonAsync("api/facturacion/notas-credito", request);

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<NotaCreditoResponseDto>());
        }

        return (false, await ReadErrorAsync(response, "No se pudo generar la nota de credito."), null);
    }

    public async Task<LiquidacionComisionResponse> GetLiquidacionComisionesAsync(DateOnly desde, DateOnly hasta, Guid? operadorId = null)
    {
        var operadorQuery = operadorId.HasValue && operadorId.Value != Guid.Empty
            ? $"&operadorId={operadorId.Value}"
            : string.Empty;
        return await httpClient.GetFromJsonAsync<LiquidacionComisionResponse>($"api/facturacion/comisiones/liquidacion?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}{operadorQuery}")
            ?? new LiquidacionComisionResponse { Desde = desde, Hasta = hasta };
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetRidePdfAsync(Guid facturaId)
    {
        return GetFileAsync($"api/reporteria/facturas/{facturaId}/ride");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetComprobanteRidePdfAsync(Guid comprobanteId)
    {
        return GetFileAsync($"api/reporteria/comprobantes/{comprobanteId}/ride");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetXmlGeneradoAsync(Guid facturaId)
    {
        return GetFileAsync($"api/reporteria/facturas/{facturaId}/xml-generado");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetComprobanteXmlGeneradoAsync(Guid comprobanteId)
    {
        return GetFileAsync($"api/reporteria/comprobantes/{comprobanteId}/xml-generado");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetXmlFirmadoAsync(Guid facturaId)
    {
        return GetFileAsync($"api/reporteria/facturas/{facturaId}/xml-firmado");
    }

    public Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)> GetComprobanteXmlFirmadoAsync(Guid comprobanteId)
    {
        return GetFileAsync($"api/reporteria/comprobantes/{comprobanteId}/xml-firmado");
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

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallback)
    {
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
