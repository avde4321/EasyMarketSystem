using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SaasPagoRegistroEntityConfiguration : IEntityTypeConfiguration<SaasPagoRegistroEntity>
{
    public void Configure(EntityTypeBuilder<SaasPagoRegistroEntity> b)
    {
        b.ToTable("SaasPagoRegistros", t =>
        {
            t.HasCheckConstraint("CK_SaasPagoRegistros_Monto", "[Monto] > 0");
            t.HasCheckConstraint("CK_SaasPagoRegistros_Estado", "[EstadoPago] IN ('Pendiente','Aprobado','Rechazado')");
            t.HasCheckConstraint("CK_SaasPagoRegistros_Metodo", "[MetodoPago] IN ('TarjetaPasarela','TransferenciaBancaria')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Monto).HasPrecision(18, 4);
        b.Property(x => x.ComprobanteReferencia).HasMaxLength(200).IsRequired();
        b.Property(x => x.Observaciones).HasMaxLength(2000);
        b.Property(x => x.ObservacionesConfirmacion).HasMaxLength(2000);
        b.Property(x => x.MetodoPago).HasConversion<string>().HasMaxLength(30).IsUnicode(false);
        b.Property(x => x.EstadoPago).HasConversion<string>().HasMaxLength(15).IsUnicode(false);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.EmpresaId, x.MetodoPago, x.ComprobanteReferencia }).IsUnique();
        b.HasIndex(x => new { x.EmpresaId, x.EstadoPago, x.FechaPago });
        b.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.SaasPlanId).OnDelete(DeleteBehavior.Restrict);
    }
}
