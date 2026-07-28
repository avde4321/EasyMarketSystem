using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Empleados;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empleados;

namespace TestDeIa.Client.Services.Empleados;

public sealed class EmpleadosApiClient
{
    private readonly HttpClient httpClient;

    public EmpleadosApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<PagedResultResponse<EmpleadoResponse>> GetPagedAsync(string? term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<EmpleadoResponse>>($"api/empleados?term={encodedTerm}&skip={skip}&take={take}")
            ?? new PagedResultResponse<EmpleadoResponse> { Skip = skip, Take = take };
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateAsync(EmpleadoRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/empleados", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateAsync(Guid id, EmpleadoRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/empleados/{id}", request);
        return await BuildResultAsync(response);
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/empleados/{id}");
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
            return (false, error?.Message ?? "No se pudo guardar el empleado.");
        }

        return (false, "No se pudo completar la operacion.");
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
