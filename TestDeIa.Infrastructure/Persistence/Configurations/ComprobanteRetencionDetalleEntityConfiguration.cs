using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ComprobanteRetencionDetalleEntityConfiguration : IEntityTypeConfiguration<ComprobanteRetencionDetalleEntity>
{
    public void Configure(EntityTypeBuilder<ComprobanteRetencionDetalleEntity> builder)
    {
        builder.ToTable("ComprobanteRetencionDetalles");

        builder.HasKey(detalle => detalle.Id);

        builder.Property(detalle => detalle.CodigoImpuesto)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(detalle => detalle.CodigoRetencionSRI)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(detalle => detalle.CodDocSustento)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(detalle => detalle.NumDocSustento)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(detalle => detalle.BaseImponible).HasPrecision(18, 4);
        builder.Property(detalle => detalle.PorcentajeRetencion).HasPrecision(5, 2);
        builder.Property(detalle => detalle.ValorRetenido).HasPrecision(18, 4);

        builder.HasIndex(detalle => new
        {
            detalle.ComprobanteRetencionId,
            detalle.CodigoImpuesto,
            detalle.CodigoRetencionSRI
        });
    }
}
