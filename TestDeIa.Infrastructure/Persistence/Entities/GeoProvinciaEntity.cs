namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class GeoProvinciaEntity
{
    public Guid Id { get; set; }
    public Guid RegionId { get; set; }
    public GeoRegionEntity Region { get; set; } = default!;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<GeoCiudadEntity> Ciudades { get; set; } = [];
}
