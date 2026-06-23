using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class FacturaEntityConfiguration : IEntityTypeConfiguration<FacturaEntity>
{
    public void Configure(EntityTypeBuilder<FacturaEntity> builder)
    {
        builder.ToTable("Facturas");

        builder.HasKey(factura => factura.Id);

        builder.Property(factura => factura.Secuencial)
            .UseIdentityColumn();

        builder.Property(factura => factura.Establecimiento)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(factura => factura.PuntoEmision)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(factura => factura.RucEmisor)
            .HasMaxLength(13)
            .IsRequired();

        builder.Property(factura => factura.RazonSocialEmisor)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(factura => factura.NombreComercialEmisor)
            .HasMaxLength(300);

        builder.Property(factura => factura.DireccionMatrizEmisor)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(factura => factura.DireccionEstablecimientoEmisor)
            .HasMaxLength(300);

        builder.Property(factura => factura.AmbienteSri)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(factura => factura.TipoEmision)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(factura => factura.ContribuyenteEspecial)
            .HasMaxLength(40);

        builder.Property(factura => factura.RegimenRimpe)
            .HasMaxLength(60);

        builder.Property(factura => factura.AgenteRetencionResolucion)
            .HasMaxLength(60);

        builder.Property(factura => factura.ClienteIdentificacion)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(factura => factura.ClienteTipoIdentificacion)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(factura => factura.ClienteNombre)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(factura => factura.ClienteDireccion)
            .HasMaxLength(300);

        builder.Property(factura => factura.ClienteEmail)
            .HasMaxLength(180);

        builder.Property(factura => factura.ClienteTelefono)
            .HasMaxLength(40);

        builder.Property(factura => factura.FormaPago)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(factura => factura.FormaPagoSriCodigo)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(factura => factura.Estado)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(factura => factura.Observacion)
            .HasMaxLength(300);

        builder.Property(factura => factura.ClaveAcceso)
            .HasMaxLength(80);

        builder.Property(factura => factura.NumeroAutorizacion)
            .HasMaxLength(80);

        builder.Property(factura => factura.MensajeEstado)
            .HasMaxLength(400);

        builder.Property(factura => factura.XmlFirmado)
            .HasColumnType("nvarchar(max)");

        builder.Property(factura => factura.Subtotal).HasPrecision(18, 2);
        builder.Property(factura => factura.TotalDescuento).HasPrecision(18, 2);
        builder.Property(factura => factura.SubtotalIva0).HasPrecision(18, 2);
        builder.Property(factura => factura.SubtotalIva5).HasPrecision(18, 2);
        builder.Property(factura => factura.SubtotalIva8).HasPrecision(18, 2);
        builder.Property(factura => factura.SubtotalIva15).HasPrecision(18, 2);
        builder.Property(factura => factura.IvaTotal).HasPrecision(18, 2);
        builder.Property(factura => factura.Total).HasPrecision(18, 2);

        builder.Property(factura => factura.ProcessingNode)
            .HasMaxLength(100);

        builder.Property(factura => factura.RowVersion)
            .IsRowVersion();

        builder.HasIndex(factura => new { factura.Establecimiento, factura.PuntoEmision, factura.Secuencial })
            .IsUnique();

        builder.HasIndex(factura => new { factura.Estado, factura.CreatedAt });
    }
}
