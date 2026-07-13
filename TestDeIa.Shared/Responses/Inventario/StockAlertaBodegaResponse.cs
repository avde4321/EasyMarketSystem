namespace TestDeIa.Shared.Responses.Inventario;

public sealed class StockAlertaBodegaResponse
{
    public Guid BodegaId { get; set; }
    public string BodegaNombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
}
