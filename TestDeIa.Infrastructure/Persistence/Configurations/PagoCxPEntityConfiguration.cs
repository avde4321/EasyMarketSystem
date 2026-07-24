using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class PagoCxPEntityConfiguration : IEntityTypeConfiguration<PagoCxPEntity>
{
    public void Configure(EntityTypeBuilder<PagoCxPEntity> builder)
    {
        builder.ToTable("PagosCxP");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.MontoPagado)
            .HasColumnType("decimal(18,2)");

        builder.Property(entity => entity.FormaPago)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(entity => entity.ReferenciaTransaccion)
            .HasMaxLength(100);

        builder.Property(entity => entity.NumeroComprobantePago)
            .HasMaxLength(120);

        builder.HasOne(entity => entity.CuentaContableSalida)
            .WithMany()
            .HasForeignKey(entity => entity.CuentaContableSalidaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.CuentaPorPagarId, entity.FechaPago });
    }
}
