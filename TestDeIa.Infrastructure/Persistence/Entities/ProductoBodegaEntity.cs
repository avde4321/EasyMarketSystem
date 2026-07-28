namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ProductoBodegaEntity
{
    public Guid ProductoId { get; set; }

    public ProductoEntity Producto { get; set; } = default!;

    public Guid BodegaId { get; set; }

    public BodegaEntity Bodega { get; set; } = default!;

    public Guid EmpresaId { get; set; }

    public decimal StockActual { get; set; }

    public byte[] RowVersion { get; set; } = null!;
}
