using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Clientes;

namespace TestDeIa.Client.Services.Clientes;

public sealed class ClientesApiClient
{
    private readonly HttpClient httpClient;

    public ClientesApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<PagedResultResponse<ClienteResponse>> GetPagedAsync(string? term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<ClienteResponse>>($"api/clientes?term={encodedTerm}&skip={skip}&take={take}")
            ?? new PagedResultResponse<ClienteResponse> { Skip = skip, Take = take };
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
