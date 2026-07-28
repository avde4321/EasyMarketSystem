using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Client.Services.Compras;

public sealed class ProveedoresApiClient
{
    private readonly HttpClient httpClient;

    public ProveedoresApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<PagedResultResponse<ProveedorResponse>> GetPagedAsync(string? term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<ProveedorResponse>>($"api/proveedores?term={encodedTerm}&skip={skip}&take={take}")
            ?? new PagedResultResponse<ProveedorResponse> { Skip = skip, Take = take };
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateAsync(ProveedorRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/proveedores", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateAsync(Guid id, ProveedorRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/proveedores/{id}", request);
        return await BuildResultAsync(response);
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/proveedores/{id}");
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
            return (false, error?.Message ?? "No se pudo guardar el proveedor.");
        }

        return (false, "No se pudo completar la operacion.");
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
