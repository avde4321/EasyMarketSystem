namespace TestDeIa.Shared.Requests.Proformas;

public sealed class ProformaDetalleRequest
{
    public Guid ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal TarifaIVA { get; set; }

    public decimal ValorIVA { get; set; }

    public decimal Subtotal { get; set; }
}
