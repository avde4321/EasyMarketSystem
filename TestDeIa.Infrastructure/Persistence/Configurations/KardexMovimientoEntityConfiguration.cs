using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class KardexMovimientoEntityConfiguration : IEntityTypeConfiguration<KardexMovimientoEntity>
{
    public void Configure(EntityTypeBuilder<KardexMovimientoEntity> builder)
    {
        builder.ToTable("KardexMovimientos");

        builder.HasKey(movimiento => movimiento.Id);

        builder.Property(movimiento => movimiento.EmpresaId)
            .IsRequired();

        builder.Property(movimiento => movimiento.TipoMovimiento)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(movimiento => movimiento.Concepto)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(movimiento => movimiento.Referencia)
            .HasMaxLength(80);

        builder.Property(movimiento => movimiento.CantidadEntrada)
            .HasPrecision(18, 4);

        builder.Property(movimiento => movimiento.CantidadSalida)
            .HasPrecision(18, 4);

        builder.Property(movimiento => movimiento.SaldoCantidad)
            .HasPrecision(18, 4);

        builder.Property(movimiento => movimiento.CostoUnitario)
            .HasPrecision(18, 6);

        builder.Property(movimiento => movimiento.CostoPromedio)
            .HasPrecision(18, 6);

        builder.Property(movimiento => movimiento.SaldoValor)
            .HasPrecision(18, 6);

        builder.HasOne(movimiento => movimiento.Bodega)
            .WithMany(bodega => bodega.KardexMovimientos)
            .HasForeignKey(movimiento => movimiento.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movimiento => movimiento.Producto)
            .WithMany(producto => producto.KardexMovimientos)
            .HasForeignKey(movimiento => movimiento.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(movimiento => new { movimiento.ProductoId, movimiento.BodegaId, movimiento.FechaMovimiento });
    }
}
