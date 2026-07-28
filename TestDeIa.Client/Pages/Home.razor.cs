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

    private decimal GetTopServiceWidth(DashboardTopProductoResponse servicio)
    {
        if (overview.TopServicios.Count == 0)
        {
            return 0m;
        }

        var maximo = overview.TopServicios.Max(current => current.TotalVendido);
        if (maximo <= 0m)
        {
            return 0m;
        }

        return Math.Round((servicio.TotalVendido / maximo) * 100m, 2, MidpointRounding.AwayFromZero);
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
