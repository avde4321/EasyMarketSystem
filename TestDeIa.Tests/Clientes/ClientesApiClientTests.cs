using System.Net;
using System.Text;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Shared.Requests.Clientes;

namespace TestDeIa.Tests.Clientes;

public sealed class ClientesApiClientTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{\"title\":\"Validación\",\"errors\":{\"FechaNacimiento\":[\"La fecha de nacimiento no puede ser futura.\"]}}", "La fecha de nacimiento no puede ser futura.")]
    [InlineData(HttpStatusCode.Conflict, "{\"message\":\"La persona ya tiene el rol de cliente.\"}", "La persona ya tiene el rol de cliente.")]
    [InlineData(HttpStatusCode.BadRequest, "{\"detail\":\"Revisa la identificación.\"}", "Revisa la identificación.")]
    [InlineData(HttpStatusCode.BadRequest, "<html>error</html>", "No se pudo guardar el cliente. Revisa los datos e intenta nuevamente.")]
    public async Task ValidationAndConflictMessagesReachTheForm(HttpStatusCode status, string body, string expected)
    {
        using var http = new HttpClient(new ResponseHandler(status, body)) { BaseAddress = new Uri("https://example.invalid/") };
        var result = await new ClientesApiClient(http).CreateAsync(new ClienteRequest());
        Assert.False(result.Succeeded);
        Assert.Equal(expected, result.ErrorMessage);
    }

    private sealed class ResponseHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
}
