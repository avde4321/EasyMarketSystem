using System.Net;
using System.Net.Http.Json;
using System.Text;
using TestDeIa.Client.Services.Saas;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Tests.Saas;

public sealed class PublicTenantApiClientTests
{
    [Fact]
    public async Task RegistrationPostsSharedDtoAndReadsSessionWithoutTenantHeaders()
    {
        var companyId = Guid.NewGuid();
        using var http = new HttpClient(new Handler(async message =>
        {
            Assert.Equal(HttpMethod.Post, message.Method);
            Assert.Equal("/api/v1/saas/public/register", message.RequestUri!.AbsolutePath);
            Assert.Null(message.Headers.Authorization);
            Assert.False(message.Headers.Contains("X-Empresa-Id"));
            var dto = await message.Content!.ReadFromJsonAsync<PublicTenantRegisterDto>();
            Assert.NotNull(dto); Assert.Equal("1234567890001", dto.Ruc); Assert.True(dto.EsDemo); Assert.Equal(1, dto.PlanId);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new LoginResponse
                { Succeeded = true, Token = "test-token", ActiveEmpresaId = companyId }) };
        })) { BaseAddress = new Uri("https://example.invalid/") };
        var response = await new PublicTenantApiClient(http).RegisterAsync(new PublicTenantRegisterDto { Ruc = "1234567890001", PlanId = 1, EsDemo = true });
        Assert.True(response.Succeeded); Assert.Equal(companyId, response.ActiveEmpresaId); Assert.Equal("test-token", response.Token);
    }

    [Theory]
    [InlineData(409, "{\"detail\":\"El RUC ya existe.\"}", "El RUC ya existe.")]
    [InlineData(400, "{\"errors\":{\"Email\":[\"Correo inválido.\"]}}", "Correo inválido.")]
    [InlineData(429, "", "límite de intentos")]
    [InlineData(503, "{\"detail\":\"Internal database secret\"}", "no está disponible")]
    [InlineData(400, "<html>Error del proxy</html>", "No se pudo completar")]
    [InlineData(400, "[]", "No se pudo completar")]
    public async Task ApiErrorsAreReturnedWithoutBreakingTheForm(int status, string body, string expected)
    {
        using var http = new HttpClient(new Handler(_ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
            { Content = new StringContent(body, Encoding.UTF8, "application/json") }))) { BaseAddress = new Uri("https://example.invalid/") };
        var response = await new PublicTenantApiClient(http).RegisterAsync(new PublicTenantRegisterDto());
        Assert.False(response.Succeeded); Assert.Contains(expected, response.ErrorMessage);
        Assert.DoesNotContain("Internal database secret", response.ErrorMessage);
    }

    [Fact]
    public async Task PlansAreLoadedFromThePublicCatalog()
    {
        using var http = new HttpClient(new Handler(message =>
        {
            Assert.Equal(HttpMethod.Get, message.Method);
            Assert.Equal("/api/saas/plans", message.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("[{\"id\":1,\"nombre\":\"Básico\",\"esActivo\":true}]", Encoding.UTF8, "application/json") });
        })) { BaseAddress = new Uri("https://example.invalid/") };
        var plans = await new PublicTenantApiClient(http).GetPlansAsync();
        Assert.Equal("Básico", Assert.Single(plans).Nombre);
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}
