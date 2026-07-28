using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Client.Services.Compras;

public sealed class CuentasPorPagarApiClient
{
    private readonly HttpClient httpClient;

    public CuentasPorPagarApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<PagedResultResponse<CuentaPorPagarResponse>> GetPagedAsync(string? term, Guid? proveedorId, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        var proveedorQuery = proveedorId.HasValue && proveedorId.Value != Guid.Empty
            ? $"&proveedorId={proveedorId.Value}"
            : string.Empty;

        return await httpClient.GetFromJsonAsync<PagedResultResponse<CuentaPorPagarResponse>>(
            $"api/cuentas-por-pagar?term={encodedTerm}{proveedorQuery}&skip={skip}&take={take}")
            ?? new PagedResultResponse<CuentaPorPagarResponse> { Skip = skip, Take = take };
    }

    public async Task<CuentasPorPagarResumenResponse> GetResumenAsync()
    {
        return await httpClient.GetFromJsonAsync<CuentasPorPagarResumenResponse>("api/cuentas-por-pagar/resumen")
            ?? new CuentasPorPagarResumenResponse();
    }

    public async Task<(bool Succeeded, string? ErrorMessage, CuentaPorPagarResponse? Data)> RegistrarAbonoAsync(RegistrarAbonoCxPRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/cuentas-por-pagar/abonos", request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<CuentaPorPagarResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo registrar el abono.", null);
        }

        return (false, "No se pudo registrar el abono.", null);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
