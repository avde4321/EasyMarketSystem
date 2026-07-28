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

        builder.Property(producto => producto.UnidadMedida)
            .HasMaxLength(60)
            .HasDefaultValue("Unidad")
            .IsRequired();

        builder.Property(producto => producto.NaturalezaItem)
            .HasMaxLength(30)
            .HasDefaultValue("Mercaderia")
            .IsRequired();

        builder.Property(producto => producto.CodigoIva)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(producto => producto.PorcentajeIva)
            .HasPrecision(9, 2);

        builder.Property(producto => producto.PrecioVenta)
            .HasPrecision(18, 6);

        builder.Property(producto => producto.CostoReferencial)
            .HasPrecision(18, 6);

        builder.Property(producto => producto.StockMinimo)
            .HasPrecision(18, 4)
            .IsRequired(false);

        builder.Property(producto => producto.CostoPromedio)
            .HasPrecision(18, 6);

        builder.Property(producto => producto.ControlaStock)
            .HasDefaultValue(true);

        builder.Property(producto => producto.AplicaComision)
            .HasDefaultValue(false);

        builder.Property(producto => producto.TipoComision)
            .HasMaxLength(20);

        builder.Property(producto => producto.ValorComision)
            .HasPrecision(18, 2);

        builder.HasIndex(producto => new { producto.EmpresaId, producto.Codigo })
            .IsUnique();
    }
}
