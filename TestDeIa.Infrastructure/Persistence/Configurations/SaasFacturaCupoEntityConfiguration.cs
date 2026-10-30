using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SaasFacturaCupoEntityConfiguration : IEntityTypeConfiguration<SaasFacturaCupoEntity>
{
    public void Configure(EntityTypeBuilder<SaasFacturaCupoEntity> b)
    {
        b.ToTable("SaasFacturaCupos", t => t.HasCheckConstraint("CK_SaasFacturaCupos_Estado", "[Estado] IN ('RESERVADO','CONFIRMADO','LIBERADO')"));
        // La reserva precede al INSERT de factura, pero ambos pertenecen a la misma transacción.
        b.HasKey(x => x.FacturaId);
        b.Property(x => x.Estado).HasMaxLength(12).IsUnicode(false).IsRequired();
        b.Property(x => x.PeriodoAutorizacion).HasColumnType("date");
        b.HasIndex(x => new { x.EmpresaId, x.Estado, x.PeriodoAutorizacion });
        b.HasOne<EmpresaEmisoraEntity>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
    }
}
