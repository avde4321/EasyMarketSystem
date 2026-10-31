using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class WhatsAppNotificacionLogEntityConfiguration : IEntityTypeConfiguration<WhatsAppNotificacionLogEntity>
{
    public void Configure(EntityTypeBuilder<WhatsAppNotificacionLogEntity> builder)
    {
        builder.ToTable("WhatsAppNotificacionesLog");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.NumeroDestino)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.TipoDocumento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.EstadoEnvio)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entity => entity.MensajeError).HasColumnType("nvarchar(max)");

        builder.HasIndex(entity => new { entity.EmpresaId, entity.EstadoEnvio, entity.FechaEnvio });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.TipoDocumento, entity.DocumentoId });

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
