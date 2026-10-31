using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class NumeracionHistorialEntityConfiguration : IEntityTypeConfiguration<NumeracionHistorialEntity>
{
    public void Configure(EntityTypeBuilder<NumeracionHistorialEntity> b)
    {
        b.ToTable("NumeracionHistorial");
        b.HasKey(x => x.Id);
        b.Property(x => x.Serie).HasMaxLength(7).IsRequired();
        b.Property(x => x.AmbienteSri).HasMaxLength(1).IsRequired();
        b.Property(x => x.CodigoDocumento).HasMaxLength(2).IsRequired();
        b.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Fecha });
        b.HasOne<EmpresaEmisoraEntity>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
    }
}
