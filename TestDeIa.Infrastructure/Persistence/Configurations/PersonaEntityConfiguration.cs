using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class PersonaEntityConfiguration : IEntityTypeConfiguration<PersonaEntity>
{
    public void Configure(EntityTypeBuilder<PersonaEntity> builder)
    {
        builder.ToTable("Personas");

        builder.HasKey(persona => persona.Id);

        builder.Property(persona => persona.EmpresaId)
            .IsRequired();

        builder.Property(persona => persona.TipoIdentificacion)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(persona => persona.Identificacion)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(persona => persona.RazonSocialONombresCompletos)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(persona => persona.NombreComercial)
            .HasMaxLength(150);

        builder.Property(persona => persona.DireccionPrincipal)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(persona => persona.CorreoElectronicoPrincipal)
            .HasMaxLength(180);

        builder.Property(persona => persona.TelefonoCelular)
            .HasMaxLength(40);

        builder.Property(persona => persona.Genero)
            .HasMaxLength(30);

        builder.Property(persona => persona.EsPersonaJuridica)
            .HasDefaultValue(false);

        builder.Property(persona => persona.EsEmpresa)
            .HasDefaultValue(false);

        builder.Property(persona => persona.RegionCodigo)
            .HasMaxLength(80);

        builder.Property(persona => persona.ProvinciaCodigo)
            .HasMaxLength(80);

        builder.Property(persona => persona.CiudadCodigo)
            .HasMaxLength(80);

        builder.Property(persona => persona.SectorCodigo)
            .HasMaxLength(80);

        builder.HasIndex(persona => new { persona.EmpresaId, persona.Identificacion })
            .IsUnique();

        builder.Property(persona => persona.IsSystemRecord)
            .HasDefaultValue(false);

        builder.HasData(new PersonaEntity
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            EmpresaId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            TipoIdentificacion = "05",
            Identificacion = "ADMIN",
            RazonSocialONombresCompletos = "Administrador Sistema",
            NombreComercial = null,
            DireccionPrincipal = "Sistema",
            CorreoElectronicoPrincipal = "admin@testdeia.local",
            EsPersonaJuridica = false,
            EsEmpresa = false,
            RegionCodigo = "NORTE",
            ProvinciaCodigo = "PICHINCHA",
            CiudadCodigo = "QUITO",
            SectorCodigo = "QUITO_NORTE",
            IsActive = true,
            IsSystemRecord = true,
            CreatedAt = new DateTimeOffset(2026, 6, 18, 0, 0, 0, TimeSpan.Zero)
        });
    }
}
