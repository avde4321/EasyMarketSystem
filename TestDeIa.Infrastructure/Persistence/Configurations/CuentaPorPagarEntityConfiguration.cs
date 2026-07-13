using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CuentaPorPagarEntityConfiguration : IEntityTypeConfiguration<CuentaPorPagarEntity>
{
    public void Configure(EntityTypeBuilder<CuentaPorPagarEntity> builder)
    {
        builder.ToTable("CuentasPorPagar");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.EstadoDeuda)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.MontoOriginal)
            .HasColumnType("decimal(18,2)");

        builder.Property(entity => entity.SaldoActual)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(entity => new { entity.EmpresaId, entity.ProveedorId, entity.FechaVence });

        builder.HasOne(entity => entity.Compra)
            .WithMany()
            .HasForeignKey(entity => entity.CompraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Proveedor)
            .WithMany()
            .HasForeignKey(entity => entity.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Pagos)
            .WithOne(entity => entity.CuentaPorPagar)
            .HasForeignKey(entity => entity.CuentaPorPagarId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
