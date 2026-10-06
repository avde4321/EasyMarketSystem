using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class SaasPlanEntityConfiguration : IEntityTypeConfiguration<SaasPlanEntity>
{
    public void Configure(EntityTypeBuilder<SaasPlanEntity> builder)
    {
        builder.ToTable("SaasPlanes");

        builder.HasKey(plan => plan.Id);

        builder.Property(plan => plan.Id)
            .ValueGeneratedNever();

        builder.Property(plan => plan.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(plan => plan.PrecioMensual)
            .HasPrecision(18, 2);

        builder.Property(plan => plan.EsActivo)
            .HasDefaultValue(true);

        builder.HasIndex(plan => plan.Nombre)
            .IsUnique();

        builder.HasData(
            new SaasPlanEntity
            {
                Id = 1,
                Nombre = "Básico",
                LimiteFacturasMensuales = 100,
                LimiteUsuarios = 3,
                LimiteSucursales = 1,
                PermiteModuloSRI = true,
                PermiteModuloKardexAvanzado = false,
                PrecioMensual = 29.99m,
                EsActivo = true
            },
            new SaasPlanEntity
            {
                Id = 2,
                Nombre = "Pro",
                LimiteFacturasMensuales = 1000,
                LimiteUsuarios = 15,
                LimiteSucursales = 5,
                PermiteModuloSRI = true,
                PermiteModuloKardexAvanzado = true,
                PrecioMensual = 79.99m,
                EsActivo = true
            },
            new SaasPlanEntity
            {
                Id = 3,
                Nombre = "Enterprise",
                LimiteFacturasMensuales = 0,
                LimiteUsuarios = 0,
                LimiteSucursales = 0,
                PermiteModuloSRI = true,
                PermiteModuloKardexAvanzado = true,
                PrecioMensual = 0m,
                EsActivo = true
            });
    }
}
