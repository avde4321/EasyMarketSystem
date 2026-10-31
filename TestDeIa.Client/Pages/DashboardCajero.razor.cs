using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Dashboard;
using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Client.Pages;

public partial class DashboardCajero
{
    [Inject]
    private DashboardApiClient DashboardApiClient { get; set; } = default!;

    private DashboardCajeroOverviewResponse overview = new();
    private string? errorMessage;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            overview = await DashboardApiClient.GetCajeroOverviewAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar tu dashboard operativo. Verifica que tu perfil tenga rol Cajero o Asesor Comercial.";
        }
        finally
        {
            isLoading = false;
        }
    }
}
