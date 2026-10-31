using TestDeIa.Domain.Modules.Compras.Enums;

namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class CompraDetalle
{
    public CompraDetalle(
        Guid id,
        Guid compraId,
        Guid? productoId,
        string productoCodigo,
        string productoNombre,
        NaturalezaCompra naturalezaCompra,
        string? nombreActivo,
        string? categoriaSriActivo,
        string? serieUbicacionActivo,
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
        NaturalezaCompra = naturalezaCompra;
        NombreActivo = nombreActivo;
        CategoriaSriActivo = categoriaSriActivo;
        SerieUbicacionActivo = serieUbicacionActivo;
        CodigoIva = codigoIva;
        PorcentajeIva = porcentajeIva;
        Cantidad = cantidad;
        CostoUnitario = costoUnitario;
        Descuento = descuento;
        CostoTotalSinImpuesto = costoTotalSinImpuesto;
    }

    public Guid Id { get; }
    public Guid CompraId { get; }
    public Guid? ProductoId { get; }
    public string ProductoCodigo { get; }
    public string ProductoNombre { get; }
    public NaturalezaCompra NaturalezaCompra { get; }
    public string? NombreActivo { get; }
    public string? CategoriaSriActivo { get; }
    public string? SerieUbicacionActivo { get; }
    public string CodigoIva { get; }
    public decimal PorcentajeIva { get; }
    public decimal Cantidad { get; }
    public decimal CostoUnitario { get; }
    public decimal Descuento { get; }
    public decimal CostoTotalSinImpuesto { get; }
    public decimal TotalImpuesto => Math.Round(CostoTotalSinImpuesto * (PorcentajeIva / 100m), 2, MidpointRounding.AwayFromZero);
    public decimal TotalLinea => CostoTotalSinImpuesto + TotalImpuesto;
}

