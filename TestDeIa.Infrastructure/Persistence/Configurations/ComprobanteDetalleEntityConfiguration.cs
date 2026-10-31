using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ComprobanteDetalleEntityConfiguration : IEntityTypeConfiguration<ComprobanteDetalleEntity>
{
    public void Configure(EntityTypeBuilder<ComprobanteDetalleEntity> builder)
    {
        builder.ToTable("ComprobanteDetalle");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CodigoProducto).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.NombreProducto).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.CodigoIva).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.PorcentajeIva).HasPrecision(9, 2);
        builder.Property(entity => entity.Cantidad).HasPrecision(18, 4);
        builder.Property(entity => entity.PrecioUnitario).HasPrecision(18, 6);
        builder.Property(entity => entity.Descuento).HasPrecision(18, 2);
        builder.Property(entity => entity.Subtotal).HasPrecision(18, 2);
        builder.Property(entity => entity.IvaValor).HasPrecision(18, 2);
        builder.Property(entity => entity.Total).HasPrecision(18, 2);

        builder.HasOne(entity => entity.ComprobanteCabecera)
            .WithMany(entity => entity.Detalles)
            .HasForeignKey(entity => entity.ComprobanteCabeceraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entity => entity.FacturaDetalleOrigenId);
    }
}
