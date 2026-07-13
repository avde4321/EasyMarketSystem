namespace TestDeIa.Shared.Responses.Compras;

public sealed class ProveedorResponse
{
    public Guid Id { get; set; }
    public Guid PersonaId { get; set; }
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string RazonSocialONombresCompletos { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string NombreCompleto => RazonSocialONombresCompletos;
    public string DireccionPrincipal { get; set; } = string.Empty;
    public string? CorreoElectronicoPrincipal { get; set; }
    public string? TelefonoCelular { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Genero { get; set; }
    public string CodigoRetencionIvaDefault { get; set; } = string.Empty;
    public string CodigoRetencionRentaDefault { get; set; } = string.Empty;
    public bool PermiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public string EstadoProveedor { get; set; } = string.Empty;
    public IReadOnlyCollection<string> RolesPersona { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
}
