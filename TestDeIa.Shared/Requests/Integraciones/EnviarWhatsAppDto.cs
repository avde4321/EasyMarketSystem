using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Integraciones;

public sealed class EnviarWhatsAppDto
{
    public Guid? DocumentoId { get; set; }

    [Required]
    [StringLength(20)]
    public string NumeroTelefono { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string NombreCliente { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string TipoDoc { get; set; } = string.Empty;

    [Url]
    [StringLength(500)]
    public string? LinkPdfRide { get; set; }

    [Url]
    [StringLength(500)]
    public string? LinkXml { get; set; }

    [StringLength(1000)]
    public string? MensajePersonalizado { get; set; }
}
