using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Domain.Modules.Inventario;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class KardexMovimientoEntityConfiguration : IEntityTypeConfiguration<KardexMovimientoEntity>
{
    public void Configure(EntityTypeBuilder<KardexMovimientoEntity> builder)
    {
        builder.ToTable("KardexMovimientos", table =>
        {
            table.HasCheckConstraint(
                "CK_KardexMovimientos_TipoMovimiento",
                $"[TipoMovimiento] IN ('{TipoMovimientoInventario.EntradaCompra}', '{TipoMovimientoInventario.SalidaVenta}', '{TipoMovimientoInventario.DevolucionVenta}', '{TipoMovimientoInventario.AjusteIngreso}', '{TipoMovimientoInventario.AjusteEgreso}', '{TipoMovimientoInventario.TransferenciaEntrada}', '{TipoMovimientoInventario.TransferenciaSalida}', '{TipoMovimientoInventario.MermaInventario}', '{TipoMovimientoInventario.TomaFisica}')");
        });

        builder.HasKey(movimiento => movimiento.Id);

        builder.Property(movimiento => movimiento.EmpresaId)
            .IsRequired();

        builder.Property(movimiento => movimiento.TipoMovimiento)
            .HasColumnType("varchar(30)")
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

        builder.Property(movimiento => movimiento.CostoTotal)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(movimiento => movimiento.CostoPromedio)
            .HasPrecision(18, 6);

        builder.Property(movimiento => movimiento.StockAnterior)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(movimiento => movimiento.StockNuevo)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(movimiento => movimiento.SaldoValor)
            .HasPrecision(18, 6);

        builder.Property(movimiento => movimiento.FechaMovimiento)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(movimiento => movimiento.Bodega)
            .WithMany(bodega => bodega.KardexMovimientos)
            .HasForeignKey(movimiento => movimiento.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movimiento => movimiento.Producto)
            .WithMany(producto => producto.KardexMovimientos)
            .HasForeignKey(movimiento => movimiento.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FacturaEntity>()
            .WithMany()
            .HasForeignKey(movimiento => movimiento.FacturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CompraEntity>()
            .WithMany()
            .HasForeignKey(movimiento => movimiento.CompraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TransferenciaInventarioEntity>()
            .WithMany()
            .HasForeignKey(movimiento => movimiento.TransferenciaInventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(movimiento => new { movimiento.ProductoId, movimiento.BodegaId, movimiento.FechaMovimiento });
        builder.HasIndex(movimiento => new { movimiento.EmpresaId, movimiento.FechaMovimiento, movimiento.BodegaId, movimiento.ProductoId });
        builder.HasIndex(movimiento => new { movimiento.EmpresaId, movimiento.BodegaId, movimiento.ProductoId, movimiento.FechaMovimiento });
    }
}
