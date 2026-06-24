using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EmpleadoEntityConfiguration : IEntityTypeConfiguration<EmpleadoEntity>
{
    public void Configure(EntityTypeBuilder<EmpleadoEntity> builder)
    {
        builder.ToTable("Empleados");

        builder.HasKey(empleado => empleado.Id);

        builder.Property(empleado => empleado.EmpresaId)
            .IsRequired();

        builder.HasIndex(empleado => new { empleado.EmpresaId, empleado.PersonaId })
            .IsUnique();

        builder.HasOne(empleado => empleado.Persona)
            .WithOne(persona => persona.Empleado)
            .HasForeignKey<EmpleadoEntity>(empleado => empleado.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
