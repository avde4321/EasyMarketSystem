using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class FacturaPagoEntityConfiguration : IEntityTypeConfiguration<FacturaPagoEntity>
{
    public void Configure(EntityTypeBuilder<FacturaPagoEntity> builder)
    {
        builder.ToTable("FacturaPagos");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.FormaPagoCodigo)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.Monto)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.LoteNumero)
            .HasMaxLength(80)
            .IsUnicode(false);

        builder.Property(entity => entity.VoucherNumero)
            .HasMaxLength(80)
            .IsUnicode(false);

        builder.Property(entity => entity.BancoNombre)
            .HasMaxLength(120);

        builder.Property(entity => entity.NumeroReferencia)
            .HasMaxLength(120)
            .IsUnicode(false);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.CajaSesionId, entity.FacturaId });

        builder.HasOne(entity => entity.Factura)
            .WithMany()
            .HasForeignKey(entity => entity.FacturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CajaSesion)
            .WithMany()
            .HasForeignKey(entity => entity.CajaSesionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
