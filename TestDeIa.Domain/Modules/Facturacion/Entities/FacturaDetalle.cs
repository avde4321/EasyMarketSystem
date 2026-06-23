namespace TestDeIa.Domain.Modules.Facturacion.Entities;

public sealed class FacturaDetalle
{
    public FacturaDetalle(
        Guid id,
        Guid facturaId,
        Guid productoId,
        string codigoProducto,
        string nombreProducto,
        string codigoIva,
        decimal porcentajeIva,
        decimal cantidad,
        decimal precioUnitario,
        decimal subtotal,
        decimal ivaValor,
        decimal total)
    {
        Id = id;
        FacturaId = facturaId;
        ProductoId = productoId;
        CodigoProducto = codigoProducto;
        NombreProducto = nombreProducto;
        CodigoIva = codigoIva;
        PorcentajeIva = porcentajeIva;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Subtotal = subtotal;
        IvaValor = ivaValor;
        Total = total;
    }

    public Guid Id { get; }

    public Guid FacturaId { get; }

    public Guid ProductoId { get; }

    public string CodigoProducto { get; }

    public string NombreProducto { get; }

    public string CodigoIva { get; }

    public decimal PorcentajeIva { get; }

    public decimal Cantidad { get; }

    public decimal PrecioUnitario { get; }

    public decimal Subtotal { get; }

    public decimal IvaValor { get; }

    public decimal Total { get; }
}
