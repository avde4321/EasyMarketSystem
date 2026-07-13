using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Financiero;

namespace TestDeIa.Client.Services.Financiero;

public sealed class FinancieroApiClient(HttpClient httpClient)
{
    public async Task<ConsolidadoIvaMensualResponse> GetConsolidadoIvaAsync(int mes, int anio)
    {
        return await httpClient.GetFromJsonAsync<ConsolidadoIvaMensualResponse>(
                   $"api/financiero/reportes/iva-mensual?mes={mes}&anio={anio}")
               ?? new ConsolidadoIvaMensualResponse();
    }
}
