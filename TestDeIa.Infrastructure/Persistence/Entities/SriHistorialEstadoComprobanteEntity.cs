namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SriHistorialEstadoComprobanteEntity : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmpresaId { get; set; }

    public Guid ComprobanteId { get; set; }

    public string TipoDocumentoId { get; set; } = string.Empty;

    public int? EstadoAnteriorId { get; set; }

    public SriEstadoComprobanteEntity? EstadoAnterior { get; set; }

    public int EstadoNuevoId { get; set; }

    public SriEstadoComprobanteEntity EstadoNuevo { get; set; } = null!;

    public string? CodigoErrorSri { get; set; }

    public string? MensajeRespuesta { get; set; }

    public DateTimeOffset FechaTransaccion { get; set; } = DateTimeOffset.UtcNow;

    public Guid? UsuarioId { get; set; }

    public string? WorkerNode { get; set; }
}
