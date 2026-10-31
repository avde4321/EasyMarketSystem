namespace TestDeIa.Domain.Modules.Dashboard.Entities;

public sealed class ProductoBodegaConsumoHistorico
{
    public Guid ProductoId { get; init; }
    public Guid BodegaId { get; init; }
    public string CodigoProducto { get; init; } = string.Empty;
    public string NombreProducto { get; init; } = string.Empty;
    public string BodegaNombre { get; init; } = string.Empty;
    public decimal StockActual { get; init; }
    public decimal StockMinimo { get; init; }
    public decimal CostoPromedio { get; init; }
    public IReadOnlyCollection<ConsumoDiarioHistorico> ConsumosDiarios { get; init; } = Array.Empty<ConsumoDiarioHistorico>();
}
