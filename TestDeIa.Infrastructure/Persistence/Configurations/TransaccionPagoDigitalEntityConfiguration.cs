using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class TransaccionPagoDigitalEntityConfiguration : IEntityTypeConfiguration<TransaccionPagoDigitalEntity>
{
    public void Configure(EntityTypeBuilder<TransaccionPagoDigitalEntity> builder)
    {
        builder.ToTable("TransaccionesPagosDigitales");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Monto).HasPrecision(18, 4);

        builder.Property(entity => entity.Pasarela)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.TransactionIdPasarela)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entity => entity.EstadoPago)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entity => entity.LinkPagoUrl).HasMaxLength(500);
        builder.Property(entity => entity.QrCodeBase64).HasColumnType("nvarchar(max)");

        builder.HasIndex(entity => new { entity.EmpresaId, entity.EstadoPago, entity.FechaCreacion });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.TransactionIdPasarela })
            .IsUnique()
            .HasFilter("[TransactionIdPasarela] <> ''");
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FacturaId });

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Factura)
            .WithMany()
            .HasForeignKey(entity => entity.FacturaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(entity => entity.Cliente)
            .WithMany()
            .HasForeignKey(entity => entity.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
