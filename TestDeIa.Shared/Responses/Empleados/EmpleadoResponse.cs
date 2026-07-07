namespace TestDeIa.Shared.Responses.Empleados;

public sealed class EmpleadoResponse
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

    public string? CodigoEmpleado { get; set; }

    public string? CodigoBiometrico { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public DateOnly? FechaSalida { get; set; }

    public string TipoContrato { get; set; } = string.Empty;

    public string? CargoPuesto { get; set; }

    public decimal SueldoBase { get; set; }

    public decimal PorcentajeComisionVentas { get; set; }

    public string EstadoLaboral { get; set; } = string.Empty;

    public string? NombreContactoEmergencia { get; set; }

    public string? TelefonoEmergencia { get; set; }

    public IReadOnlyCollection<string> RolesPersona { get; set; } = [];

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid UsuarioCreacionId { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UsuarioModificacionId { get; set; }
}
