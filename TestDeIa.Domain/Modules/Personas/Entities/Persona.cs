namespace TestDeIa.Domain.Modules.Personas.Entities;

public sealed class Persona
{
    public Persona(
        Guid id,
        string tipoIdentificacion,
        string identificacion,
        string nombres,
        string apellidos,
        DateOnly? fechaNacimiento,
        string? email,
        string? telefono,
        string? direccion,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        TipoIdentificacion = tipoIdentificacion;
        Identificacion = identificacion;
        Nombres = nombres;
        Apellidos = apellidos;
        FechaNacimiento = fechaNacimiento;
        Email = email;
        Telefono = telefono;
        Direccion = direccion;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public string TipoIdentificacion { get; }

    public string Identificacion { get; }

    public string Nombres { get; }

    public string Apellidos { get; }

    public DateOnly? FechaNacimiento { get; }

    public string? Email { get; }

    public string? Telefono { get; }

    public string? Direccion { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}
