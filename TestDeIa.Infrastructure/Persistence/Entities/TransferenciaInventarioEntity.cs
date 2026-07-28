namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class TransferenciaInventarioEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid BodegaOrigenId { get; set; }

    public BodegaEntity BodegaOrigen { get; set; } = default!;

    public Guid BodegaDestinoId { get; set; }

    public BodegaEntity BodegaDestino { get; set; } = default!;

    public DateTimeOffset FechaEmision { get; set; }

    public DateTimeOffset? FechaTraslado { get; set; }

    public string Estado { get; set; } = "Borrador";

    public string MotivoTraslado { get; set; } = string.Empty;

    public Guid? GuiaRemisionId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<TransferenciaInventarioDetalleEntity> Detalles { get; set; } = [];
}
