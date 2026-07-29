using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ColaProcesamientoSriEntityConfiguration : IEntityTypeConfiguration<ColaProcesamientoSriEntity>
{
    public void Configure(EntityTypeBuilder<ColaProcesamientoSriEntity> builder)
    {
        builder.ToTable("ColaProcesamientoSRI");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.EmpresaId).IsRequired();
        builder.Property(entity => entity.TipoDocumentoId).HasMaxLength(2).IsRequired();
        builder.Property(entity => entity.Estado).HasMaxLength(30).IsRequired();
        builder.Property(entity => entity.Mensaje).HasMaxLength(400);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Estado, entity.NextRetryAt, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.ComprobanteId, entity.TipoDocumentoId });
    }
}
