using TestDeIa.Domain.Modules.Saas.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SaasPagoRegistroEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public int SaasPlanId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public DateOnly? FechaPagoInformada { get; set; }
    public MetodoPagoSaas MetodoPago { get; set; }
    public EstadoPagoSaas EstadoPago { get; set; }
    public string ComprobanteReferencia { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public Guid RegistradoPorUsuarioId { get; set; }
    public Guid? ConfirmadoPorUsuarioId { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public string? ObservacionesConfirmacion { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EmpresaEmisoraEntity Empresa { get; set; } = default!;
    public SaasPlanEntity Plan { get; set; } = default!;
}
