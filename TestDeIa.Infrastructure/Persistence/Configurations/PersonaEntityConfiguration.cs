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

        builder.Property(persona => persona.TipoIdentificacion)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(persona => persona.Identificacion)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(persona => persona.Nombres)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(persona => persona.Apellidos)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(persona => persona.Email)
            .HasMaxLength(180);

        builder.Property(persona => persona.Telefono)
            .HasMaxLength(40);

        builder.Property(persona => persona.Direccion)
            .HasMaxLength(250);

        builder.HasIndex(persona => persona.Identificacion)
            .IsUnique();

        builder.Property(persona => persona.IsSystemRecord)
            .HasDefaultValue(false);

        builder.HasData(new PersonaEntity
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TipoIdentificacion = "Sistema",
            Identificacion = "ADMIN",
            Nombres = "Administrador",
            Apellidos = "Sistema",
            Email = "admin@testdeia.local",
            IsActive = true,
            IsSystemRecord = true,
            CreatedAt = new DateTimeOffset(2026, 6, 18, 0, 0, 0, TimeSpan.Zero)
        });
    }
}
