using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Personas;

public sealed class PersonaRequest
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

    [StringLength(80, ErrorMessage = "La region no puede superar 80 caracteres.")]
    public string? RegionCodigo { get; set; }

    [StringLength(80, ErrorMessage = "La provincia no puede superar 80 caracteres.")]
    public string? ProvinciaCodigo { get; set; }

    [StringLength(80, ErrorMessage = "La ciudad no puede superar 80 caracteres.")]
    public string? CiudadCodigo { get; set; }

    [StringLength(80, ErrorMessage = "El sector no puede superar 80 caracteres.")]
    public string? SectorCodigo { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
    [StringLength(180, ErrorMessage = "El correo no puede superar 180 caracteres.")]
    public string? CorreoElectronicoPrincipal { get; set; }

    [StringLength(40, ErrorMessage = "El celular no puede superar 40 caracteres.")]
    public string? TelefonoCelular { get; set; }

    [StringLength(30, ErrorMessage = "El genero no puede superar 30 caracteres.")]
    public string? Genero { get; set; }

    public bool EsPersonaJuridica { get; set; }

    public bool EsEmpresa { get; set; }

    public bool IsActive { get; set; } = true;
}
