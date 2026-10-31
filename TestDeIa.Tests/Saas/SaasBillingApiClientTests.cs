using System.Net;
using System.Net.Http.Json;
using TestDeIa.Client.Services.Saas;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Tests.Saas;

public sealed class SaasBillingApiClientTests
{
    [Fact]
    public async Task ConsumptionClientUsesPagedAdminEndpointAndEscapesSearch()
    {
        using var http = new HttpClient(new Handler(request =>
        {
            Assert.Equal("/api/v1/saas/billing/admin/consumption", request.RequestUri!.AbsolutePath);
            Assert.Contains("take=10", request.RequestUri.Query);
            Assert.Contains("skip=20", request.RequestUri.Query);
            Assert.Contains("term=RUC%20%26%20Empresa", request.RequestUri.Query);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(
                new TestDeIa.Shared.Responses.Common.PagedResultResponse<SaasConsumoResponse> { TotalCount = 30, Skip = 20, Take = 10 }) });
        })) { BaseAddress = new Uri("https://example.invalid/") };
        var page = await new SaasBillingApiClient(http).GetConsumptionAsync(new SaasConsumoQuery { Term = "RUC & Empresa", Skip = 20, Take = 10 });
        Assert.Equal(30, page.TotalCount);
    }

    [Fact]
    public async Task AdminQuerySendsFiltersAndReadsPagedResult()
    {
        using var http = new HttpClient(new Handler(request =>
        {
            Assert.Equal("/api/v1/saas/billing/admin/payments", request.RequestUri!.AbsolutePath);
            Assert.Contains("skip=20", request.RequestUri.Query);
            Assert.Contains("take=20", request.RequestUri.Query);
            Assert.Contains("fechaDesde=2026-10-01", request.RequestUri.Query);
            Assert.Contains("fechaHasta=2026-10-31", request.RequestUri.Query);
            Assert.Contains("term=RUC%20%26%20Banco", request.RequestUri.Query);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(
                new TestDeIa.Shared.Responses.Common.PagedResultResponse<SaasPagoAdminResponse> { TotalCount = 30, Skip = 20, Take = 20 }) });
        })) { BaseAddress = new Uri("https://example.invalid/") };
        var page = await new SaasBillingApiClient(http).GetAdminPaymentsAsync(new SaasPagosAdminQuery { Term = "RUC & Banco", Skip = 20, Take = 20,
            FechaDesde = new DateOnly(2026, 10, 1), FechaHasta = new DateOnly(2026, 10, 31) });
        Assert.Equal(30, page.TotalCount); Assert.Equal(20, page.Skip);
    }

    [Fact]
    public async Task ConfirmationSendsThePaymentAndCompanyBeingReviewed()
    {
        var payment = Guid.NewGuid(); var company = Guid.NewGuid();
        using var http = new HttpClient(new Handler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/saas/billing/confirm-payment", request.RequestUri!.AbsolutePath);
            var dto = await request.Content!.ReadFromJsonAsync<ConfirmarPagoSaasDto>();
            Assert.Equal(payment, dto!.PagoId); Assert.Equal(company, dto.EmpresaId); Assert.True(dto.Aprobar);
            Assert.Equal("Verificado", dto.Observaciones);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new SaasPagoResponse(payment, company, 1, 29.99m,
                DateTime.UtcNow, "TransferenciaBancaria", "Aprobado", "TEST", DateTime.UtcNow, null)) };
        })) { BaseAddress = new Uri("https://example.invalid/") };
        var result = await new SaasBillingApiClient(http).ConfirmPaymentAsync(new ConfirmarPagoSaasDto
            { PagoId = payment, EmpresaId = company, Aprobar = true, Observaciones = "Verificado" });
        Assert.Equal("Aprobado", result.EstadoPago);
    }

    [Fact]
    public async Task RevokedPlatformAccessShowsForbiddenMessage()
    {
        using var http = new HttpClient(new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)
            { Content = JsonContent.Create(new { detail = "Solo el superadministrador puede consultar los pagos." }) }))) { BaseAddress = new Uri("https://example.invalid/") };
        var error = await Assert.ThrowsAsync<SaasBillingClientException>(() => new SaasBillingApiClient(http).GetAdminPaymentsAsync(new SaasPagosAdminQuery()));
        Assert.Contains("superadministrador", error.Message);
    }

    [Fact]
    public async Task TransferSendsReferenceDateAndReadsPendingStatus()
    {
        var date = new DateOnly(2026, 10, 1);
        using var http = new HttpClient(new Handler(async request =>
        {
            Assert.Equal("/api/v1/saas/billing/register-transfer", request.RequestUri!.AbsolutePath);
            var dto = await request.Content!.ReadFromJsonAsync<RegistrarTransferenciaSaasDto>();
            Assert.Equal(date, dto!.FechaPago); Assert.Equal("PRUEBA", dto.ComprobanteReferencia);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new SaasPagoResponse(Guid.NewGuid(), Guid.NewGuid(), 1, 29.99m,
                DateTime.UtcNow, "TransferenciaBancaria", "Pendiente", dto.ComprobanteReferencia, null, date)) };
        })) { BaseAddress = new Uri("https://example.invalid/") };
        var result = await new SaasBillingApiClient(http).RegisterTransferAsync(new RegistrarTransferenciaSaasDto { SaasPlanId = 1, Monto = 29.99m, FechaPago = date, ComprobanteReferencia = "PRUEBA" });
        Assert.Equal("Pendiente", result.EstadoPago); Assert.Equal(date, result.FechaPagoInformada);
    }

    [Theory]
    [InlineData(409, "{\"detail\":\"La referencia ya existe\"}", "La referencia ya existe")]
    [InlineData(400, "{\"errors\":{\"FechaPago\":[\"Fecha incorrecta\"]}}", "Fecha incorrecta")]
    [InlineData(403, "<html>Error</html>", "permisos")]
    [InlineData(503, "{\"detail\":\"SQL password\"}", "No se pudo completar")]
    public async Task ErrorsAreFriendlyAndDoNotExposeServerInternals(int status, string body, string expected)
    {
        using var http = new HttpClient(new Handler(_ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(body) })))
            { BaseAddress = new Uri("https://example.invalid/") };
        var error = await Assert.ThrowsAsync<SaasBillingClientException>(() => new SaasBillingApiClient(http).GetPortalAsync());
        Assert.Contains(expected, error.Message); Assert.DoesNotContain("SQL password", error.Message);
    }
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handle) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => handle(request); }
}
