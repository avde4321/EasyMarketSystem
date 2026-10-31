using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ProformaDetalleEntityConfiguration : IEntityTypeConfiguration<ProformaDetalleEntity>
{
    public void Configure(EntityTypeBuilder<ProformaDetalleEntity> builder)
    {
        builder.ToTable("ProformaDetalles");

        builder.HasKey(detalle => detalle.Id);

        builder.Property(detalle => detalle.Cantidad).HasPrecision(18, 4);
        builder.Property(detalle => detalle.PrecioUnitario).HasPrecision(18, 4);
        builder.Property(detalle => detalle.Descuento).HasPrecision(18, 4);
        builder.Property(detalle => detalle.TarifaIVA).HasPrecision(18, 4);
        builder.Property(detalle => detalle.ValorIVA).HasPrecision(18, 4);
        builder.Property(detalle => detalle.Subtotal).HasPrecision(18, 4);

        builder.HasIndex(detalle => new { detalle.ProformaId, detalle.ProductoId });

        builder.HasOne<ProductoEntity>()
            .WithMany()
            .HasForeignKey(detalle => detalle.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
