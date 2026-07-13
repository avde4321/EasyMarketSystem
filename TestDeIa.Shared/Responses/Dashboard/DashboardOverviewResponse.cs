namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardOverviewResponse
{
    public DashboardFinancialSummaryResponse ResumenFinanciero { get; set; } = new();
    public IReadOnlyCollection<DashboardTopProductoResponse> TopProductos { get; set; } = Array.Empty<DashboardTopProductoResponse>();
    public IReadOnlyCollection<DashboardStockPredictionAlertResponse> AlertasPredictivas { get; set; } = Array.Empty<DashboardStockPredictionAlertResponse>();
}
