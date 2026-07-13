namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityPermisoEntity
{
    public string Id { get; set; } = string.Empty;

    public string NombrePermiso { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Modulo { get; set; } = string.Empty;

    public ICollection<SecurityRolPermisoEntity> RolePermissions { get; set; } = [];
}
