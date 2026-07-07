namespace TestDeIa.Domain.Modules.Personas.Entities;

public sealed class Persona
{
    public Persona(
        Guid id,
        string tipoIdentificacion,
        string identificacion,
        string razonSocialONombresCompletos,
        string? nombreComercial,
        string direccionPrincipal,
        string? telefonoCelular,
        string? correoElectronicoPrincipal,
        DateOnly? fechaNacimiento,
        string? genero,
        IReadOnlyCollection<string> rolesPersona,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        TipoIdentificacion = tipoIdentificacion;
        Identificacion = identificacion;
        RazonSocialONombresCompletos = razonSocialONombresCompletos;
        NombreComercial = nombreComercial;
        DireccionPrincipal = direccionPrincipal;
        TelefonoCelular = telefonoCelular;
        CorreoElectronicoPrincipal = correoElectronicoPrincipal;
        FechaNacimiento = fechaNacimiento;
        Genero = genero;
        RolesPersona = rolesPersona;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public string TipoIdentificacion { get; }

    public string Identificacion { get; }

    public string RazonSocialONombresCompletos { get; }

    public string? NombreComercial { get; }

    public string DireccionPrincipal { get; }

    public string? TelefonoCelular { get; }

    public string? CorreoElectronicoPrincipal { get; }

    public DateOnly? FechaNacimiento { get; }

    public string? Genero { get; }

    public string NombreCompleto => RazonSocialONombresCompletos;

    public IReadOnlyCollection<string> RolesPersona { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}
