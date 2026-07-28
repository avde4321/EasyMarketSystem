using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Client.Services.Empresa;

public sealed class EmpresaApiClient
{
    private readonly HttpClient httpClient;

    public EmpresaApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<EmpresaResponse?> GetCurrentAsync()
    {
        var response = await httpClient.GetAsync("api/empresa/actual");

        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmpresaResponse>();
    }

    public async Task<IReadOnlyCollection<EmpresaOptionResponse>> GetMineAsync()
    {
        var response = await httpClient.GetAsync("api/empresa");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Array.Empty<EmpresaOptionResponse>();
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<EmpresaOptionResponse>>()
            ?? Array.Empty<EmpresaOptionResponse>();
    }

    public async Task<EmpresaResponse?> GetByIdAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<EmpresaResponse>($"api/empresa/{id}");
    }

    public async Task<PagedResultResponse<EmpresaResponse>> GetPagedAsync(string? term, int skip, int take)
    {
        var encodedTerm = Uri.EscapeDataString(term ?? string.Empty);
        return await httpClient.GetFromJsonAsync<PagedResultResponse<EmpresaResponse>>($"api/empresa/paged?term={encodedTerm}&skip={skip}&take={take}")
            ?? new PagedResultResponse<EmpresaResponse> { Skip = skip, Take = take };
    }

    public Task<(bool Succeeded, string? ErrorMessage, EmpresaResponse? Data)> CreateAsync(EmpresaRequest request)
    {
        return SaveInternalAsync("api/empresa", request);
    }

    public Task<(bool Succeeded, string? ErrorMessage, EmpresaResponse? Data)> UpdateAsync(Guid id, EmpresaRequest request)
    {
        return SaveInternalAsync($"api/empresa/{id}", request);
    }

    private async Task<(bool Succeeded, string? ErrorMessage, EmpresaResponse? Data)> SaveInternalAsync(string endpoint, EmpresaRequest request)
    {
        var response = endpoint == "api/empresa"
            ? await httpClient.PostAsJsonAsync(endpoint, request)
            : await httpClient.PutAsJsonAsync(endpoint, request);

        if (response.IsSuccessStatusCode)
        {
            return (true, null, await response.Content.ReadFromJsonAsync<EmpresaResponse>());
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            return (false, error?.Message ?? "No se pudo guardar la empresa.", null);
        }

        return (false, "No se pudo guardar la empresa.", null);
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
