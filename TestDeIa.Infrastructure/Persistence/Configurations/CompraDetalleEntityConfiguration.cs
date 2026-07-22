using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CompraDetalleEntityConfiguration : IEntityTypeConfiguration<CompraDetalleEntity>
{
    public void Configure(EntityTypeBuilder<CompraDetalleEntity> builder)
    {
        builder.ToTable("CompraDetalles");

        builder.HasKey(detalle => detalle.Id);

        builder.Property(detalle => detalle.EmpresaId)
            .IsRequired();

        builder.Property(detalle => detalle.ProductoCodigo)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(detalle => detalle.ProductoNombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(detalle => detalle.NaturalezaCompra)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(detalle => detalle.NombreActivo)
            .HasMaxLength(200);

        builder.Property(detalle => detalle.CategoriaSriActivo)
            .HasMaxLength(80);

        builder.Property(detalle => detalle.SerieUbicacionActivo)
            .HasMaxLength(120);

        builder.Property(detalle => detalle.CodigoIva)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(detalle => detalle.PorcentajeIva).HasColumnType("decimal(8,2)");
        builder.Property(detalle => detalle.Cantidad).HasColumnType("decimal(18,4)");
        builder.Property(detalle => detalle.CostoUnitario).HasColumnType("decimal(18,6)");
        builder.Property(detalle => detalle.Descuento).HasColumnType("decimal(18,2)");
        builder.Property(detalle => detalle.CostoTotalSinImpuesto).HasColumnType("decimal(18,2)");

        builder.HasOne(detalle => detalle.Producto)
            .WithMany()
            .HasForeignKey(detalle => detalle.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(detalle => new { detalle.EmpresaId, detalle.FechaEmisionCompra, detalle.ProductoId });
    }
}
