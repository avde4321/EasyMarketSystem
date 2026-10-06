using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ExtractoBancarioDetalleEntityConfiguration : IEntityTypeConfiguration<ExtractoBancarioDetalleEntity>
{
    public void Configure(EntityTypeBuilder<ExtractoBancarioDetalleEntity> builder)
    {
        builder.ToTable("ExtractoBancarioDetalles");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.EmpresaId)
            .IsRequired();

        builder.Property(entity => entity.CuentaBancariaId)
            .IsRequired();

        builder.Property(entity => entity.NumeroDocumentoRef)
            .HasMaxLength(50);

        builder.Property(entity => entity.ConceptoDescripcion)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entity => entity.TipoMovimiento)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(entity => entity.Monto)
            .HasPrecision(18, 4);

        builder.Property(entity => entity.Conciliado)
            .HasDefaultValue(false);

        builder.HasIndex(entity => entity.ExtractoHeaderId);
        builder.HasIndex(entity => entity.FechaTransaccion);
        builder.HasIndex(entity => entity.NumeroDocumentoRef);
        builder.HasIndex(entity => new { entity.ExtractoHeaderId, entity.Conciliado });
        builder.HasIndex(entity => new { entity.FechaTransaccion, entity.NumeroDocumentoRef });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.CuentaBancariaId, entity.FechaTransaccion });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.CuentaBancariaId, entity.Conciliado, entity.FechaTransaccion });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.CuentaBancariaId, entity.NumeroDocumentoRef, entity.Monto });

        builder.HasOne(entity => entity.ExtractoHeader)
            .WithMany(entity => entity.Detalles)
            .HasForeignKey(entity => entity.ExtractoHeaderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.MovimientoTesoreria)
            .WithMany(entity => entity.ExtractosConciliados)
            .HasForeignKey(entity => entity.MovimientoTesoreriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
