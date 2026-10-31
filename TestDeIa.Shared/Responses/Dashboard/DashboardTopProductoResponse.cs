namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardTopProductoResponse
{
    public Guid ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal CostoEstimado { get; set; }
    public decimal MargenEstimado { get; set; }
}
