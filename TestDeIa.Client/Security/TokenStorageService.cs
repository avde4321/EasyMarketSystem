using Microsoft.JSInterop;

namespace TestDeIa.Client.Security;

public sealed class TokenStorageService
{
    private const string TokenKey = "testdeia.security.token";
    private readonly IJSRuntime jsRuntime;

    public TokenStorageService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public ValueTask<string?> GetTokenAsync()
    {
        return jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    public ValueTask SetTokenAsync(string token)
    {
        return jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
    }

    public ValueTask RemoveTokenAsync()
    {
        return jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }
}
