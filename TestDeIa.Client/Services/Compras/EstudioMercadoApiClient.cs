using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Client.Services.Compras;

public sealed class EstudioMercadoApiClient(HttpClient httpClient)
{
    public async Task<EstudioMercadoCompraResponse> GetAsync(int mes, int anio)
    {
        return await httpClient.GetFromJsonAsync<EstudioMercadoCompraResponse>($"api/compras/estudio-mercado?mes={mes}&anio={anio}")
               ?? new EstudioMercadoCompraResponse();
    }
}
