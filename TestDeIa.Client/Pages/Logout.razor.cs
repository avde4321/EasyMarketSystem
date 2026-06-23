using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TestDeIa.Client.Security;

namespace TestDeIa.Client.Pages;

public partial class Logout
{
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var provider = (TokenAuthenticationStateProvider)AuthenticationStateProvider;
        await provider.MarkUserAsLoggedOutAsync();
        NavigationManager.NavigateTo("/login", replace: true);
    }
}
