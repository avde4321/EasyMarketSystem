using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class MovimientoTesoreriaEntityConfiguration : IEntityTypeConfiguration<MovimientoTesoreriaEntity>
{
    public void Configure(EntityTypeBuilder<MovimientoTesoreriaEntity> builder)
    {
        builder.ToTable("MovimientosTesoreria");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Tipo)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(entity => entity.Monto)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.Beneficiario)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entity => entity.EstadoConciliacion)
            .HasConversion<byte>()
            .HasDefaultValue(TestDeIa.Domain.Modules.Tesoreria.Enums.EstadoConciliacionTesoreria.Pendiente)
            .IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.CuentaBancariaId, entity.Fecha });
        builder.HasIndex(entity => new { entity.CuentaBancariaId, entity.Fecha });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.EstadoConciliacion, entity.Fecha });
        builder.HasIndex(entity => entity.FacturaVentaId);
        builder.HasIndex(entity => entity.CompraId);
        builder.HasIndex(entity => entity.AsientoContableId);

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.CuentaBancaria)
            .WithMany(entity => entity.MovimientosTesoreria)
            .HasForeignKey(entity => entity.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.FacturaVenta)
            .WithMany()
            .HasForeignKey(entity => entity.FacturaVentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Compra)
            .WithMany()
            .HasForeignKey(entity => entity.CompraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AsientoContable)
            .WithMany()
            .HasForeignKey(entity => entity.AsientoContableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
