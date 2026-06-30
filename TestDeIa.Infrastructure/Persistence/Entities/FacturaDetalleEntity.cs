namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class FacturaDetalleEntity
{
    public Guid Id { get; set; }

    public Guid FacturaId { get; set; }

    public FacturaEntity Factura { get; set; } = default!;

    public Guid ProductoId { get; set; }

    public string CodigoProducto { get; set; } = string.Empty;

    public string NombreProducto { get; set; } = string.Empty;

    public string CodigoIva { get; set; } = string.Empty;

    public decimal PorcentajeIva { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal Subtotal { get; set; }

    public decimal IvaValor { get; set; }

    public decimal Total { get; set; }
}
