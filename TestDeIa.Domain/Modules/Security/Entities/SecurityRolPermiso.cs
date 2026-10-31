namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityRolPermiso
{
    public SecurityRolPermiso(Guid roleId, string permisoId)
    {
        RoleId = roleId;
        PermisoId = permisoId;
    }

    public Guid RoleId { get; }

    public string PermisoId { get; }
}
