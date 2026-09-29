using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class CajaSesionEntityConfiguration : IEntityTypeConfiguration<CajaSesionEntity>
{
    public void Configure(EntityTypeBuilder<CajaSesionEntity> builder)
    {
        builder.ToTable("CajaSesiones");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.UsuarioId)
            .IsRequired();

        builder.Property(entity => entity.MontoApertura)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.TotalVentasEfectivoCalculado)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.TotalVentasTarjetaCalculado)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.TotalVentasTransferenciaCalculado)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.MontoFisicoEfectivoReal)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.MontoFisicoTarjetaReal)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.MontoFisicoTransferenciaReal)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.DiferenciaEfectivo)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.DiferenciaTarjeta)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.DiferenciaTransferencia)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.Diferencia)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.MontoDeclaradoEfectivo).HasPrecision(18, 4);
        builder.Property(entity => entity.MontoDeclaradoTarjetas).HasPrecision(18, 4);
        builder.Property(entity => entity.MontoDeclaradoTransferencias).HasPrecision(18, 4);
        builder.Property(entity => entity.MontoDeclaradoOtros).HasPrecision(18, 4);
        builder.Property(entity => entity.MontoDeclaradoTotal).HasPrecision(18, 4);
        builder.Property(entity => entity.MontoCalculadoEfectivo).HasPrecision(18, 4).HasDefaultValue(0m);
        builder.Property(entity => entity.MontoCalculadoTarjetas).HasPrecision(18, 4).HasDefaultValue(0m);
        builder.Property(entity => entity.MontoCalculadoTransferencias).HasPrecision(18, 4).HasDefaultValue(0m);
        builder.Property(entity => entity.MontoCalculadoOtros).HasPrecision(18, 4).HasDefaultValue(0m);
        builder.Property(entity => entity.MontoCalculadoTotal).HasPrecision(18, 4).HasDefaultValue(0m);
        builder.Property(entity => entity.DiferenciaMonto).HasPrecision(18, 4).HasDefaultValue(0m);

        builder.Property(entity => entity.ObservacionesCierre)
            .HasMaxLength(500);

        builder.Property(entity => entity.RowVersion)
            .IsRowVersion();

        builder.Property(entity => entity.EstadoCaja)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.UsuarioId, entity.EstadoCaja });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PuntoEmisionId, entity.UsuarioId, entity.EstadoCaja });

        builder.HasOne<EmpresaPuntoEmisionEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.PuntoEmisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BodegaEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AsientoContableEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.AsientoContableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
