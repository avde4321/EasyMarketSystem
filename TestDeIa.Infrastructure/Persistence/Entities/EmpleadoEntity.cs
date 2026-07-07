namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class EmpleadoEntity
{
    public Guid PersonaId { get; set; }

    public Guid EmpresaId { get; set; }

    public PersonaEntity Persona { get; set; } = default!;

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

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid UsuarioCreacionId { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UsuarioModificacionId { get; set; }
}
