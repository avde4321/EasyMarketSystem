using TestDeIa.Domain.Modules.Saas.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SaasSuscripcionEntity : ITenantEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public int SaasPlanId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public EstadoSuscripcionSaas EstadoSuscripcion { get; set; }

    public int FacturasEmitidasMesActual { get; set; }

    public DateTime? ContadorFacturasPeriodo { get; set; }

    public SaasPlanEntity Plan { get; set; } = default!;

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;
}
