namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardBodegueroOverviewResponse
{
    public decimal EntradasDia { get; set; }
    public decimal SalidasDia { get; set; }
    public decimal EntradasMes { get; set; }
    public decimal SalidasMes { get; set; }
    public int BodegasActivas { get; set; }
    public int ItemsCriticos { get; set; }
    public decimal ValorInventario { get; set; }
    public IReadOnlyCollection<DashboardBodegaEstadoResponse> Bodegas { get; set; } = Array.Empty<DashboardBodegaEstadoResponse>();
    public IReadOnlyCollection<DashboardBodegaItemMovimientoResponse> ItemsMovilizados { get; set; } = Array.Empty<DashboardBodegaItemMovimientoResponse>();
}
