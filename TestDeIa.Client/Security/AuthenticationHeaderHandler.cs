using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace TestDeIa.Client.Security;

public sealed class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly TokenStorageService tokenStorageService;
    private readonly Services.Empresa.EmpresaSessionService empresaSessionService;
    private readonly TokenAuthenticationStateProvider authenticationStateProvider;
    private readonly NavigationManager navigationManager;

    public AuthenticationHeaderHandler(
        TokenStorageService tokenStorageService,
        Services.Empresa.EmpresaSessionService empresaSessionService,
        TokenAuthenticationStateProvider authenticationStateProvider,
        NavigationManager navigationManager)
    {
        this.tokenStorageService = tokenStorageService;
        this.empresaSessionService = empresaSessionService;
        this.authenticationStateProvider = authenticationStateProvider;
        this.navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenStorageService.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var empresaId = await empresaSessionService.GetEmpresaIdAsync();
        if (!string.IsNullOrWhiteSpace(empresaId))
        {
            request.Headers.Remove("X-Empresa-Id");
            request.Headers.Add("X-Empresa-Id", empresaId);
        }

        var response = await base.SendAsync(request, cancellationToken);
        var isLoginRequest = request.RequestUri?.AbsolutePath.EndsWith("/api/security/login", StringComparison.OrdinalIgnoreCase) == true;

        // El cliente solo puede inspeccionar la expiración localmente; la API también puede
        // revocar un token. Al recibir 401, descartar esa sesión evita que el UI siga mostrando
        // un usuario autenticado con credenciales que el servidor ya rechazó.
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isLoginRequest)
        {
            await authenticationStateProvider.MarkUserAsLoggedOutAsync();
            await empresaSessionService.ClearEmpresaIdAsync();
            navigationManager.NavigateTo("/login", replace: true);
        }

        return response;
    }
}
