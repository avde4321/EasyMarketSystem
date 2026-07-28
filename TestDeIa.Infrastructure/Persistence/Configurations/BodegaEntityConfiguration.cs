using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class BodegaEntityConfiguration : IEntityTypeConfiguration<BodegaEntity>
{
    public void Configure(EntityTypeBuilder<BodegaEntity> builder)
    {
        builder.ToTable("Bodegas");

        builder.HasKey(bodega => bodega.Id);

        builder.Property(bodega => bodega.EmpresaId)
            .IsRequired();

        builder.Property(bodega => bodega.Codigo)
            .HasMaxLength(3)
            .HasDefaultValue("001")
            .IsRequired();

        builder.Property(bodega => bodega.Nombre)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(bodega => bodega.Direccion)
            .HasMaxLength(250);

        builder.HasIndex(bodega => new { bodega.EmpresaId, bodega.Nombre })
            .IsUnique();

        builder.HasIndex(bodega => new { bodega.EmpresaId, bodega.Codigo })
            .IsUnique();
    }
}
