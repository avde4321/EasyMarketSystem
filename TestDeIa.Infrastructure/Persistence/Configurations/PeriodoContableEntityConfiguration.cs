using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class PeriodoContableEntityConfiguration : IEntityTypeConfiguration<PeriodoContableEntity>
{
    public void Configure(EntityTypeBuilder<PeriodoContableEntity> builder)
    {
        builder.ToTable("PeriodosContables");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.Anio)
            .IsRequired();

        builder.Property(entity => entity.Mes)
            .IsRequired();

        builder.Property(entity => entity.EstaCerrado)
            .HasDefaultValue(false);

        builder.Property(entity => entity.FechaCierre);

        builder.Property(entity => entity.UsuarioCierreId);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Anio, entity.Mes })
            .IsUnique();
    }
}
