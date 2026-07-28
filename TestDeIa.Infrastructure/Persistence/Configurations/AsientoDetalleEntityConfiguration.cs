using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class AsientoDetalleEntityConfiguration : IEntityTypeConfiguration<AsientoDetalleEntity>
{
    public void Configure(EntityTypeBuilder<AsientoDetalleEntity> builder)
    {
        builder.ToTable("AsientosDetalle");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Debe)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(entity => entity.Haber)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(entity => entity.CuentaContable)
            .WithMany()
            .HasForeignKey(entity => entity.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

