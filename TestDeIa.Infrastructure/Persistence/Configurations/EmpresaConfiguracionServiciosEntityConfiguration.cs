using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EmpresaConfiguracionServiciosEntityConfiguration : IEntityTypeConfiguration<EmpresaConfiguracionServiciosEntity>
{
    public void Configure(EntityTypeBuilder<EmpresaConfiguracionServiciosEntity> builder)
    {
        builder.ToTable("EmpresaConfiguracionServicios");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.WhatsAppApiToken).HasMaxLength(600);
        builder.Property(entity => entity.WhatsAppPhoneId).HasMaxLength(80);
        builder.Property(entity => entity.WhatsAppBusinessAccountId).HasMaxLength(80);
        builder.Property(entity => entity.PayPhoneToken).HasMaxLength(600);
        builder.Property(entity => entity.PayPhoneClientAppId).HasMaxLength(120);

        builder.Property(entity => entity.PasarelaPagoActiva)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(entity => entity.ModopagosAmbiente)
            .HasConversion<byte>()
            .IsRequired();

        builder.HasIndex(entity => entity.EmpresaId)
            .IsUnique();

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
