using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Caja;

public sealed class RegistrarMovimientoCajaRequest
{
    [Required]
    [MaxLength(30)]
    public string TipoMovimiento { get; set; } = string.Empty;

    [Range(0, 999999999, ErrorMessage = "El monto debe ser positivo.")]
    public decimal Monto { get; set; }

    [Required]
    [MaxLength(250)]
    public string Concepto { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? ComprobanteReferencia { get; set; }
}
