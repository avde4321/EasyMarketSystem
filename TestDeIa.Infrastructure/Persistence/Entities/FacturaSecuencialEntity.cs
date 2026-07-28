namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class FacturaSecuencialEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string CodigoDocumento { get; set; } = string.Empty;
    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public long UltimoSecuencial { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
