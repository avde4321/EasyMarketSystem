namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class LiquidacionComisionResponse
{
    public DateOnly Desde { get; set; }

    public DateOnly Hasta { get; set; }

    public Guid? OperadorId { get; set; }

    public decimal TotalServicios { get; set; }

    public decimal TotalComisiones { get; set; }

    public IReadOnlyCollection<LiquidacionComisionDetalleResponse> Detalles { get; set; } = [];
}