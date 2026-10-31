namespace TestDeIa.Shared.Reports.Facturacion;

public sealed class NotaCreditoRideDetalleDto
{
    public string CodigoProducto { get; set; } = string.Empty;

    public string NombreProducto { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal Subtotal { get; set; }

    public decimal IvaValor { get; set; }

    public decimal Total { get; set; }
}
