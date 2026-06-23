using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Clientes;

namespace TestDeIa.Client.Services.Clientes;

public sealed class ClientesApiClient
{
    private readonly HttpClient httpClient;

    public ClientesApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<ClienteResponse>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<ClienteResponse>>("api/clientes")
            ?? Array.Empty<ClienteResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateAsync(ClienteRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/clientes", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateAsync(Guid id, ClienteRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/clientes/{id}", request);
        return await BuildResultAsync(response);
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/clientes/{id}");
        response.EnsureSuccessStatusCode();
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
            return (false, error?.Message ?? "No se pudo guardar el cliente.");
        }

        return (false, "No se pudo completar la operacion.");
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
