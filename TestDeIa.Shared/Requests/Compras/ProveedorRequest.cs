using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Compras;

public sealed class ProveedorRequest
{
    [Required(ErrorMessage = "El tipo de identificacion es obligatorio.")]
    [StringLength(2, ErrorMessage = "El tipo de identificacion debe usar el codigo oficial SRI.")]
    public string TipoIdentificacion { get; set; } = "04";

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

    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
    [StringLength(180, ErrorMessage = "El correo no puede superar 180 caracteres.")]
    public string? CorreoElectronicoPrincipal { get; set; }

    [StringLength(40, ErrorMessage = "El telefono celular no puede superar 40 caracteres.")]
    public string? TelefonoCelular { get; set; }

    [Required(ErrorMessage = "El codigo de retencion IVA es obligatorio.")]
    [StringLength(20, ErrorMessage = "El codigo de retencion IVA no puede superar 20 caracteres.")]
    public string CodigoRetencionIvaDefault { get; set; } = "0";

    [Required(ErrorMessage = "El codigo de retencion renta es obligatorio.")]
    [StringLength(20, ErrorMessage = "El codigo de retencion renta no puede superar 20 caracteres.")]
    public string CodigoRetencionRentaDefault { get; set; } = "0";

    public bool PermiteCredito { get; set; }

    [Range(0, 3650, ErrorMessage = "Los dias de credito no son validos.")]
    public int DiasCredito { get; set; }

    [Required(ErrorMessage = "El estado del proveedor es obligatorio.")]
    [StringLength(20, ErrorMessage = "El estado del proveedor no puede superar 20 caracteres.")]
    public string EstadoProveedor { get; set; } = "Activo";

    public bool IsActive { get; set; } = true;
}