namespace TestDeIa.Shared.Requests.Facturacion;

public sealed class NotaCreditoDetalleRequestDto
{
    public Guid FacturaDetalleId { get; set; }

    public decimal Cantidad { get; set; }
}
