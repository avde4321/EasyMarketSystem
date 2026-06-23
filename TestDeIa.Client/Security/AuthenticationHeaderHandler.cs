using System.Net.Http.Headers;

namespace TestDeIa.Client.Security;

public sealed class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly TokenStorageService tokenStorageService;

    public AuthenticationHeaderHandler(TokenStorageService tokenStorageService)
    {
        this.tokenStorageService = tokenStorageService;
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

        return await base.SendAsync(request, cancellationToken);
    }
}
