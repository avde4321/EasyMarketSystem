namespace TestDeIa.Domain.Modules.Dashboard.Entities;

public sealed class DashboardOverview
{
    public DashboardResumenFinanciero ResumenFinanciero { get; init; } = new();
    public IReadOnlyCollection<DashboardTopProducto> TopProductos { get; init; } = Array.Empty<DashboardTopProducto>();
    public IReadOnlyCollection<DashboardAlertaPredictivaStock> AlertasPredictivas { get; init; } = Array.Empty<DashboardAlertaPredictivaStock>();
}
