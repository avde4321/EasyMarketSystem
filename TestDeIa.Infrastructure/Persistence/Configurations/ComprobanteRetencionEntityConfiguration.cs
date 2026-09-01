using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ComprobanteRetencionEntityConfiguration : IEntityTypeConfiguration<ComprobanteRetencionEntity>
{
    public void Configure(EntityTypeBuilder<ComprobanteRetencionEntity> builder)
    {
        builder.ToTable("ComprobantesRetencion");

        builder.HasKey(retencion => retencion.Id);

        builder.Property(retencion => retencion.EmpresaId)
            .IsRequired();

        builder.Property(retencion => retencion.ProveedorId)
            .IsRequired();

        builder.Property(retencion => retencion.Establecimiento)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(retencion => retencion.PuntoEmision)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(retencion => retencion.Secuencial)
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(retencion => retencion.ClaveAcceso)
            .HasMaxLength(49);

        builder.Property(retencion => retencion.AmbienteSRI)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(retencion => retencion.EstadoSRI)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(retencion => retencion.NumeroAutorizacion)
            .HasMaxLength(49);

        builder.Property(retencion => retencion.MensajeErrorSRI)
            .HasColumnType("nvarchar(max)");

        builder.Property(retencion => retencion.TotalRetenido).HasPrecision(18, 4);

        builder.HasIndex(retencion => new { retencion.EmpresaId, retencion.Establecimiento, retencion.PuntoEmision, retencion.Secuencial })
            .IsUnique();

        builder.HasIndex(retencion => retencion.ClaveAcceso)
            .IsUnique()
            .HasFilter("[ClaveAcceso] IS NOT NULL");

        builder.HasIndex(retencion => new { retencion.EmpresaId, retencion.ProveedorId, retencion.FechaEmision });
        builder.HasIndex(retencion => new { retencion.EmpresaId, retencion.EstadoSRI, retencion.FechaEmision });

        builder.HasOne<EmpresaEmisoraEntity>()
            .WithMany()
            .HasForeignKey(retencion => retencion.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CompraEntity>()
            .WithMany()
            .HasForeignKey(retencion => retencion.CompraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProveedorEntity>()
            .WithMany()
            .HasForeignKey(retencion => retencion.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(retencion => retencion.Detalles)
            .WithOne(detalle => detalle.ComprobanteRetencion)
            .HasForeignKey(detalle => detalle.ComprobanteRetencionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
