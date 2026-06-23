using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class FacturaDetalleEntityConfiguration : IEntityTypeConfiguration<FacturaDetalleEntity>
{
    public void Configure(EntityTypeBuilder<FacturaDetalleEntity> builder)
    {
        builder.ToTable("FacturaDetalles");

        builder.HasKey(detalle => detalle.Id);

        builder.Property(detalle => detalle.CodigoProducto)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(detalle => detalle.NombreProducto)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(detalle => detalle.CodigoIva)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(detalle => detalle.PorcentajeIva).HasPrecision(9, 2);
        builder.Property(detalle => detalle.Cantidad).HasPrecision(18, 4);
        builder.Property(detalle => detalle.PrecioUnitario).HasPrecision(18, 6);
        builder.Property(detalle => detalle.Subtotal).HasPrecision(18, 2);
        builder.Property(detalle => detalle.IvaValor).HasPrecision(18, 2);
        builder.Property(detalle => detalle.Total).HasPrecision(18, 2);

        builder.HasOne(detalle => detalle.Factura)
            .WithMany(factura => factura.Detalles)
            .HasForeignKey(detalle => detalle.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
