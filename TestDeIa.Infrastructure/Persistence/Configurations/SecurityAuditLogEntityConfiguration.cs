using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SecurityAuditLogEntityConfiguration : IEntityTypeConfiguration<SecurityAuditLogEntity>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLogEntity> builder)
    {
        builder.ToTable("SecurityAuditLogs");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.UsuarioId)
            .IsRequired();

        builder.Property(entity => entity.TipoEvento)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(entity => entity.DireccionIP)
            .HasMaxLength(80);

        builder.Property(entity => entity.Detalles)
            .HasMaxLength(500);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.FechaEvento });
        builder.HasIndex(entity => new { entity.UsuarioId, entity.FechaEvento });
    }
}
