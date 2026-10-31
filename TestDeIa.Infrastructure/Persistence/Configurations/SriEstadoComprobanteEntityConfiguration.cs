using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SriEstadoComprobanteEntityConfiguration : IEntityTypeConfiguration<SriEstadoComprobanteEntity>
{
    public void Configure(EntityTypeBuilder<SriEstadoComprobanteEntity> builder)
    {
        builder.ToTable("SriEstadosComprobante");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.Codigo).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(entity => entity.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Descripcion).HasMaxLength(300).IsRequired();

        builder.HasIndex(entity => entity.Codigo).IsUnique();

        builder.HasData(
            new SriEstadoComprobanteEntity { Id = 1, Codigo = SriEstadosComprobante.Generado, Nombre = "Generado", Descripcion = "XML generado localmente y listo para firma.", RequiereReenvioRecepcion = false, RequiereConsultaAutorizacion = false, EsEstadoFinal = false, EsEditable = true },
            new SriEstadoComprobanteEntity { Id = 2, Codigo = SriEstadosComprobante.Firmado, Nombre = "Firmado", Descripcion = "XML firmado electrónicamente y pendiente de recepción SRI.", RequiereReenvioRecepcion = true, RequiereConsultaAutorizacion = false, EsEstadoFinal = false, EsEditable = false },
            new SriEstadoComprobanteEntity { Id = 3, Codigo = SriEstadosComprobante.Devuelta, Nombre = "Devuelta", Descripcion = "Recepción SRI devolvió el comprobante por errores de estructura, clave o firma.", RequiereReenvioRecepcion = true, RequiereConsultaAutorizacion = false, EsEstadoFinal = false, EsEditable = true },
            new SriEstadoComprobanteEntity { Id = 4, Codigo = SriEstadosComprobante.EnProceso, Nombre = "En proceso", Descripcion = "Comprobante recibido por el SRI y pendiente de autorización.", RequiereReenvioRecepcion = false, RequiereConsultaAutorizacion = true, EsEstadoFinal = false, EsEditable = false },
            new SriEstadoComprobanteEntity { Id = 5, Codigo = SriEstadosComprobante.Autorizado, Nombre = "Autorizado", Descripcion = "Comprobante autorizado por el SRI.", RequiereReenvioRecepcion = false, RequiereConsultaAutorizacion = false, EsEstadoFinal = true, EsEditable = false },
            new SriEstadoComprobanteEntity { Id = 6, Codigo = SriEstadosComprobante.NoAutorizado, Nombre = "No autorizado", Descripcion = "Autorización SRI rechazó el comprobante.", RequiereReenvioRecepcion = false, RequiereConsultaAutorizacion = true, EsEstadoFinal = false, EsEditable = true },
            new SriEstadoComprobanteEntity { Id = 7, Codigo = SriEstadosComprobante.Anulado, Nombre = "Anulado", Descripcion = "Comprobante anulado o marcado como sin validez operativa.", RequiereReenvioRecepcion = false, RequiereConsultaAutorizacion = false, EsEstadoFinal = true, EsEditable = false });
    }
}
