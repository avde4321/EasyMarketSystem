namespace TestDeIa.Shared.Responses.Compras;

public sealed class EstudioMercadoSugerenciaCompraResponse
{
    public Guid ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal CantidadRecomendada { get; set; }
    public decimal CostoEstimado { get; set; }
    public string JustificacionAlgoritmo { get; set; } = string.Empty;
}
