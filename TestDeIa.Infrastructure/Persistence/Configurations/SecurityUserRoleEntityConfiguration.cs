using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityUserRoleEntityConfiguration : IEntityTypeConfiguration<SecurityUserRoleEntity>
{
    public void Configure(EntityTypeBuilder<SecurityUserRoleEntity> builder)
    {
        builder.ToTable("SecurityUserRoles");

        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

        builder.HasOne(userRole => userRole.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId);

        builder.HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId);

        builder.HasData(new SecurityUserRoleEntity
        {
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            RoleId = SecuritySeedIds.AdministradorRoleId
        });
    }
}
