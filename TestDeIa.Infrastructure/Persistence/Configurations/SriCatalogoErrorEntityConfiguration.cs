using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SriCatalogoErrorEntityConfiguration : IEntityTypeConfiguration<SriCatalogoErrorEntity>
{
    public void Configure(EntityTypeBuilder<SriCatalogoErrorEntity> builder)
    {
        builder.ToTable("SriCatalogoErrores");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.CodigoSri).HasMaxLength(10).IsUnicode(false).IsRequired();
        builder.Property(entity => entity.MensajeSri).HasMaxLength(300).IsRequired();
        builder.Property(entity => entity.SolucionSugerida).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.TipoError).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();

        builder.HasIndex(entity => entity.CodigoSri).IsUnique();

        builder.HasData(
            new SriCatalogoErrorEntity { Id = 1, CodigoSri = "35", MensajeSri = "CLAVE ACCESO REGISTRADA", SolucionSugerida = "Consultar autorización con la misma clave de acceso antes de reemitir. Si ya está autorizado, recuperar XML autorizado.", TipoError = SriTipoError.Advertencia },
            new SriCatalogoErrorEntity { Id = 2, CodigoSri = "43", MensajeSri = "CLAVE DE ACCESO INVALIDA", SolucionSugerida = "Validar fecha, RUC, ambiente, establecimiento, punto de emisión, secuencial, tipo de emisión y dígito verificador módulo 11.", TipoError = SriTipoError.ErrorRecepcion },
            new SriCatalogoErrorEntity { Id = 3, CodigoSri = "45", MensajeSri = "SECUENCIAL REGISTRADO", SolucionSugerida = "Verificar que el secuencial no haya sido usado previamente para el mismo establecimiento y punto de emisión.", TipoError = SriTipoError.ErrorRecepcion },
            new SriCatalogoErrorEntity { Id = 4, CodigoSri = "70", MensajeSri = "FIRMA INVALIDA", SolucionSugerida = "Revisar vigencia del certificado, contraseña, algoritmo de firma XAdES-BES y que el XML no se haya alterado después de firmar.", TipoError = SriTipoError.ErrorFirma });
    }
}
