using System.Net;
using System.Net.Http.Json;
using TestDeIa.Client.Services.Saas;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Tests.Saas;

public sealed class SaasBillingApiClientTests
{
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
