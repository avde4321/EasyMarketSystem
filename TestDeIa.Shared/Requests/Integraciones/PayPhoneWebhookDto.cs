using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Integraciones;

public sealed class PayPhoneWebhookDto
{
    [Required]
    public Guid EmpresaId { get; set; }

    [Required]
    [StringLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ClientTransactionId { get; set; }

    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Monto { get; set; }

    [StringLength(1000)]
    public string? RawPayload { get; set; }
}
