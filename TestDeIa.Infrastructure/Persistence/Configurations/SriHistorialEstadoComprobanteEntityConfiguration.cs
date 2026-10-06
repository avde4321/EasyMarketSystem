using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SriHistorialEstadoComprobanteEntityConfiguration : IEntityTypeConfiguration<SriHistorialEstadoComprobanteEntity>
{
    public void Configure(EntityTypeBuilder<SriHistorialEstadoComprobanteEntity> builder)
    {
        builder.ToTable("SriHistorialEstadosComprobante");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.EmpresaId).IsRequired();
        builder.Property(entity => entity.TipoDocumentoId).HasMaxLength(10).IsUnicode(false).IsRequired();
        builder.Property(entity => entity.CodigoErrorSri).HasMaxLength(10).IsUnicode(false);
        builder.Property(entity => entity.MensajeRespuesta).HasMaxLength(1000);
        builder.Property(entity => entity.WorkerNode).HasMaxLength(120);
        builder.Property(entity => entity.FechaTransaccion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(entity => entity.EstadoAnterior)
            .WithMany()
            .HasForeignKey(entity => entity.EstadoAnteriorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.EstadoNuevo)
            .WithMany()
            .HasForeignKey(entity => entity.EstadoNuevoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.ComprobanteId, entity.FechaTransaccion });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.EstadoNuevoId, entity.FechaTransaccion });
    }
}
