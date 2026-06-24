using System.Net.Http.Headers;

namespace TestDeIa.Client.Security;

public sealed class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly TokenStorageService tokenStorageService;
    private readonly Services.Empresa.EmpresaSessionService empresaSessionService;

    public AuthenticationHeaderHandler(
        TokenStorageService tokenStorageService,
        Services.Empresa.EmpresaSessionService empresaSessionService)
    {
        this.tokenStorageService = tokenStorageService;
        this.empresaSessionService = empresaSessionService;
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

        return await base.SendAsync(request, cancellationToken);
    }
}
