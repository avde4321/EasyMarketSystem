using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CertificadoDigitalEmpresaEntityConfiguration : IEntityTypeConfiguration<CertificadoDigitalEmpresaEntity>
{
    public void Configure(EntityTypeBuilder<CertificadoDigitalEmpresaEntity> builder)
    {
        builder.ToTable("CertificadosDigitalesEmpresa");

        builder.HasKey(certificado => certificado.Id);

        builder.Property(certificado => certificado.Nombre)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(certificado => certificado.NombreArchivo)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(certificado => certificado.Contenido)
            .HasColumnType("varbinary(max)")
            .IsRequired();

        builder.Property(certificado => certificado.Clave)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(certificado => certificado.Sujeto)
            .HasMaxLength(500);

        builder.Property(certificado => certificado.Emisor)
            .HasMaxLength(500);

        builder.Property(certificado => certificado.NumeroSerie)
            .HasMaxLength(120);

        builder.Property(certificado => certificado.HuellaDigital)
            .HasMaxLength(120);

        builder.HasOne(certificado => certificado.Empresa)
            .WithMany(empresa => empresa.CertificadosDigitales)
            .HasForeignKey(certificado => certificado.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(certificado => new { certificado.EmpresaId, certificado.EsPrincipal });
        builder.HasIndex(certificado => new { certificado.EmpresaId, certificado.FechaFinVigencia });
        builder.HasIndex(certificado => new { certificado.IsActive, certificado.FechaFinVigencia });
    }
}
