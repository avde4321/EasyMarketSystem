using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EmpresaClienteEntityConfiguration : IEntityTypeConfiguration<EmpresaClienteEntity>
{
    public void Configure(EntityTypeBuilder<EmpresaClienteEntity> builder)
    {
        builder.ToTable("EmpresasCliente");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.RazonSocial).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.NombreComercial).HasMaxLength(150);
        builder.Property(entity => entity.Ruc).HasMaxLength(13).IsRequired();
        builder.Property(entity => entity.RepresentanteLegal).HasMaxLength(180);
        builder.Property(entity => entity.ContribuyenteEspecial).HasMaxLength(30);
        builder.Property(entity => entity.EmailFacturacion).HasMaxLength(180);
        builder.Property(entity => entity.Telefono).HasMaxLength(40);
        builder.Property(entity => entity.DireccionMatriz).HasMaxLength(250).IsRequired();

        builder.HasIndex(entity => new { entity.EmpresaId, entity.Ruc }).IsUnique();
    }
}
