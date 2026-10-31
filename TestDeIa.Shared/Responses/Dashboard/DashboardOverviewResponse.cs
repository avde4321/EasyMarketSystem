namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardOverviewResponse
{
    public DashboardFinancialSummaryResponse ResumenFinanciero { get; set; } = new();
    public IReadOnlyCollection<DashboardTopProductoResponse> TopProductos { get; set; } = Array.Empty<DashboardTopProductoResponse>();
    public IReadOnlyCollection<DashboardTopProductoResponse> TopServicios { get; set; } = Array.Empty<DashboardTopProductoResponse>();
    public IReadOnlyCollection<DashboardStockPredictionAlertResponse> AlertasPredictivas { get; set; } = Array.Empty<DashboardStockPredictionAlertResponse>();
    public DashboardVentasComparativoResponse ComparativoInteranual { get; set; } = new();
    public IReadOnlyCollection<DashboardProductividadUsuarioResponse> ProductividadUsuarios { get; set; } = Array.Empty<DashboardProductividadUsuarioResponse>();
    public IReadOnlyCollection<DashboardEstrategiaNegocioResponse> EstrategiasNegocio { get; set; } = Array.Empty<DashboardEstrategiaNegocioResponse>();
}
