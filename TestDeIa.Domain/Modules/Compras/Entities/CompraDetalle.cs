namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class CompraDetalle
{
    public CompraDetalle(
        Guid id,
        Guid compraId,
        Guid productoId,
        string productoCodigo,
        string productoNombre,
        string codigoIva,
        decimal porcentajeIva,
        decimal cantidad,
        decimal costoUnitario,
        decimal descuento,
        decimal costoTotalSinImpuesto)
    {
        Id = id;
        CompraId = compraId;
        ProductoId = productoId;
        ProductoCodigo = productoCodigo;
        ProductoNombre = productoNombre;
        CodigoIva = codigoIva;
        PorcentajeIva = porcentajeIva;
        Cantidad = cantidad;
        CostoUnitario = costoUnitario;
        Descuento = descuento;
        CostoTotalSinImpuesto = costoTotalSinImpuesto;
    }

    public Guid Id { get; }
    public Guid CompraId { get; }
    public Guid ProductoId { get; }
    public string ProductoCodigo { get; }
    public string ProductoNombre { get; }
    public string CodigoIva { get; }
    public decimal PorcentajeIva { get; }
    public decimal Cantidad { get; }
    public decimal CostoUnitario { get; }
    public decimal Descuento { get; }
    public decimal CostoTotalSinImpuesto { get; }
    public decimal TotalImpuesto => Math.Round(CostoTotalSinImpuesto * (PorcentajeIva / 100m), 2, MidpointRounding.AwayFromZero);
    public decimal TotalLinea => CostoTotalSinImpuesto + TotalImpuesto;
}
