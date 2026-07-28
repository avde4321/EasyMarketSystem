using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class TransferenciaInventarioDetalleEntityConfiguration : IEntityTypeConfiguration<TransferenciaInventarioDetalleEntity>
{
    public void Configure(EntityTypeBuilder<TransferenciaInventarioDetalleEntity> builder)
    {
        builder.ToTable("TransferenciasInventarioDetalle");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.CantidadEnviada).HasPrecision(18, 4);
        builder.Property(entity => entity.CantidadRecibida).HasPrecision(18, 4);
        builder.Property(entity => entity.CostoUnitario).HasPrecision(18, 6);

        builder.HasOne(entity => entity.Producto)
            .WithMany()
            .HasForeignKey(entity => entity.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.TransferenciaInventarioId, entity.ProductoId })
            .IsUnique();
    }
}
