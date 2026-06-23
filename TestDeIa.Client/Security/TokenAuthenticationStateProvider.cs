using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TestDeIa.Client.Security;

public sealed class TokenAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private readonly TokenStorageService tokenStorageService;

    public TokenAuthenticationStateProvider(TokenStorageService tokenStorageService)
    {
        this.tokenStorageService = tokenStorageService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonymous);
        }

        if (IsExpired(token))
        {
            await tokenStorageService.RemoveTokenAsync();
            return new AuthenticationState(Anonymous);
        }

        var principal = CreatePrincipal(token);
        return new AuthenticationState(principal);
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await tokenStorageService.SetTokenAsync(token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreatePrincipal(token))));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await tokenStorageService.RemoveTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static ClaimsPrincipal CreatePrincipal(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");

        return new ClaimsPrincipal(identity);
    }

    private static bool IsExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.ValidTo <= DateTime.UtcNow;
    }
}
