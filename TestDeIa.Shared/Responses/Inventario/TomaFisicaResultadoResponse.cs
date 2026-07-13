namespace TestDeIa.Shared.Responses.Inventario;

public sealed class TomaFisicaResultadoResponse
{
    public Guid BodegaId { get; set; }
    public string BodegaNombre { get; set; } = string.Empty;
    public int ProductosProcesados { get; set; }
    public int MovimientosGenerados { get; set; }
}
