using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Empleados;

public sealed class EmpleadoRequest
{
    [Required(ErrorMessage = "El tipo de identificacion es obligatorio.")]
    [StringLength(2, ErrorMessage = "El tipo de identificacion debe usar el codigo oficial SRI.")]
    public string TipoIdentificacion { get; set; } = "05";

    [Required(ErrorMessage = "La identificacion es obligatoria.")]
    [StringLength(30, ErrorMessage = "La identificacion no puede superar 30 caracteres.")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razon social o nombres completos son obligatorios.")]
    [StringLength(180, ErrorMessage = "La razon social o nombres completos no pueden superar 180 caracteres.")]
    public string RazonSocialONombresCompletos { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "El nombre comercial no puede superar 150 caracteres.")]
    public string? NombreComercial { get; set; }

    [Required(ErrorMessage = "La direccion principal es obligatoria.")]
    [StringLength(250, ErrorMessage = "La direccion principal no puede superar 250 caracteres.")]
    public string DireccionPrincipal { get; set; } = string.Empty;

    public string? RegionCodigo { get; set; }

    public string? ProvinciaCodigo { get; set; }

    public string? CiudadCodigo { get; set; }

    public string? SectorCodigo { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
    [StringLength(180, ErrorMessage = "El correo no puede superar 180 caracteres.")]
    public string? CorreoElectronicoPrincipal { get; set; }

    [StringLength(40, ErrorMessage = "El celular no puede superar 40 caracteres.")]
    public string? TelefonoCelular { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [StringLength(30, ErrorMessage = "El genero no puede superar 30 caracteres.")]
    public string? Genero { get; set; }

    [StringLength(50, ErrorMessage = "El codigo de empleado no puede superar 50 caracteres.")]
    public string? CodigoEmpleado { get; set; }

    [StringLength(50, ErrorMessage = "El codigo biometrico no puede superar 50 caracteres.")]
    public string? CodigoBiometrico { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public DateOnly? FechaSalida { get; set; }

    [Required(ErrorMessage = "El tipo de contrato es obligatorio.")]
    [StringLength(30, ErrorMessage = "El tipo de contrato no puede superar 30 caracteres.")]
    public string TipoContrato { get; set; } = "Indefinido";

    [StringLength(120, ErrorMessage = "El cargo no puede superar 120 caracteres.")]
    public string? CargoPuesto { get; set; }

    [Range(0, 999999999, ErrorMessage = "El sueldo base no puede ser negativo.")]
    public decimal SueldoBase { get; set; }

    [Range(0, 100, ErrorMessage = "La comision debe estar entre 0 y 100.")]
    public decimal PorcentajeComisionVentas { get; set; }

    [Required(ErrorMessage = "El estado laboral es obligatorio.")]
    [StringLength(30, ErrorMessage = "El estado laboral no puede superar 30 caracteres.")]
    public string EstadoLaboral { get; set; } = "Activo";

    [StringLength(150, ErrorMessage = "El nombre del contacto de emergencia no puede superar 150 caracteres.")]
    public string? NombreContactoEmergencia { get; set; }

    [StringLength(40, ErrorMessage = "El telefono de emergencia no puede superar 40 caracteres.")]
    public string? TelefonoEmergencia { get; set; }

    public bool IsActive { get; set; } = true;
}
