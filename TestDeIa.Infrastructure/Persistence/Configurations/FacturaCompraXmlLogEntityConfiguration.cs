using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class FacturaCompraXmlLogEntityConfiguration : IEntityTypeConfiguration<FacturaCompraXmlLogEntity>
{
    public void Configure(EntityTypeBuilder<FacturaCompraXmlLogEntity> builder)
    {
        builder.ToTable("FacturaCompraXmlLogs");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.ClaveAcceso)
            .HasMaxLength(49)
            .IsRequired();

        builder.Property(entity => entity.RucEmisor)
            .HasMaxLength(13)
            .IsRequired();

        builder.Property(entity => entity.RazonSocialEmisor)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(entity => entity.RucComprador)
            .HasMaxLength(13)
            .IsRequired();

        builder.Property(entity => entity.CodDoc)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(entity => entity.EstabPuntoEmiSecuencial)
            .HasMaxLength(17)
            .IsRequired();

        builder.Property(entity => entity.TotalSinImpuestos)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.TotalDescuento)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.ImporteTotal)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.XmlContenido)
            .IsRequired();

        builder.Property(entity => entity.EstadoProcesamiento)
            .HasConversion<byte>();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.ClaveAcceso })
            .IsUnique();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.EstadoProcesamiento, entity.FechaEmision });

        builder.HasOne(entity => entity.Compra)
            .WithMany()
            .HasForeignKey(entity => entity.CompraId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
