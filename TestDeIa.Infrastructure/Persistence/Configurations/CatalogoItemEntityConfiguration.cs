using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CatalogoItemEntityConfiguration : IEntityTypeConfiguration<CatalogoItemEntity>
{
    public void Configure(EntityTypeBuilder<CatalogoItemEntity> builder)
    {
        builder.ToTable("CatalogoItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Codigo)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.Nombre)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Descripcion)
            .HasMaxLength(250);

        builder.HasOne(item => item.ParentItem)
            .WithMany(item => item.Children)
            .HasForeignKey(item => item.ParentItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => new { item.CatalogoId, item.Codigo })
            .IsUnique();

        builder.HasOne(item => item.Catalogo)
            .WithMany(catalogo => catalogo.Items)
            .HasForeignKey(item => item.CatalogoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(CatalogSeedData.GetCatalogoItems());
    }
}
