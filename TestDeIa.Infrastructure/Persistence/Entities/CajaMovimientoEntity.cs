namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CajaMovimientoEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid CajaSesionId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public string? ComprobanteReferencia { get; set; }
    public Guid CreadoPorUsuarioId { get; set; }
    public DateTimeOffset FechaMovimiento { get; set; }

    public CajaSesionEntity CajaSesion { get; set; } = default!;
}
