using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EmpresaEmisoraEntityConfiguration : IEntityTypeConfiguration<EmpresaEmisoraEntity>
{
    public void Configure(EntityTypeBuilder<EmpresaEmisoraEntity> builder)
    {
        builder.ToTable("EmpresasEmisoras");

        builder.HasKey(empresa => empresa.Id);

        builder.Property(empresa => empresa.OwnerUserId)
            .IsRequired();

        builder.Property(empresa => empresa.RazonSocial)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(empresa => empresa.NombreComercial)
            .HasMaxLength(300);

        builder.Property(empresa => empresa.Ruc)
            .HasMaxLength(13)
            .IsRequired();

        builder.Property(empresa => empresa.DireccionMatriz)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(empresa => empresa.DireccionEstablecimiento)
            .HasMaxLength(300);

        builder.Property(empresa => empresa.Establecimiento)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(empresa => empresa.PuntoEmision)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(empresa => empresa.AmbienteSri)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(empresa => empresa.TipoEmision)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(empresa => empresa.ContribuyenteEspecial)
            .HasMaxLength(40);

        builder.Property(empresa => empresa.RegimenRimpe)
            .HasMaxLength(60);

        builder.Property(empresa => empresa.AgenteRetencionResolucion)
            .HasMaxLength(60);

        builder.Property(empresa => empresa.CertificadoNombreArchivo)
            .HasMaxLength(260);

        builder.Property(empresa => empresa.CertificadoContenido)
            .HasColumnType("varbinary(max)");

        builder.Property(empresa => empresa.CertificadoClave)
            .HasMaxLength(200);

        builder.HasIndex(empresa => empresa.Ruc)
            .IsUnique();

        builder.HasIndex(empresa => new { empresa.OwnerUserId, empresa.Ruc })
            .IsUnique();

        builder.HasData(new EmpresaEmisoraEntity
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            OwnerUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            RazonSocial = "EasyMarket Demo S.A.",
            NombreComercial = "EasyMarket Demo",
            Ruc = "0999999999001",
            DireccionMatriz = "Matriz demo",
            DireccionEstablecimiento = "Sucursal demo",
            Establecimiento = "001",
            PuntoEmision = "001",
            AmbienteSri = "Pruebas",
            ModoDesarrollo = true,
            TipoEmision = "Normal",
            ObligadoContabilidad = false,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero)
        });
    }
}
