namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardFinancialSummaryResponse
{
    public decimal TotalVentasFacturadas { get; set; }
    public decimal VentasInventario { get; set; }
    public decimal IngresosPorServicios { get; set; }
    public decimal TotalComprasRegistradas { get; set; }
    public decimal MargenGananciaEstimado { get; set; }
}
