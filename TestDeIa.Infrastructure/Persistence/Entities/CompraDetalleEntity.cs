using TestDeIa.Domain.Modules.Compras.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CompraDetalleEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid CompraId { get; set; }
    public CompraEntity Compra { get; set; } = default!;
    public DateTimeOffset FechaEmisionCompra { get; set; }
    public Guid? ProductoId { get; set; }
    public ProductoEntity? Producto { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public NaturalezaCompra NaturalezaCompra { get; set; } = NaturalezaCompra.MercaderiaInventario;
    public string? NombreActivo { get; set; }
    public string? CategoriaSriActivo { get; set; }
    public string? SerieUbicacionActivo { get; set; }
    public string CodigoIva { get; set; } = string.Empty;
    public decimal PorcentajeIva { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal CostoTotalSinImpuesto { get; set; }
}




