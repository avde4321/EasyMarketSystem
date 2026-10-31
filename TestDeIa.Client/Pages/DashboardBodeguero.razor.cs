using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Dashboard;
using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Client.Pages;

public partial class DashboardBodeguero
{
    [Inject]
    private DashboardApiClient DashboardApiClient { get; set; } = default!;

    private DashboardBodegueroOverviewResponse overview = new();
    private string? errorMessage;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            overview = await DashboardApiClient.GetBodegueroOverviewAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el dashboard de bodegas para la empresa activa.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private decimal GetBodegaMovementWidth(decimal value)
    {
        var max = overview.Bodegas
            .SelectMany(current => new[] { current.EntradasDia, current.SalidasDia, current.EntradasMes, current.SalidasMes })
            .DefaultIfEmpty(0m)
            .Max();

        return max <= 0m ? 0m : Math.Max(4m, Math.Round((value / max) * 100m, 2, MidpointRounding.AwayFromZero));
    }
}
