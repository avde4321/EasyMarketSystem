using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Compras;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Client.Services.Compras;

public sealed class ComprasApiClient
{
    private readonly HttpClient httpClient;

    public ComprasApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CompraResponse? Data)> RegistrarAsync(RegistrarCompraRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/compras", request);

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<CompraResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo registrar la compra.", null);
        }

        return (false, "No se pudo registrar la compra.", null);
    }

    public async Task<(bool Succeeded, string? ErrorMessage, ReporteComprasConsolidadoResponse? Data)> GetReporteFisicoFinancieroAsync(
        DateTime fechaInicio,
        DateTime fechaFin,
        NaturalezaCompra? naturalezaCompra)
    {
        var url = $"api/compras/reporte-fisico-financiero?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";

        if (naturalezaCompra.HasValue)
        {
            url += $"&naturalezaCompra={naturalezaCompra.Value}";
        }

        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<ReporteComprasConsolidadoResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo consultar el reporte consolidado.", null);
        }

        return (false, "No se pudo consultar el reporte consolidado.", null);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
