using System.ComponentModel.DataAnnotations;

namespace TestDeIa.Shared.Requests.Saas;

public sealed class RegistrarTransferenciaSaasDto
{
    [Range(1, int.MaxValue)] public int SaasPlanId { get; set; }
    [Range(1, 12)] public int PeriodoMeses { get; set; } = 1;
    [Range(typeof(decimal), "0.0001", "99999999999999.9999", ParseLimitsInInvariantCulture = true)]
    public decimal Monto { get; set; }
    public DateOnly? FechaPago { get; set; }
    [Required, StringLength(200)] public string ComprobanteReferencia { get; set; } = string.Empty;
    [StringLength(2000)] public string? Observaciones { get; set; }
}

public sealed class ConfirmarPagoSaasDto
{
    public Guid PagoId { get; set; }
    public Guid EmpresaId { get; set; }
    public bool Aprobar { get; set; }
    [StringLength(2000)] public string? Observaciones { get; set; }
}

public sealed class CheckoutSaasDto
{
    [Range(1, int.MaxValue)] public int SaasPlanId { get; set; }
}
