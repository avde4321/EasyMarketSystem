using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ProductoBodegaEntityConfiguration : IEntityTypeConfiguration<ProductoBodegaEntity>
{
    public void Configure(EntityTypeBuilder<ProductoBodegaEntity> builder)
    {
        builder.ToTable("ProductosBodega");

        builder.HasKey(entity => new { entity.ProductoId, entity.BodegaId });

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.StockActual)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(entity => entity.StockMinimo)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(entity => entity.StockMaximo)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(entity => entity.EsActivo)
            .HasDefaultValue(true);

        builder.Property(entity => entity.RowVersion)
            .IsRowVersion();

        builder.HasOne(entity => entity.Producto)
            .WithMany(producto => producto.ProductosBodega)
            .HasForeignKey(entity => entity.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Bodega)
            .WithMany(bodega => bodega.ProductosBodega)
            .HasForeignKey(entity => entity.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.BodegaId });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.BodegaId, entity.ProductoId });
    }
}
