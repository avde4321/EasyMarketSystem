using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

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

        builder.HasData(new SecurityRoleEntity
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Administrador",
            NormalizedName = "ADMINISTRADOR",
            IsActive = true
        });
    }
}
