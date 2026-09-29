using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ExtractoBancarioHeaderEntityConfiguration : IEntityTypeConfiguration<ExtractoBancarioHeaderEntity>
{
    public void Configure(EntityTypeBuilder<ExtractoBancarioHeaderEntity> builder)
    {
        builder.ToTable("ExtractoBancarioHeaders");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.NombreArchivoOriginal)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(entity => entity.Observaciones)
            .HasMaxLength(1000);

        builder.HasIndex(entity => new { entity.CuentaBancariaId, entity.FechaDesde, entity.FechaHasta });
        builder.HasIndex(entity => entity.FechaImportacion);

        builder.HasOne(entity => entity.CuentaBancaria)
            .WithMany(entity => entity.Extractos)
            .HasForeignKey(entity => entity.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
