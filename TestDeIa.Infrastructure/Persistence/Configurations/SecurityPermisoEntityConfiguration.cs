using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityPermisoEntityConfiguration : IEntityTypeConfiguration<SecurityPermisoEntity>
{
    public void Configure(EntityTypeBuilder<SecurityPermisoEntity> builder)
    {
        builder.ToTable("SecurityPermisos");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasMaxLength(120);

        builder.Property(entity => entity.NombrePermiso)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(entity => entity.Descripcion)
            .HasMaxLength(240)
            .IsRequired();

        builder.Property(entity => entity.Modulo)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(entity => entity.NombrePermiso)
            .IsUnique();

        builder.HasData(SecurityPermissionCatalog.Definitions.Select(current => new SecurityPermisoEntity
        {
            Id = current.Id,
            NombrePermiso = current.Id,
            Descripcion = current.Description,
            Modulo = current.Module
        }));
    }
}
