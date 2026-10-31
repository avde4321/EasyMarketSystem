using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services.Empresa;

namespace TestDeIa.Client.Pages;

public partial class Logout
{
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private EmpresaSessionService EmpresaSessionService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var provider = (TokenAuthenticationStateProvider)AuthenticationStateProvider;
        await provider.MarkUserAsLoggedOutAsync();
        await EmpresaSessionService.ClearEmpresaIdAsync();
        NavigationManager.NavigateTo("/login", replace: true);
    }
}
