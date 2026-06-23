using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Client.Security;

public sealed class SecurityApiClient
{
    private readonly HttpClient httpClient;

    public SecurityApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/security/login", request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = "Usuario o contrasena incorrectos."
            };
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = "No se pudo leer la respuesta del servidor."
            };
    }
}
