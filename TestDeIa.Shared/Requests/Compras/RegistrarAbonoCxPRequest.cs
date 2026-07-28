using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Compras;

public sealed class RegistrarAbonoCxPRequest
{
    [Required]
    public Guid CuentaPorPagarId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "El monto abonado debe ser mayor a cero.")]
    public decimal MontoPagado { get; set; }

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string FormaPago { get; set; } = "01";

    [Required]
    public Guid CuentaContableSalidaId { get; set; }

    [StringLength(120)]
    public string? NumeroComprobantePago { get; set; }

    [StringLength(100)]
    public string? ReferenciaTransaccion { get; set; }

    public DateTimeOffset FechaPago { get; set; } = DateTimeOffset.Now;
}
