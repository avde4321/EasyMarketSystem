using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ComprobanteCabeceraEntityConfiguration : IEntityTypeConfiguration<ComprobanteCabeceraEntity>
{
    public void Configure(EntityTypeBuilder<ComprobanteCabeceraEntity> builder)
    {
        builder.ToTable("ComprobanteCabecera");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId).IsRequired();
        builder.Property(entity => entity.TipoDocumentoId).HasMaxLength(2).IsRequired();
        builder.Property(entity => entity.Establecimiento).HasMaxLength(3).IsRequired();
        builder.Property(entity => entity.PuntoEmision).HasMaxLength(3).IsRequired();
        builder.Property(entity => entity.MotivoModificacion).HasMaxLength(300);
        builder.Property(entity => entity.CodDocModificado).HasMaxLength(2);
        builder.Property(entity => entity.NumDocModificado).HasMaxLength(17);
        builder.Property(entity => entity.RucEmisor).HasMaxLength(13).IsRequired();
        builder.Property(entity => entity.RazonSocialEmisor).HasMaxLength(300).IsRequired();
        builder.Property(entity => entity.NombreComercialEmisor).HasMaxLength(300);
        builder.Property(entity => entity.DireccionMatrizEmisor).HasMaxLength(300).IsRequired();
        builder.Property(entity => entity.DireccionEstablecimientoEmisor).HasMaxLength(300);
        builder.Property(entity => entity.AmbienteSri).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.TipoEmision).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.ClienteTipoIdentificacion).HasMaxLength(2).IsRequired();
        builder.Property(entity => entity.ClienteIdentificacion).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.ClienteNombre).HasMaxLength(300).IsRequired();
        builder.Property(entity => entity.ClienteDireccion).HasMaxLength(300);
        builder.Property(entity => entity.ClaveAcceso).HasMaxLength(49).IsRequired();
        builder.Property(entity => entity.NumeroAutorizacion).HasMaxLength(80);
        builder.Property(entity => entity.XmlGenerado).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.XmlFirmado).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.Estado).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.Property(entity => entity.Subtotal).HasPrecision(18, 2);
        builder.Property(entity => entity.TotalDescuento).HasPrecision(18, 2);
        builder.Property(entity => entity.IvaTotal).HasPrecision(18, 2);
        builder.Property(entity => entity.Total).HasPrecision(18, 2);

        builder.HasOne(entity => entity.ComprobanteModificado)
            .WithMany()
            .HasForeignKey(entity => entity.ComprobanteModificadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.EmpresaId, entity.TipoDocumentoId, entity.Establecimiento, entity.PuntoEmision, entity.Secuencial })
            .IsUnique();
        builder.HasIndex(entity => entity.ClaveAcceso).IsUnique();
        builder.HasIndex(entity => new { entity.Estado, entity.CreatedAt });
    }
}
