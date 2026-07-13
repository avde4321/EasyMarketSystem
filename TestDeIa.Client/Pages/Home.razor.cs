using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Dashboard;
using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Client.Pages;

public partial class Home
{
    [Inject]
    private DashboardApiClient DashboardApiClient { get; set; } = default!;

    private DashboardOverviewResponse overview = new();
    private string? errorMessage;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            overview = await DashboardApiClient.GetOverviewAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el dashboard gerencial de la empresa activa.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string GetRiskCssClass(string riesgo)
    {
        return riesgo switch
        {
            "Critico" => "risk-critical",
            "Alto" => "risk-high",
            "Medio" => "risk-medium",
            _ => "risk-low"
        };
    }
}
