using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Empresa;

public sealed class EmpresaRequest
{
    [Required(ErrorMessage = "La razon social es obligatoria.")]
    [StringLength(300, ErrorMessage = "La razon social no puede superar 300 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "El nombre comercial no puede superar 300 caracteres.")]
    public string? NombreComercial { get; set; }

    [Required(ErrorMessage = "El RUC es obligatorio.")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "El RUC debe tener 13 digitos.")]
    public string Ruc { get; set; } = string.Empty;

    [Required(ErrorMessage = "La direccion matriz es obligatoria.")]
    [StringLength(300, ErrorMessage = "La direccion matriz no puede superar 300 caracteres.")]
    public string DireccionMatriz { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ambiente SRI es obligatorio.")]
    public string AmbienteSri { get; set; } = "Pruebas";

    public bool ModoDesarrollo { get; set; } = true;

    [Required(ErrorMessage = "El tipo de emision es obligatorio.")]
    public string TipoEmision { get; set; } = "Normal";

    public bool ObligadoContabilidad { get; set; }

    [StringLength(40, ErrorMessage = "El numero de contribuyente especial no puede superar 40 caracteres.")]
    public string? ContribuyenteEspecial { get; set; }

    [StringLength(60, ErrorMessage = "El regimen RIMPE no puede superar 60 caracteres.")]
    public string? RegimenRimpe { get; set; }

    [StringLength(60, ErrorMessage = "La resolucion de agente de retencion no puede superar 60 caracteres.")]
    public string? AgenteRetencionResolucion { get; set; }

    [StringLength(260, ErrorMessage = "El nombre del certificado no puede superar 260 caracteres.")]
    public string? CertificadoNombreArchivo { get; set; }

    public byte[]? CertificadoContenido { get; set; }

    [StringLength(200, ErrorMessage = "La clave del certificado no puede superar 200 caracteres.")]
    public string? CertificadoClave { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Debes registrar al menos un punto de emision.")]
    public List<EmpresaPuntoEmisionRequest> PuntosEmision { get; set; } = [new() { IsDefault = true }];
}
