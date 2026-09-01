using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Integraciones;

public sealed class GenerarLinkPagoDto
{
    public Guid? FacturaId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "El monto debe ser mayor a cero.", ParseLimitsInInvariantCulture = true)]
    public decimal Monto { get; set; }

    [Required]
    [StringLength(300)]
    public string Concepto { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(160)]
    public string? EmailCliente { get; set; }

    [StringLength(20)]
    public string? TelefonoCliente { get; set; }
}
