using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Clientes;

public sealed class ClienteRequest
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

    [StringLength(40, ErrorMessage = "El telefono celular no puede superar 40 caracteres.")]
    public string? TelefonoCelular { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [StringLength(30, ErrorMessage = "El genero no puede superar 30 caracteres.")]
    public string? Genero { get; set; }

    [EmailAddress(ErrorMessage = "El correo de facturacion no tiene un formato valido.")]
    [StringLength(180, ErrorMessage = "El correo de facturacion no puede superar 180 caracteres.")]
    public string? CorreoFacturacionElectronica { get; set; }

    [Required(ErrorMessage = "El tipo de cliente es obligatorio.")]
    [StringLength(20, ErrorMessage = "El tipo de cliente no puede superar 20 caracteres.")]
    public string TipoCliente { get; set; } = "Natural";

    public bool ObligadoContabilidad { get; set; }

    public bool EsContribuyenteEspecial { get; set; }

    public bool PermiteCredito { get; set; }

    [Range(0, 999999999, ErrorMessage = "El limite de credito no puede ser negativo.")]
    public decimal LimiteCredito { get; set; }

    [Range(0, 3650, ErrorMessage = "Los dias maximos de credito no son validos.")]
    public int DiasCreditoMaximo { get; set; }

    [Required(ErrorMessage = "El estado de credito es obligatorio.")]
    [StringLength(20, ErrorMessage = "El estado de credito no puede superar 20 caracteres.")]
    public string EstadoCredito { get; set; } = "Normal";

    public bool IsActive { get; set; } = true;
}
