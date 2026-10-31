namespace TestDeIa.Shared.Responses.Clientes;

public sealed class ClienteResponse
{
    public Guid Id { get; set; }

    public Guid PersonaId { get; set; }

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

    public string? CorreoElectronicoPrincipal { get; set; }

    public string? TelefonoCelular { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? Genero { get; set; }

    public string? CorreoFacturacionElectronica { get; set; }

    public string TipoCliente { get; set; } = string.Empty;

    public bool ObligadoContabilidad { get; set; }

    public bool EsContribuyenteEspecial { get; set; }

    public bool PermiteCredito { get; set; }

    public decimal LimiteCredito { get; set; }

    public int DiasCreditoMaximo { get; set; }

    public string EstadoCredito { get; set; } = string.Empty;

    public IReadOnlyCollection<string> RolesPersona { get; set; } = [];

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid UsuarioCreacionId { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UsuarioModificacionId { get; set; }
}
