namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class NotaCreditoOrigenDetalleResponseDto
{
    public Guid FacturaDetalleId { get; set; }

    public string CodigoProducto { get; set; } = string.Empty;

    public string NombreProducto { get; set; } = string.Empty;

    public decimal CantidadFacturada { get; set; }

    public decimal CantidadDevuelta { get; set; }

    public decimal CantidadDisponible { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public decimal IvaValor { get; set; }

    public decimal Total { get; set; }
}
