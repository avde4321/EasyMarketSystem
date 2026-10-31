namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class GeoSectorEntity
{
    public Guid Id { get; set; }
    public Guid CiudadId { get; set; }
    public GeoCiudadEntity Ciudad { get; set; } = default!;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool IsActive { get; set; } = true;
}
