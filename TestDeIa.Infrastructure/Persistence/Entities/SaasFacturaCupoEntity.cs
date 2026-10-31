namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SaasFacturaCupoEntity : ITenantEntity
{
    public Guid FacturaId { get; set; }
    public Guid EmpresaId { get; set; }
    public string Estado { get; set; } = "RESERVADO";
    public DateTimeOffset FechaReserva { get; set; }
    public DateTimeOffset? FechaConfirmacion { get; set; }
    public DateTime? PeriodoAutorizacion { get; set; }
}
