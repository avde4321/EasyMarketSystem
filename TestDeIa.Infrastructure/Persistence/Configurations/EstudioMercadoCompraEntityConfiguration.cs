using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EstudioMercadoCompraEntityConfiguration : IEntityTypeConfiguration<EstudioMercadoCompraEntity>
{
    public void Configure(EntityTypeBuilder<EstudioMercadoCompraEntity> builder)
    {
        builder.ToTable("EstudiosMercadoCompra");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.TopProductosVendidosJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(entity => entity.SugerenciasCompraJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(entity => entity.AnalisisEstrategicoIA)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Anio, entity.Mes })
            .IsUnique();
    }
}
