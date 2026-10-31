namespace TestDeIa.Shared.Responses.Inventario;

public sealed class StockDisponibleBodegaResponse
{
    public Guid ProductoId { get; set; }

    public Guid BodegaId { get; set; }

    public string BodegaCodigo { get; set; } = string.Empty;

    public string BodegaNombre { get; set; } = string.Empty;

    public decimal StockActual { get; set; }
}
