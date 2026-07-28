using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class AsientoContableEntityConfiguration : IEntityTypeConfiguration<AsientoContableEntity>
{
    public void Configure(EntityTypeBuilder<AsientoContableEntity> builder)
    {
        builder.ToTable("AsientosContables");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.NumeroAsiento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.FechaContable)
            .IsRequired();

        builder.Property(entity => entity.Concepto)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(entity => entity.ModuloOrigen)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.DocumentoSoporte)
            .HasMaxLength(49);

        builder.Property(entity => entity.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.NumeroAsiento })
            .IsUnique();

        builder.HasMany(entity => entity.Detalles)
            .WithOne(entity => entity.AsientoContable)
            .HasForeignKey(entity => entity.AsientoContableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
