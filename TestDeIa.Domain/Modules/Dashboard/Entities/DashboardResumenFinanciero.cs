namespace TestDeIa.Domain.Modules.Dashboard.Entities;

public sealed class DashboardResumenFinanciero
{
    public decimal TotalVentasFacturadas { get; init; }
    public decimal TotalComprasRegistradas { get; init; }
    public decimal MargenGananciaEstimado { get; init; }
}
