using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Client.Services.Contabilidad;

public sealed class ContabilidadApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<CuentaContableResponse>> GetPlanCuentasAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<CuentaContableResponse>>("api/contabilidad/plan-cuentas")
            ?? Array.Empty<CuentaContableResponse>();
    }

    public async Task<IReadOnlyCollection<CuentaContableResponse>> GetCuentasAceptablesAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<CuentaContableResponse>>("api/contabilidad/cuentas-aceptables")
            ?? Array.Empty<CuentaContableResponse>();
    }

    public async Task<IReadOnlyCollection<AsientoContableResponse>> GetLibroDiarioAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<AsientoContableResponse>>("api/contabilidad/libro-diario")
            ?? Array.Empty<AsientoContableResponse>();
    }

    public async Task<string> CrearAsientoAsync(CrearAsientoRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/contabilidad/asientos-manuales", request);
        var payload = await response.Content.ReadFromJsonAsync<NumeroAsientoPayload>();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(payload?.Message ?? "No se pudo registrar el asiento contable manual.");
        }

        return payload?.NumeroAsiento ?? string.Empty;
    }

    private sealed class NumeroAsientoPayload
    {
        public string NumeroAsiento { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
