namespace TestDeIa.Domain.Modules.Dashboard.Entities;

public sealed class DashboardAlertaPredictivaStock
{
    public Guid ProductoId { get; init; }
    public Guid BodegaId { get; init; }
    public string CodigoProducto { get; init; } = string.Empty;
    public string NombreProducto { get; init; } = string.Empty;
    public string BodegaNombre { get; init; } = string.Empty;
    public decimal StockActual { get; init; }
    public decimal StockMinimo { get; init; }
    public decimal ConsumoPromedioDiario { get; init; }
    public decimal TendenciaConsumo { get; init; }
    public decimal DiasStockEstimados { get; init; }
    public DateOnly? FechaProbableQuiebre { get; init; }
    public string Riesgo { get; init; } = string.Empty;
}
