namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityRolPermisoEntity
{
    public Guid RoleId { get; set; }

    public SecurityRoleEntity Role { get; set; } = default!;

    public string PermisoId { get; set; } = string.Empty;

    public SecurityPermisoEntity Permiso { get; set; } = default!;
}
