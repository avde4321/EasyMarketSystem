namespace TestDeIa.Shared.Responses.Inventario;

public sealed class TransferenciaInventarioResponse
{
    public Guid Id { get; set; }

    public Guid BodegaOrigenId { get; set; }

    public string BodegaOrigenNombre { get; set; } = string.Empty;

    public Guid BodegaDestinoId { get; set; }

    public string BodegaDestinoNombre { get; set; } = string.Empty;

    public DateTimeOffset FechaEmision { get; set; }

    public DateTimeOffset? FechaTraslado { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string MotivoTraslado { get; set; } = string.Empty;

    public Guid? GuiaRemisionId { get; set; }

    public decimal TotalUnidades { get; set; }

    public decimal TotalRecibido { get; set; }

    public IReadOnlyCollection<TransferenciaInventarioDetalleResponse> Detalles { get; set; } = [];
}
