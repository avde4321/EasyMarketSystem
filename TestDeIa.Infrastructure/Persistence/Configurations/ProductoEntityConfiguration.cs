using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ProductoEntityConfiguration : IEntityTypeConfiguration<ProductoEntity>
{
    public void Configure(EntityTypeBuilder<ProductoEntity> builder)
    {
        builder.ToTable("Productos");

        builder.HasKey(producto => producto.Id);

        builder.Property(producto => producto.EmpresaId)
            .IsRequired();

        builder.Property(producto => producto.Codigo)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(producto => producto.Nombre)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(producto => producto.Descripcion)
            .HasMaxLength(300);

        builder.Property(producto => producto.CodigoIva)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(producto => producto.PorcentajeIva)
            .HasPrecision(9, 2);

        builder.Property(producto => producto.PrecioVenta)
            .HasPrecision(18, 6);

        builder.Property(producto => producto.StockMinimo)
            .HasPrecision(18, 4)
            .IsRequired(false);

        builder.Property(producto => producto.CostoPromedio)
            .HasPrecision(18, 6);

        builder.Property(producto => producto.ControlaStock)
            .HasDefaultValue(true);

        builder.HasIndex(producto => new { producto.EmpresaId, producto.Codigo })
            .IsUnique();
    }
}