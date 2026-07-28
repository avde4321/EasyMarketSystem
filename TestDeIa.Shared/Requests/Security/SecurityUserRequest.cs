using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Security;

public sealed class SecurityUserRequest
{
    [Required(ErrorMessage = "El tipo de identificacion es obligatorio.")]
    [StringLength(30, ErrorMessage = "El tipo de identificacion no puede superar 30 caracteres.")]
    public string TipoIdentificacion { get; set; } = "05";

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

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(80, ErrorMessage = "El usuario no puede superar 80 caracteres.")]
    public string UserName { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "La clave no puede superar 80 caracteres.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Debe seleccionar al menos un rol.")]
    public IReadOnlyCollection<string> Roles { get; set; } = [];

    public bool IsActive { get; set; } = true;
}

