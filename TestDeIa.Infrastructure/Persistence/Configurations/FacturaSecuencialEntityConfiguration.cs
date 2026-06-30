using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class FacturaSecuencialEntityConfiguration : IEntityTypeConfiguration<FacturaSecuencialEntity>
{
    public void Configure(EntityTypeBuilder<FacturaSecuencialEntity> builder)
    {
        builder.ToTable("FacturaSecuenciales");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.Establecimiento)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(entity => entity.PuntoEmision)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(entity => entity.UltimoSecuencial)
            .IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Establecimiento, entity.PuntoEmision })
            .IsUnique();
    }
}
