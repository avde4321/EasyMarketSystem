using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;
using TestDeIa.Shared.Responses.Common;
using System.Globalization;

namespace TestDeIa.Client.Services.Saas;

public sealed class SaasBillingApiClient(HttpClient http)
{
    public Task<PagedResultResponse<SaasConsumoResponse>> GetConsumptionAsync(SaasConsumoQuery query, CancellationToken ct = default)
        => SendAsync<PagedResultResponse<SaasConsumoResponse>>(new HttpRequestMessage(HttpMethod.Get,
            $"api/v1/saas/billing/admin/consumption?skip={query.Skip}&take={query.Take}&term={Uri.EscapeDataString(query.Term ?? string.Empty)}"), ct);
    public Task<SaasDashboardResponse> GetDashboardAsync(CancellationToken ct = default)
        => SendAsync<SaasDashboardResponse>(new HttpRequestMessage(HttpMethod.Get, "api/v1/saas/billing/admin/dashboard"), ct);
    public Task<PagedResultResponse<SaasPagoAdminResponse>> GetAdminPaymentsAsync(SaasPagosAdminQuery query, CancellationToken ct = default)
    {
        var parts = new List<string> { $"skip={query.Skip}", $"take={query.Take}",
            $"estado={Uri.EscapeDataString(query.Estado ?? "Todos")}", $"term={Uri.EscapeDataString(query.Term ?? string.Empty)}" };
        if (query.EmpresaId.HasValue) parts.Add($"empresaId={query.EmpresaId:D}");
        if (query.FechaDesde.HasValue) parts.Add("fechaDesde=" + query.FechaDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (query.FechaHasta.HasValue) parts.Add("fechaHasta=" + query.FechaHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        return SendAsync<PagedResultResponse<SaasPagoAdminResponse>>(new HttpRequestMessage(HttpMethod.Get,
            "api/v1/saas/billing/admin/payments?" + string.Join("&", parts)), ct);
    }
    public Task<SaasPagoResponse> ConfirmPaymentAsync(ConfirmarPagoSaasDto dto, CancellationToken ct = default)
        => SendAsync<SaasPagoResponse>(new HttpRequestMessage(HttpMethod.Post, "api/v1/saas/billing/confirm-payment") { Content = JsonContent.Create(dto) }, ct);
    public Task<SaasBillingPortalResponse> GetPortalAsync(CancellationToken ct = default)
        => SendAsync<SaasBillingPortalResponse>(new HttpRequestMessage(HttpMethod.Get, "api/v1/saas/billing/portal"), ct);
    public Task<SaasPagoResponse> RegisterTransferAsync(RegistrarTransferenciaSaasDto dto, CancellationToken ct = default)
        => SendAsync<SaasPagoResponse>(new HttpRequestMessage(HttpMethod.Post, "api/v1/saas/billing/register-transfer") { Content = JsonContent.Create(dto) }, ct);
    public Task<SaasCheckoutResponse> CheckoutAsync(int planId, CancellationToken ct = default)
        => SendAsync<SaasCheckoutResponse>(new HttpRequestMessage(HttpMethod.Post, "api/v1/saas/billing/checkout")
            { Content = JsonContent.Create(new CheckoutSaasDto { SaasPlanId = planId }) }, ct);

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
    {
        using (request)
        using (var response = await http.SendAsync(request, ct))
        {
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<T>(ct)
                    ?? throw new SaasBillingClientException("No se pudo leer la respuesta del servicio.");
            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión terminó. Cierra sesión y vuelve a ingresar.",
                HttpStatusCode.Forbidden => "Tu usuario no tiene permisos para gestionar pagos de esta empresa.",
                HttpStatusCode.TooManyRequests => "Se alcanzó el límite de consultas. Espera un momento antes de reintentar.",
                _ => "No se pudo completar la operación. Intenta nuevamente."
            };
            if ((int)response.StatusCode < 500 && response.StatusCode != HttpStatusCode.TooManyRequests)
            {
                try
                {
                    using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                    if (document.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (document.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(detail.GetString()))
                            message = detail.GetString()!;
                        else if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                            message = string.Join(" ", errors.EnumerateObject().Where(x => x.Value.ValueKind == JsonValueKind.Array)
                                .SelectMany(x => x.Value.EnumerateArray()).Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()).Distinct().Take(8));
                    }
                }
                catch (JsonException) { }
            }
            throw new SaasBillingClientException(string.IsNullOrWhiteSpace(message) ? "Revisa los datos del formulario." : message);
        }
    }
}

public sealed class SaasBillingClientException(string message) : Exception(message);
