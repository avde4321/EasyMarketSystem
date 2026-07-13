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
            .HasPrecision(18, 2);

        builder.Property(entity => entity.TotalVentasEfectivoCalculado)
            .HasPrecision(18, 2);

        builder.Property(entity => entity.TotalVentasTarjetaCalculado)
            .HasPrecision(18, 2);

        builder.Property(entity => entity.MontoFisicoEfectivoReal)
            .HasPrecision(18, 2);

        builder.Property(entity => entity.MontoFisicoTarjetaReal)
            .HasPrecision(18, 2);

        builder.Property(entity => entity.DiferenciaEfectivo)
            .HasPrecision(18, 2);

        builder.Property(entity => entity.DiferenciaTarjeta)
            .HasPrecision(18, 2);

        builder.Property(entity => entity.EstadoCaja)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.UsuarioId, entity.EstadoCaja });
    }
}
