namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardCajeroOverviewResponse
{
    public decimal VentasDia { get; set; }
    public int ComprobantesEmitidos { get; set; }
    public string FormaPagoMasUsada { get; set; } = "Sin movimientos";
    public IReadOnlyCollection<DashboardVentaCajeroResponse> UltimasVentas { get; set; } = Array.Empty<DashboardVentaCajeroResponse>();
}
