namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityRoleEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public ICollection<SecurityUserRoleEntity> UserRoles { get; set; } = [];

    public ICollection<SecurityRolPermisoEntity> RolePermissions { get; set; } = [];
}
