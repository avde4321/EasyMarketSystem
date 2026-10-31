using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Client.Services.Saas;

// Cliente anónimo: nunca transmite el JWT ni la empresa de una sesión anterior.
public sealed class PublicTenantApiClient(HttpClient http)
{
    public async Task<IReadOnlyCollection<SaasPlanResponse>> GetPlansAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<IReadOnlyCollection<SaasPlanResponse>>("api/saas/plans", cancellationToken) ?? [];

    public async Task<LoginResponse> RegisterAsync(PublicTenantRegisterDto request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/v1/saas/public/register", request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
                ?? new LoginResponse { ErrorMessage = "No se pudo confirmar la respuesta. Intenta iniciar sesión con el correo registrado antes de repetir el registro." };
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            return new LoginResponse { ErrorMessage = "Se alcanzó el límite de intentos de registro. Espera un momento antes de volver a intentarlo." };
        if ((int)response.StatusCode >= 500)
            return new LoginResponse { ErrorMessage = "El servicio no está disponible en este momento. Intenta más tarde." };
        try
        {
            using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = problem.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return new LoginResponse { ErrorMessage = "No se pudo completar el registro. Revisa los datos e intenta nuevamente." };
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = errors.EnumerateObject().SelectMany(x => x.Value.ValueKind == JsonValueKind.Array
                    ? x.Value.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()!) : [])
                    .Distinct().Take(8).ToArray();
                if (messages.Length > 0) return new LoginResponse { ErrorMessage = string.Join(" ", messages) };
            }
            foreach (var key in new[] { "detail", "message", "title" })
                if (root.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()))
                    return new LoginResponse { ErrorMessage = value.GetString() };
        }
        catch (JsonException) { /* Una respuesta HTML/vacía no debe romper el formulario. */ }
        return new LoginResponse { ErrorMessage = "No se pudo completar el registro. Revisa los datos e intenta nuevamente." };
    }
}
