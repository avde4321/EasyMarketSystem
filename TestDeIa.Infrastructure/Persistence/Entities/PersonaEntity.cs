namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class PersonaEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }

    public string TipoIdentificacion { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string RazonSocialONombresCompletos { get; set; } = string.Empty;

    public string? NombreComercial { get; set; }

    public string DireccionPrincipal { get; set; } = string.Empty;

    public DateOnly? FechaNacimiento { get; set; }

    public string? CorreoElectronicoPrincipal { get; set; }

    public string? TelefonoCelular { get; set; }

    public string? Genero { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystemRecord { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ClienteEntity? Cliente { get; set; }

    public EmpleadoEntity? Empleado { get; set; }

    public ProveedorEntity? Proveedor { get; set; }

    public SecurityUserEntity? SecurityUser { get; set; }
}
