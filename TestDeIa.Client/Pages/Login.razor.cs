using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TestDeIa.Client.Security;
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
