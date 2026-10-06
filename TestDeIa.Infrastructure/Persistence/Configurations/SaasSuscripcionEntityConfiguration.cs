using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SaasSuscripcionEntityConfiguration : IEntityTypeConfiguration<SaasSuscripcionEntity>
{
    public void Configure(EntityTypeBuilder<SaasSuscripcionEntity> builder)
    {
        builder.ToTable("SaasSuscripciones");

        builder.HasKey(suscripcion => suscripcion.Id);

        builder.Property(suscripcion => suscripcion.EstadoSuscripcion)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(suscripcion => suscripcion.FacturasEmitidasMesActual)
            .HasDefaultValue(0);

        builder.HasIndex(suscripcion => new { suscripcion.EmpresaId, suscripcion.EstadoSuscripcion });

        builder.HasOne(suscripcion => suscripcion.Plan)
            .WithMany(plan => plan.Suscripciones)
            .HasForeignKey(suscripcion => suscripcion.SaasPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(suscripcion => suscripcion.Empresa)
            .WithMany()
            .HasForeignKey(suscripcion => suscripcion.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new SaasSuscripcionEntity
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            EmpresaId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            SaasPlanId = 3,
            FechaInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            FechaVencimiento = new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            EstadoSuscripcion = EstadoSuscripcionSaas.Activa,
            FacturasEmitidasMesActual = 0,
            ContadorFacturasPeriodo = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
