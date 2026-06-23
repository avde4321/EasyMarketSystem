using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Client.Services.Facturacion;

public sealed class FacturacionApiClient
{
    private readonly HttpClient httpClient;

    public FacturacionApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<PosClienteResponse>> SearchClientesAsync(string term)
    {
        var encodedTerm = Uri.EscapeDataString(term);
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PosClienteResponse>>(
            $"api/facturacion/clientes?term={encodedTerm}") ?? Array.Empty<PosClienteResponse>();
    }

    public async Task<IReadOnlyCollection<PosProductoResponse>> SearchProductosAsync(string term)
    {
        var encodedTerm = Uri.EscapeDataString(term);
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PosProductoResponse>>(
            $"api/facturacion/productos?term={encodedTerm}") ?? Array.Empty<PosProductoResponse>();
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

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
