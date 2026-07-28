namespace TestDeIa.Shared.Responses.Personas;

public sealed class PersonaResponse
{
    public Guid Id { get; set; }

    public string TipoIdentificacion { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string RazonSocialONombresCompletos { get; set; } = string.Empty;

    public string? NombreComercial { get; set; }

    public string NombreCompleto => RazonSocialONombresCompletos;

    public string DireccionPrincipal { get; set; } = string.Empty;

    public string? RegionCodigo { get; set; }

    public string? ProvinciaCodigo { get; set; }

    public string? CiudadCodigo { get; set; }

    public string? SectorCodigo { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? CorreoElectronicoPrincipal { get; set; }

    public string? TelefonoCelular { get; set; }

    public string? Genero { get; set; }

    public bool EsPersonaJuridica { get; set; }

    public bool EsEmpresa { get; set; }

    public IReadOnlyCollection<string> RolesPersona { get; set; } = [];

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
