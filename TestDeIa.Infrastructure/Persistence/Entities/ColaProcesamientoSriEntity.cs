namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ColaProcesamientoSriEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmpresaId { get; set; }

    public Guid ComprobanteId { get; set; }

    public string TipoDocumentoId { get; set; } = string.Empty;

    public string Estado { get; set; } = TestDeIa.Domain.Modules.Sri.SriOutboxEstados.Pendiente;

    public int Intentos { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? Mensaje { get; set; }

    public string? UltimoError { get; set; }

    public string? ProcessingNode { get; set; }

    public DateTimeOffset? ProcessingStartedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;
}
