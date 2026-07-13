using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityRolPermisoEntityConfiguration : IEntityTypeConfiguration<SecurityRolPermisoEntity>
{
    public void Configure(EntityTypeBuilder<SecurityRolPermisoEntity> builder)
    {
        builder.ToTable("SecurityRolPermisos");

        builder.HasKey(entity => new { entity.RoleId, entity.PermisoId });

        builder.Property(entity => entity.PermisoId)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasOne(entity => entity.Role)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(entity => entity.RoleId);

        builder.HasOne(entity => entity.Permiso)
            .WithMany(permiso => permiso.RolePermissions)
            .HasForeignKey(entity => entity.PermisoId);

        builder.HasData(BuildSeeds());
    }

    private static IEnumerable<SecurityRolPermisoEntity> BuildSeeds()
    {
        var roleIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
        {
            [SecurityRoleNames.Administrador] = SecuritySeedIds.AdministradorRoleId,
            [SecurityRoleNames.Cajero] = SecuritySeedIds.CajeroRoleId,
            [SecurityRoleNames.Bodeguero] = SecuritySeedIds.BodegueroRoleId,
            [SecurityRoleNames.Contador] = SecuritySeedIds.ContadorRoleId
        };

        return SecurityPermissionCatalog.RoleMappings
            .SelectMany(role => role.Value.Select(permission => new SecurityRolPermisoEntity
            {
                RoleId = roleIds[role.Key],
                PermisoId = permission
            }))
            .ToArray();
    }
}
