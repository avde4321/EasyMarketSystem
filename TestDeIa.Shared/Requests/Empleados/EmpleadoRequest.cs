using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Empleados;

public sealed class EmpleadoRequest
{
    [Required(ErrorMessage = "El tipo de identificacion es obligatorio.")]
    [StringLength(30, ErrorMessage = "El tipo de identificacion no puede superar 30 caracteres.")]
    public string TipoIdentificacion { get; set; } = "Cedula";

    [Required(ErrorMessage = "La identificacion es obligatoria.")]
    [StringLength(30, ErrorMessage = "La identificacion no puede superar 30 caracteres.")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(120, ErrorMessage = "Los nombres no pueden superar 120 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(120, ErrorMessage = "Los apellidos no pueden superar 120 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
    [StringLength(180, ErrorMessage = "El correo no puede superar 180 caracteres.")]
    public string? Email { get; set; }

    [StringLength(40, ErrorMessage = "El celular no puede superar 40 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(250, ErrorMessage = "La direccion no puede superar 250 caracteres.")]
    public string? Direccion { get; set; }

    public bool IsActive { get; set; } = true;
}
