using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services.Empresa;
using TestDeIa.Shared.Requests.Security;

namespace TestDeIa.Client.Pages;

public partial class Login
{
    [Inject]
    private SecurityApiClient SecurityApiClient { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private EmpresaSessionService EmpresaSessionService { get; set; } = default!;

    private readonly LoginRequest loginRequest = new();
    private bool isBusy;
    private string? errorMessage;

    private async Task LoginAsync()
    {
        isBusy = true;
        errorMessage = null;

        try
        {
            var response = await SecurityApiClient.LoginAsync(loginRequest);

            if (!response.Succeeded || string.IsNullOrWhiteSpace(response.Token))
            {
                errorMessage = response.ErrorMessage ?? "No se pudo iniciar sesion.";
                return;
            }

            var provider = (TokenAuthenticationStateProvider)AuthenticationStateProvider;
            await provider.MarkUserAsAuthenticatedAsync(response.Token);

            if (response.ActiveEmpresaId.HasValue)
            {
                await EmpresaSessionService.SetEmpresaIdAsync(response.ActiveEmpresaId.Value);
            }
            else
            {
                await EmpresaSessionService.ClearEmpresaIdAsync();
            }

            NavigationManager.NavigateTo("/");
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo conectar con la API.";
        }
        finally
        {
            isBusy = false;
        }
    }
}
