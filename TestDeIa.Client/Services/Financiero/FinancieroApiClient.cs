using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Financiero;

namespace TestDeIa.Client.Services.Financiero;

public sealed class FinancieroApiClient(HttpClient httpClient)
{
    public async Task<ConsolidadoIvaMensualResponse> GetConsolidadoIvaAsync(int mes, int anio, string? puntoEmision = null, string? cajero = null)
    {
        var url = $"api/financiero/reportes/iva-mensual?mes={mes}&anio={anio}";

        if (!string.IsNullOrWhiteSpace(puntoEmision))
        {
            url += $"&puntoEmision={Uri.EscapeDataString(puntoEmision.Trim())}";
        }

        if (!string.IsNullOrWhiteSpace(cajero))
        {
            url += $"&cajero={Uri.EscapeDataString(cajero.Trim())}";
        }

        return await httpClient.GetFromJsonAsync<ConsolidadoIvaMensualResponse>(url)
               ?? new ConsolidadoIvaMensualResponse();
    }
}
