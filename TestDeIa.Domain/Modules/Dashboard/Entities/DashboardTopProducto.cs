namespace TestDeIa.Domain.Modules.Dashboard.Entities;

public sealed class DashboardTopProducto
{
    public Guid ProductoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public decimal CantidadVendida { get; init; }
    public decimal TotalVendido { get; init; }
    public decimal CostoEstimado { get; init; }
    public decimal MargenEstimado { get; init; }
}
