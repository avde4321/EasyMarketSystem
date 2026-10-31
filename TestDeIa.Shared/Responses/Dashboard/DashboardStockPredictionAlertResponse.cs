namespace TestDeIa.Shared.Responses.Dashboard;

public sealed class DashboardStockPredictionAlertResponse
{
    public Guid ProductoId { get; set; }
    public Guid BodegaId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string BodegaNombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal ConsumoPromedioDiario { get; set; }
    public decimal TendenciaConsumo { get; set; }
    public decimal DiasStockEstimados { get; set; }
    public DateOnly? FechaProbableQuiebre { get; set; }
    public string Riesgo { get; set; } = string.Empty;
}
