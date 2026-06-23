using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class FacturaSriEventoEntityConfiguration : IEntityTypeConfiguration<FacturaSriEventoEntity>
{
    public void Configure(EntityTypeBuilder<FacturaSriEventoEntity> builder)
    {
        builder.ToTable("FacturaSriEventos");

        builder.HasKey(evento => evento.Id);

        builder.Property(evento => evento.Estado)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(evento => evento.Mensaje)
            .HasMaxLength(400)
            .IsRequired();

        builder.HasOne(evento => evento.Factura)
            .WithMany(factura => factura.EventosSri)
            .HasForeignKey(evento => evento.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(evento => new { evento.FacturaId, evento.CreatedAt });
    }
}
