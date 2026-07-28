using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Client.Services.Security;

public sealed class SecurityUserAdminApiClient
{
    private readonly HttpClient httpClient;

    public SecurityUserAdminApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<SecurityPointEmissionResponse>> GetPuntosEmisionAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<SecurityPointEmissionResponse>>("api/security/usuarios-admin/puntos-emision")
            ?? [];
    }

    public async Task<IReadOnlyCollection<Guid>> GetPuntosEmisionByUserAsync(Guid userId)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<Guid>>($"api/security/usuarios-admin/{userId}/puntos-emision")
            ?? [];
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateAsync(SecurityUserAdminRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/security/usuarios-admin", request);
        return await ParseResponseAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateAsync(Guid id, SecurityUserAdminRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/security/usuarios-admin/{id}", request);
        return await ParseResponseAsync(response);
    }

    private static async Task<(bool Succeeded, string? ErrorMessage)> ParseResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var apiError = await response.Content.ReadFromJsonAsync<ApiError>();
        if (!string.IsNullOrWhiteSpace(apiError?.Message))
        {
            return (false, apiError.Message);
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(errorMessage) ? "La operacion no pudo completarse." : errorMessage);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}

