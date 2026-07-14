using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Client.Services.Contabilidad;

public sealed class ContabilidadApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<CuentaContableResponse>> GetPlanCuentasAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<CuentaContableResponse>>("api/contabilidad/plan-cuentas")
            ?? Array.Empty<CuentaContableResponse>();
    }
}