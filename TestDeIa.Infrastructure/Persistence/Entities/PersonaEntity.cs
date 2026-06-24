namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class PersonaEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }

    public string TipoIdentificacion { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string? EstadoCivil { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystemRecord { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ClienteEntity? Cliente { get; set; }

    public EmpleadoEntity? Empleado { get; set; }

    public SecurityUserEntity? SecurityUser { get; set; }
}
