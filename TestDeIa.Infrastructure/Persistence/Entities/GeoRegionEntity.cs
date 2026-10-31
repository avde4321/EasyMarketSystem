namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class GeoRegionEntity
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<GeoProvinciaEntity> Provincias { get; set; } = [];
}
