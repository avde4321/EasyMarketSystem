using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ActivoFijoEntityConfiguration : IEntityTypeConfiguration<ActivoFijoEntity>
{
    public void Configure(EntityTypeBuilder<ActivoFijoEntity> builder)
    {
        builder.ToTable("ActivosFijos");

        builder.HasKey(activo => activo.Id);

        builder.HasIndex(activo => new { activo.EmpresaId, activo.CodigoActivo })
            .IsUnique();

        builder.HasIndex(activo => activo.CompraDetalleId)
            .IsUnique()
            .HasFilter("[CompraDetalleId] IS NOT NULL");

        builder.Property(activo => activo.CodigoActivo)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(activo => activo.Nombre)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(activo => activo.SerieMarca)
            .HasMaxLength(120);

        builder.Property(activo => activo.CategoriaSRI)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(activo => activo.CostoInicial)
            .HasPrecision(18, 2);

        builder.Property(activo => activo.ValorResidual)
            .HasPrecision(18, 2);

        builder.Property(activo => activo.PorcentajeDepreciacionAnual)
            .HasPrecision(8, 2);

        builder.Property(activo => activo.UbicacionFisica)
            .HasMaxLength(160);

        builder.Property(activo => activo.CustodioResponsable)
            .HasMaxLength(160);

        builder.Property(activo => activo.EstadoActivo)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(activo => activo.CompraDetalle)
            .WithOne()
            .HasForeignKey<ActivoFijoEntity>(activo => activo.CompraDetalleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
