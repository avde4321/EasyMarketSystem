using System.Net;
using System.Net.Http.Json;
using TestDeIa.Shared.Requests.Personas;
using TestDeIa.Shared.Responses.Personas;

namespace TestDeIa.Client.Services.Personas;

public sealed class PersonasApiClient
{
    private readonly HttpClient httpClient;

    public PersonasApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<PersonaResponse>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PersonaResponse>>("api/personas")
            ?? Array.Empty<PersonaResponse>();
    }

    public async Task<PersonaResponse?> FindByIdentificacionAsync(string identificacion)
    {
        var response = await httpClient.GetAsync($"api/personas/buscar?identificacion={Uri.EscapeDataString(identificacion.Trim())}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PersonaResponse>();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> CreateAsync(PersonaRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/personas", request);
        return await BuildResultAsync(response);
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> UpdateAsync(Guid id, PersonaRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/personas/{id}", request);
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
            return (false, error?.Message ?? "No se pudo guardar la persona.");
        }

        return (false, "No se pudo completar la operacion.");
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
