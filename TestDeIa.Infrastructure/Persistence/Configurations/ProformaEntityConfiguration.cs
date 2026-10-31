using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ProformaEntityConfiguration : IEntityTypeConfiguration<ProformaEntity>
{
    public void Configure(EntityTypeBuilder<ProformaEntity> builder)
    {
        builder.ToTable("Proformas");

        builder.HasKey(proforma => proforma.Id);

        builder.Property(proforma => proforma.EmpresaId)
            .IsRequired();

        builder.Property(proforma => proforma.ClienteId)
            .IsRequired();

        builder.Property(proforma => proforma.UsuarioId)
            .IsRequired();

        builder.Property(proforma => proforma.BodegaId)
            .IsRequired();

        builder.Property(proforma => proforma.Secuencial)
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(proforma => proforma.Estado)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(proforma => proforma.SubtotalSinImpuestos).HasPrecision(18, 4);
        builder.Property(proforma => proforma.SubtotalIVA).HasPrecision(18, 4);
        builder.Property(proforma => proforma.DescuentoTotal).HasPrecision(18, 4);
        builder.Property(proforma => proforma.Total).HasPrecision(18, 4);

        builder.Property(proforma => proforma.Observacion)
            .HasMaxLength(500);

        builder.HasIndex(proforma => new { proforma.EmpresaId, proforma.Secuencial })
            .IsUnique();

        builder.HasIndex(proforma => new { proforma.EmpresaId, proforma.ClienteId, proforma.FechaEmision });
        builder.HasIndex(proforma => new { proforma.EmpresaId, proforma.Estado, proforma.FechaEmision });

        builder.HasOne<EmpresaEmisoraEntity>()
            .WithMany()
            .HasForeignKey(proforma => proforma.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClienteEntity>()
            .WithMany()
            .HasForeignKey(proforma => proforma.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SecurityUserEntity>()
            .WithMany()
            .HasForeignKey(proforma => proforma.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BodegaEntity>()
            .WithMany()
            .HasForeignKey(proforma => proforma.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FacturaEntity>()
            .WithMany()
            .HasForeignKey(proforma => proforma.FacturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(proforma => proforma.Detalles)
            .WithOne(detalle => detalle.Proforma)
            .HasForeignKey(detalle => detalle.ProformaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
