namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ColaProcesamientoSriEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ComprobanteId { get; set; }

    public string TipoDocumentoId { get; set; } = string.Empty;

    public string Estado { get; set; } = "Pendiente";

    public int Intentos { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? Mensaje { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
