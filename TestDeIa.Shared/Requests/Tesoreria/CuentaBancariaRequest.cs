using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Tesoreria;

public sealed class CuentaBancariaRequest
{
    [Required]
    [MaxLength(100)]
    public string BancoNombre { get; set; } = string.Empty;

    [Range(1, 3)]
    public byte TipoCuenta { get; set; }

    [Required]
    [MaxLength(30)]
    public string NumeroCuenta { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999999.9999", ParseLimitsInInvariantCulture = true)]
    public decimal SaldoContable { get; set; }

    [Range(typeof(decimal), "0", "999999999999.9999", ParseLimitsInInvariantCulture = true)]
    public decimal SaldoConciliado { get; set; }

    [Required]
    [MaxLength(3)]
    public string Moneda { get; set; } = "USD";

    [Required]
    public Guid CuentaContableId { get; set; }

    public bool Activa { get; set; } = true;
}
