using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Empresa;
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

    public async Task<(bool Succeeded, string? ErrorMessage, EmpresaResponse? Data)> SaveAsync(EmpresaRequest request)
    {
        var response = await httpClient.PutAsJsonAsync("api/empresa/actual", request);

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
