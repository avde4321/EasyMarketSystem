namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class GeoCiudadEntity
{
    public Guid Id { get; set; }
    public Guid ProvinciaId { get; set; }
    public GeoProvinciaEntity Provincia { get; set; } = default!;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<GeoSectorEntity> Sectores { get; set; } = [];
}
