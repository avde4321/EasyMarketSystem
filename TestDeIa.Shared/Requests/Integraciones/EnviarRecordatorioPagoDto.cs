using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Integraciones;

public sealed class EnviarRecordatorioPagoDto
{
    [Required]
    [StringLength(20)]
    public string NumeroTelefono { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string NombreCliente { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "El saldo pendiente debe ser mayor a cero.", ParseLimitsInInvariantCulture = true)]
    public decimal SaldoPendiente { get; set; }

    [Url]
    [StringLength(500)]
    public string? LinkPago { get; set; }

    [StringLength(1000)]
    public string? MensajePersonalizado { get; set; }

    public Guid? DocumentoId { get; set; }
}
