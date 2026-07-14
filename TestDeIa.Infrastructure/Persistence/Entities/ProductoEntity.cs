namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ProductoEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string CodigoIva { get; set; } = string.Empty;

    public decimal PorcentajeIva { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal? StockMinimo { get; set; }

    public decimal CostoPromedio { get; set; }

    public bool ControlaStock { get; set; } = true;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ProductoBodegaEntity> ProductosBodega { get; set; } = [];
    public ICollection<KardexMovimientoEntity> KardexMovimientos { get; set; } = [];
}

