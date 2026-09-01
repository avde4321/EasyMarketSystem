using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ColaProcesamientoSriEntityConfiguration : IEntityTypeConfiguration<ColaProcesamientoSriEntity>
{
    public void Configure(EntityTypeBuilder<ColaProcesamientoSriEntity> builder)
    {
        builder.ToTable("ColaProcesamientoSRI");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.EmpresaId).IsRequired();
        builder.Property(entity => entity.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(entity => entity.Intentos).HasDefaultValue(0);
        builder.Property(entity => entity.TipoDocumentoId).HasMaxLength(10).IsRequired();
        builder.Property(entity => entity.Estado)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();
        builder.Property(entity => entity.Mensaje).HasMaxLength(400);
        builder.Property(entity => entity.UltimoError).HasMaxLength(2000);
        builder.Property(entity => entity.ProcessingNode).HasMaxLength(120);
        builder.Property(entity => entity.RowVersion).IsRowVersion();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Estado, entity.NextRetryAt, entity.CreatedAt });
        builder.HasIndex(entity => new { entity.ComprobanteId, entity.TipoDocumentoId });
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_ColaProcesamientoSRI_Estado",
            $"[Estado] IN ('{SriOutboxEstados.Pendiente}','{SriOutboxEstados.EnProceso}','{SriOutboxEstados.Autorizado}','{SriOutboxEstados.Error}','{SriOutboxEstados.Devuelto}')"));
    }
}
