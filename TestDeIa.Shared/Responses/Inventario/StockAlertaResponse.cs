namespace TestDeIa.Shared.Responses.Inventario;

public sealed class StockAlertaResponse
{
    public Guid ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal StockMinimo { get; set; }
    public decimal StockTotal { get; set; }
    public IReadOnlyCollection<StockAlertaBodegaResponse> BodegasComprometidas { get; set; } = Array.Empty<StockAlertaBodegaResponse>();
}
