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

    public async Task<IReadOnlyCollection<SecurityUserResponse>> GetUsersAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<SecurityUserResponse>>("api/security/users")
            ?? Array.Empty<SecurityUserResponse>();
    }

    public async Task<IReadOnlyCollection<SecurityRoleResponse>> GetRolesAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<SecurityRoleResponse>>("api/security/roles")
            ?? Array.Empty<SecurityRoleResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateUserAsync(SecurityUserRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/security/users", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateUserAsync(Guid id, SecurityUserRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/security/users/{id}", request);
        return await BuildResultAsync(response);
    }

    private static async Task<(bool Succeeded, string? ErrorMessage)> BuildResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo completar la operacion.");
        }

        return (false, "No se pudo completar la operacion.");
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
