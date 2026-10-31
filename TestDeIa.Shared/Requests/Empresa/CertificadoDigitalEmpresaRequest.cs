using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Empresa;

public sealed class CertificadoDigitalEmpresaRequest
{
    [Required]
    public Guid EmpresaId { get; set; }

    [Required(ErrorMessage = "El nombre del certificado es obligatorio.")]
    [StringLength(160, ErrorMessage = "El nombre del certificado no puede superar 160 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del archivo es obligatorio.")]
    [StringLength(260, ErrorMessage = "El nombre del archivo no puede superar 260 caracteres.")]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contenido del certificado es obligatorio.")]
    public byte[] Contenido { get; set; } = [];

    [Required(ErrorMessage = "La clave del certificado es obligatoria.")]
    [StringLength(200, ErrorMessage = "La clave del certificado no puede superar 200 caracteres.")]
    public string Clave { get; set; } = string.Empty;

    public bool ActivarComoPrincipal { get; set; } = true;
}
