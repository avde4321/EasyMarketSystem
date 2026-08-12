using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Client.Services.Reporteria;

public sealed class ReporteriaVentasApiClient(HttpClient httpClient)
{
    public async Task<(bool Succeeded, string? ErrorMessage, ReporteVentasResponse? Data)> GetVentasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var response = await httpClient.GetAsync($"api/reporteria/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<ReporteVentasResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo consultar el reporte de ventas.", null);
        }

        return (false, "No se pudo consultar el reporte de ventas.", null);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
