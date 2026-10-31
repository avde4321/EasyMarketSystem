using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CajaMovimientoEntityConfiguration : IEntityTypeConfiguration<CajaMovimientoEntity>
{
    public void Configure(EntityTypeBuilder<CajaMovimientoEntity> builder)
    {
        builder.ToTable("CajaMovimientos");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.TipoMovimiento)
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.Monto)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.Concepto)
            .HasMaxLength(250)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.ComprobanteReferencia)
            .HasMaxLength(80)
            .IsUnicode(false);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.CajaSesionId, entity.FechaMovimiento });

        builder.HasOne(entity => entity.CajaSesion)
            .WithMany()
            .HasForeignKey(entity => entity.CajaSesionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
