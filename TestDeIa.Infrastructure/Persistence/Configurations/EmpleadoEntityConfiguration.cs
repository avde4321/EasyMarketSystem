using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Persistence.Configurations;

public sealed class EmpleadoEntityConfiguration : IEntityTypeConfiguration<EmpleadoEntity>
{
    public void Configure(EntityTypeBuilder<EmpleadoEntity> builder)
    {
        builder.ToTable("Empleados");

        builder.HasKey(empleado => empleado.PersonaId);

        builder.Property(empleado => empleado.EmpresaId)
            .IsRequired();

        builder.Property(empleado => empleado.CodigoEmpleado)
            .HasMaxLength(50);

        builder.Property(empleado => empleado.CodigoBiometrico)
            .HasMaxLength(50);

        builder.Property(empleado => empleado.TipoContrato)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(empleado => empleado.CargoPuesto)
            .HasMaxLength(120);

        builder.Property(empleado => empleado.SueldoBase)
            .HasPrecision(18, 2);

        builder.Property(empleado => empleado.PorcentajeComisionVentas)
            .HasPrecision(5, 2);

        builder.Property(empleado => empleado.EstadoLaboral)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(empleado => empleado.NombreContactoEmergencia)
            .HasMaxLength(150);

        builder.Property(empleado => empleado.TelefonoEmergencia)
            .HasMaxLength(40);

        builder.HasIndex(empleado => new { empleado.EmpresaId, empleado.PersonaId })
            .IsUnique();

        builder.HasOne(empleado => empleado.Persona)
            .WithOne(persona => persona.Empleado)
            .HasForeignKey<EmpleadoEntity>(empleado => empleado.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
