using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class ProveedorEntityConfiguration : IEntityTypeConfiguration<ProveedorEntity>
{
    public void Configure(EntityTypeBuilder<ProveedorEntity> builder)
    {
        builder.ToTable("Proveedores");

        builder.HasKey(proveedor => proveedor.PersonaId);

        builder.Property(proveedor => proveedor.EmpresaId)
            .IsRequired();

        builder.HasIndex(proveedor => new { proveedor.EmpresaId, proveedor.PersonaId })
            .IsUnique();

        builder.Property(proveedor => proveedor.CodigoRetencionIvaDefault)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(proveedor => proveedor.CodigoRetencionRentaDefault)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(proveedor => proveedor.EstadoProveedor)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(proveedor => proveedor.Persona)
            .WithOne(persona => persona.Proveedor)
            .HasForeignKey<ProveedorEntity>(proveedor => proveedor.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
