namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardBodegaEstadoResponse
{
    public Guid BodegaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
    public int ItemsConStock { get; set; }
    public int ItemsCriticos { get; set; }
    public decimal StockTotal { get; set; }
    public decimal ValorInventario { get; set; }
    public decimal EntradasDia { get; set; }
    public decimal SalidasDia { get; set; }
    public decimal EntradasMes { get; set; }
    public decimal SalidasMes { get; set; }
}
