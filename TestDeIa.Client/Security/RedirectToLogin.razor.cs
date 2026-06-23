using Microsoft.AspNetCore.Components;

namespace TestDeIa.Client.Security;

public partial class RedirectToLogin
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        if (!currentUrl.StartsWith("login", StringComparison.OrdinalIgnoreCase))
        {
            NavigationManager.NavigateTo("/login", replace: true);
        }
    }
}
