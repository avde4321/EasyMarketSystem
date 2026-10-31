using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CuentaBancariaEntityConfiguration : IEntityTypeConfiguration<CuentaBancariaEntity>
{
    public void Configure(EntityTypeBuilder<CuentaBancariaEntity> builder)
    {
        builder.ToTable("CuentasBancarias");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.BancoNombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entity => entity.TipoCuenta)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(entity => entity.NumeroCuenta)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entity => entity.SaldoContable)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.SaldoConciliado)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.Moneda)
            .HasMaxLength(3)
            .IsUnicode(false)
            .HasDefaultValue("USD")
            .IsRequired();

        builder.Property(entity => entity.Activa)
            .HasDefaultValue(true);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.NumeroCuenta });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Activa });

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CuentaContable)
            .WithMany()
            .HasForeignKey(entity => entity.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
