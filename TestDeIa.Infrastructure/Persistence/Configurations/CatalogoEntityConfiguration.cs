using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CatalogoEntityConfiguration : IEntityTypeConfiguration<CatalogoEntity>
{
    public void Configure(EntityTypeBuilder<CatalogoEntity> builder)
    {
        builder.ToTable("Catalogos");

        builder.HasKey(catalogo => catalogo.Id);

        builder.Property(catalogo => catalogo.Codigo)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(catalogo => catalogo.Nombre)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(catalogo => catalogo.Descripcion)
            .HasMaxLength(250);

        builder.HasIndex(catalogo => catalogo.Codigo)
            .IsUnique();

        builder.HasData(CatalogSeedData.GetCatalogos());
    }
}
