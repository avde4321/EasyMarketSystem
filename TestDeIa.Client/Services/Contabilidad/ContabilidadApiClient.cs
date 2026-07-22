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

    public async Task<IReadOnlyCollection<LibroDiarioLineaResponse>> GetLibroDiarioAsync(DateTime desde, DateTime hasta)
    {
        var url = $"api/contabilidad/libros/diario?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<LibroDiarioLineaResponse>>(url)
            ?? Array.Empty<LibroDiarioLineaResponse>();
    }

    public async Task<LibroMayorResponse> GetLibroMayorAsync(Guid cuentaContableId, DateTime desde, DateTime hasta)
    {
        var url = $"api/contabilidad/libros/mayor?cuentaContableId={cuentaContableId}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        return await httpClient.GetFromJsonAsync<LibroMayorResponse>(url)
            ?? new LibroMayorResponse();
    }

    public async Task<BalanceGeneralResponse> GetBalanceGeneralAsync()
    {
        return await httpClient.GetFromJsonAsync<BalanceGeneralResponse>("api/contabilidad/estados/balance-general")
            ?? new BalanceGeneralResponse();
    }

    public async Task<EstadoResultadosResponse> GetEstadoResultadosAsync()
    {
        return await httpClient.GetFromJsonAsync<EstadoResultadosResponse>("api/contabilidad/estados/estado-resultados")
            ?? new EstadoResultadosResponse();
    }

    public async Task<IReadOnlyCollection<PeriodoContableResponse>> GetPeriodosAsync(int anio)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PeriodoContableResponse>>($"api/contabilidad/periodos?anio={anio}")
            ?? Array.Empty<PeriodoContableResponse>();
    }

    public async Task<PeriodoContableResponse> CerrarPeriodoFiscalAsync(CerrarPeriodoFiscalRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/contabilidad/periodos/cerrar", request);
        if (!response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorPayload>();
            throw new HttpRequestException(payload?.Message ?? "No se pudo cerrar el periodo fiscal.");
        }

        return await response.Content.ReadFromJsonAsync<PeriodoContableResponse>()
            ?? new PeriodoContableResponse();
    }

    public async Task<AjusteInventarioContableResponse> AjustarInventarioContableAsync(bool generarAsiento)
    {
        var response = await httpClient.PostAsync($"api/contabilidad/inventario/ajuste-contable?generarAsiento={generarAsiento}", null);
        if (!response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorPayload>();
            throw new HttpRequestException(payload?.Message ?? "No se pudo reconciliar el inventario contable.");
        }

        return await response.Content.ReadFromJsonAsync<AjusteInventarioContableResponse>()
            ?? new AjusteInventarioContableResponse();
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

    private sealed class ErrorPayload
    {
        public string? Message { get; set; }
    }
}
