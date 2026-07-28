using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CompraEntityConfiguration : IEntityTypeConfiguration<CompraEntity>
{
    public void Configure(EntityTypeBuilder<CompraEntity> builder)
    {
        builder.ToTable("Compras");

        builder.HasKey(compra => compra.Id);

        builder.Property(compra => compra.EmpresaId)
            .IsRequired();

        builder.HasIndex(compra => new { compra.EmpresaId, compra.TipoDocumentoCodigo, compra.Establecimiento, compra.PuntoEmision, compra.Secuencial })
            .IsUnique();

        builder.Property(compra => compra.TipoDocumentoCodigo)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(compra => compra.NaturalezaCompra)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(compra => compra.TipoComprobanteSRI)
            .HasMaxLength(2)
            .HasDefaultValue("01")
            .IsRequired();

        builder.Property(compra => compra.SustentoTributarioSRI)
            .HasMaxLength(2)
            .HasDefaultValue("01")
            .IsRequired();

        builder.Property(compra => compra.Establecimiento)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(compra => compra.PuntoEmision)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(compra => compra.Secuencial)
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(compra => compra.ClaveAccesoProveedor)
            .HasMaxLength(49);

        builder.Property(compra => compra.ClaveAccesoGenerada)
            .HasMaxLength(49);

        builder.Property(compra => compra.NumeroAutorizacion)
            .HasMaxLength(64);

        builder.Property(compra => compra.MensajeEstado)
            .HasMaxLength(500);

        builder.Property(compra => compra.FormaPagoSriCodigo)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(compra => compra.FormaPagoCompra)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(compra => compra.RequiereBancarizacion)
            .IsRequired();

        builder.Property(compra => compra.Observacion)
            .HasMaxLength(500);

        builder.Property(compra => compra.EstadoCompra)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(compra => compra.SubtotalIva0).HasColumnType("decimal(18,2)");
        builder.Property(compra => compra.SubtotalIva5).HasColumnType("decimal(18,2)");
        builder.Property(compra => compra.SubtotalIva8).HasColumnType("decimal(18,2)");
        builder.Property(compra => compra.SubtotalIva15).HasColumnType("decimal(18,2)");
        builder.Property(compra => compra.TotalDescuento).HasColumnType("decimal(18,2)");
        builder.Property(compra => compra.TotalImpuestos).HasColumnType("decimal(18,2)");
        builder.Property(compra => compra.ImporteTotal).HasColumnType("decimal(18,2)");

        builder.HasOne(compra => compra.Proveedor)
            .WithMany()
            .HasForeignKey(compra => compra.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(compra => compra.Bodega)
            .WithMany()
            .HasForeignKey(compra => compra.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(compra => compra.Detalles)
            .WithOne(detalle => detalle.Compra)
            .HasForeignKey(detalle => detalle.CompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
