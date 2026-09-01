using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class DocumentoAdjuntoEntityConfiguration : IEntityTypeConfiguration<DocumentoAdjuntoEntity>
{
    public void Configure(EntityTypeBuilder<DocumentoAdjuntoEntity> builder)
    {
        builder.ToTable("DocumentosAdjuntos");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .ValueGeneratedNever();

        builder.Property(entity => entity.Modulo)
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.EntidadTipo)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.TipoAdjunto)
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.NombreArchivo)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(entity => entity.ContentType)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.RutaStorage)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entity => entity.HashSHA256)
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.Origen)
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(entity => entity.EsActivo)
            .HasDefaultValue(true);

        builder.Property(entity => entity.Version)
            .HasDefaultValue(1);

        builder.Property(entity => entity.FechaCreacion)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.EntidadTipo, entity.EntidadId });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Modulo, entity.TipoAdjunto, entity.FechaCreacion });
    }
}
