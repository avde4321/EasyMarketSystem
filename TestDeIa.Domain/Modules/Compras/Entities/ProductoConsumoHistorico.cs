namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class ProductoConsumoHistorico
{
    public Guid ProductoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public decimal StockActual { get; init; }
    public decimal CostoPromedio { get; init; }
    public IReadOnlyCollection<ConsumoDiarioHistoricoCompra> ConsumosDiarios { get; init; } = Array.Empty<ConsumoDiarioHistoricoCompra>();
}
