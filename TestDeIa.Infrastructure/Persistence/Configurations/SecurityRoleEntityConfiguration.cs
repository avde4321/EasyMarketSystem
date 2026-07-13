using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityRoleEntityConfiguration : IEntityTypeConfiguration<SecurityRoleEntity>
{
    public void Configure(EntityTypeBuilder<SecurityRoleEntity> builder)
    {
        builder.ToTable("SecurityRoles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(role => role.NormalizedName)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(role => role.NormalizedName)
            .IsUnique();

        builder.HasData(
            new SecurityRoleEntity
            {
                Id = SecuritySeedIds.AdministradorRoleId,
                Name = SecurityRoleNames.Administrador,
                NormalizedName = SecurityRoleNames.Administrador.ToUpperInvariant(),
                IsActive = true
            },
            new SecurityRoleEntity
            {
                Id = SecuritySeedIds.CajeroRoleId,
                Name = SecurityRoleNames.Cajero,
                NormalizedName = SecurityRoleNames.Cajero.ToUpperInvariant(),
                IsActive = true
            },
            new SecurityRoleEntity
            {
                Id = SecuritySeedIds.BodegueroRoleId,
                Name = SecurityRoleNames.Bodeguero,
                NormalizedName = SecurityRoleNames.Bodeguero.ToUpperInvariant(),
                IsActive = true
            },
            new SecurityRoleEntity
            {
                Id = SecuritySeedIds.ContadorRoleId,
                Name = SecurityRoleNames.Contador,
                NormalizedName = SecurityRoleNames.Contador.ToUpperInvariant(),
                IsActive = true
            });
    }
}
