namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class EstudioMercadoTopProducto
{
    public Guid ProductoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public decimal CantidadVendida { get; init; }
    public decimal TotalVendido { get; init; }
    public decimal CostoEstimado { get; init; }
    public decimal MargenEstimado { get; init; }
    public decimal StockActual { get; init; }
}
