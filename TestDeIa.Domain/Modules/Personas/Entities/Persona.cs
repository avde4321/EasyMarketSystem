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
        bool esPersonaJuridica,
        bool esEmpresa,
        IReadOnlyCollection<string> rolesPersona,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        string? regionCodigo = null,
        string? provinciaCodigo = null,
        string? ciudadCodigo = null,
        string? sectorCodigo = null)
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
        EsPersonaJuridica = esPersonaJuridica;
        EsEmpresa = esEmpresa;
        RegionCodigo = regionCodigo;
        ProvinciaCodigo = provinciaCodigo;
        CiudadCodigo = ciudadCodigo;
        SectorCodigo = sectorCodigo;
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

    public bool EsPersonaJuridica { get; }

    public bool EsEmpresa { get; }

    public string? RegionCodigo { get; }

    public string? ProvinciaCodigo { get; }

    public string? CiudadCodigo { get; }

    public string? SectorCodigo { get; }

    public string NombreCompleto => RazonSocialONombresCompletos;

    public IReadOnlyCollection<string> RolesPersona { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}
