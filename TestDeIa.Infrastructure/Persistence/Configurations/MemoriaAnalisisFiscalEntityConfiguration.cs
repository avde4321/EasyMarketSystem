using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class MemoriaAnalisisFiscalEntityConfiguration : IEntityTypeConfiguration<MemoriaAnalisisFiscalEntity>
{
    public void Configure(EntityTypeBuilder<MemoriaAnalisisFiscalEntity> builder)
    {
        builder.ToTable("MemoriasAnalisisFiscal");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.ResumenNumericoJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(entity => entity.RazonamientoIA)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(entity => entity.ContextoPrevioUtilizado)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Mes, entity.Anio })
            .IsUnique();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Anio, entity.Mes, entity.CreatedAt });
    }
}
