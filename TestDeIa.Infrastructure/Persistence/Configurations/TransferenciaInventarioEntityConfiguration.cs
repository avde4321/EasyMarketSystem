using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class TransferenciaInventarioEntityConfiguration : IEntityTypeConfiguration<TransferenciaInventarioEntity>
{
    public void Configure(EntityTypeBuilder<TransferenciaInventarioEntity> builder)
    {
        builder.ToTable("TransferenciasInventario");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId).IsRequired();
        builder.Property(entity => entity.Estado).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.MotivoTraslado).HasMaxLength(250).IsRequired();

        builder.HasOne(entity => entity.BodegaOrigen)
            .WithMany(bodega => bodega.TransferenciasOrigen)
            .HasForeignKey(entity => entity.BodegaOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.BodegaDestino)
            .WithMany(bodega => bodega.TransferenciasDestino)
            .HasForeignKey(entity => entity.BodegaDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Detalles)
            .WithOne(detalle => detalle.TransferenciaInventario)
            .HasForeignKey(detalle => detalle.TransferenciaInventarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Estado });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.BodegaOrigenId, entity.BodegaDestinoId });
    }
}
