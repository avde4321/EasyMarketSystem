using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CuentaContableEntityConfiguration : IEntityTypeConfiguration<CuentaContableEntity>
{
    public void Configure(EntityTypeBuilder<CuentaContableEntity> builder)
    {
        builder.ToTable("CuentasContables");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.Codigo)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(entity => entity.Nombre)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(entity => entity.Nivel)
            .IsRequired();

        builder.Property(entity => entity.TipoCuenta)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.EsAceptable)
            .HasDefaultValue(false);

        builder.Property(entity => entity.SaldoActual)
            .HasPrecision(18, 2);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Codigo })
            .IsUnique();
    }
}