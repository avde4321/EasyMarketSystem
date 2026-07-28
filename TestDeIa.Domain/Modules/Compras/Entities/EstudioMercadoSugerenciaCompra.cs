namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class EstudioMercadoSugerenciaCompra
{
    public Guid ProductoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public decimal CantidadRecomendada { get; init; }
    public decimal CostoEstimado { get; init; }
    public string JustificacionAlgoritmo { get; init; } = string.Empty;
}
