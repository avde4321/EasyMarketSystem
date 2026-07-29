namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardBodegaItemMovimientoResponse
{
    public Guid ProductoId { get; set; }
    public Guid BodegaId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string BodegaNombre { get; set; } = string.Empty;
    public decimal EntradasDia { get; set; }
    public decimal SalidasDia { get; set; }
    public decimal EntradasMes { get; set; }
    public decimal SalidasMes { get; set; }
    public decimal MovimientoTotalMes { get; set; }
}
