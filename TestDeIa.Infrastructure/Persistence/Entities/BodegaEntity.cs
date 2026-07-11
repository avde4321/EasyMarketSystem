namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class BodegaEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ProductoBodegaEntity> ProductosBodega { get; set; } = [];

    public ICollection<KardexMovimientoEntity> KardexMovimientos { get; set; } = [];
}
