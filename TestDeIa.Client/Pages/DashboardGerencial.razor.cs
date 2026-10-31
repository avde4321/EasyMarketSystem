using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Dashboard;
using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Client.Pages;

public partial class DashboardGerencial
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
        return maximo <= 0m ? 0m : Math.Round((servicio.TotalVendido / maximo) * 100m, 2, MidpointRounding.AwayFromZero);
    }

    private string GetTrendClass()
    {
        return overview.ComparativoInteranual.PorcentajeVariacion >= 0m ? "trend-up" : "trend-down";
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

    private static string GetStrategyCssClass(string severidad)
    {
        return severidad switch
        {
            "Critico" => "strategy-critical",
            "Alto" => "strategy-high",
            _ => "strategy-info"
        };
    }

    private IReadOnlyCollection<ChartBarItem> GetFinancialBars()
    {
        var resumen = overview.ResumenFinanciero;
        var items = new[]
        {
            new ChartBarItem("Ventas", resumen.TotalVentasFacturadas, "sales-fill"),
            new ChartBarItem("Compras", resumen.TotalComprasRegistradas, "purchase-fill"),
            new ChartBarItem("Margen", resumen.MargenGananciaEstimado, "margin-fill")
        };

        var max = items.Max(current => Math.Abs(current.Amount));
        if (max <= 0m)
        {
            return items.Select(current => current with { Width = 0m }).ToArray();
        }

        return items
            .Select(current => current with
            {
                Width = Math.Max(4m, Math.Round((Math.Abs(current.Amount) / max) * 100m, 2, MidpointRounding.AwayFromZero))
            })
            .ToArray();
    }

    private decimal GetInventoryShare()
    {
        var total = overview.ResumenFinanciero.TotalVentasFacturadas;
        return total <= 0m ? 0m : Math.Round((overview.ResumenFinanciero.VentasInventario / total) * 100m, 2, MidpointRounding.AwayFromZero);
    }

    private decimal GetServiceShare()
    {
        var total = overview.ResumenFinanciero.TotalVentasFacturadas;
        return total <= 0m ? 0m : Math.Round((overview.ResumenFinanciero.IngresosPorServicios / total) * 100m, 2, MidpointRounding.AwayFromZero);
    }

    private decimal GetSellerWidth(decimal totalVentas)
    {
        if (overview.ProductividadUsuarios.Count == 0)
        {
            return 0m;
        }

        var max = overview.ProductividadUsuarios.Max(current => current.TotalVentas);
        return max <= 0m ? 0m : Math.Max(4m, Math.Round((totalVentas / max) * 100m, 2, MidpointRounding.AwayFromZero));
    }

    private sealed record ChartBarItem(string Label, decimal Amount, string CssClass)
    {
        public decimal Width { get; init; }
    }
}
