using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EmpresaPuntoEmisionEntityConfiguration : IEntityTypeConfiguration<EmpresaPuntoEmisionEntity>
{
    public void Configure(EntityTypeBuilder<EmpresaPuntoEmisionEntity> builder)
    {
        builder.ToTable("EmpresaPuntosEmision");

        builder.HasKey(punto => punto.Id);

        builder.Property(punto => punto.Establecimiento)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(punto => punto.PuntoEmision)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(punto => punto.DireccionEstablecimiento)
            .HasMaxLength(300);

        builder.Property(punto => punto.BodegaId)
            .IsRequired();

        builder.HasIndex(punto => new { punto.EmpresaEmisoraId, punto.Establecimiento, punto.PuntoEmision })
            .IsUnique();

        builder.HasOne(punto => punto.Empresa)
            .WithMany(empresa => empresa.PuntosEmision)
            .HasForeignKey(punto => punto.EmpresaEmisoraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(punto => punto.Bodega)
            .WithMany()
            .HasForeignKey(punto => punto.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
