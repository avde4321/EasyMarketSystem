namespace TestDeIa.Shared.Responses.Retenciones;

public sealed class ComprobanteRetencionResponse
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? CompraId { get; set; }

    public Guid ProveedorId { get; set; }

    public string ProveedorNombre { get; set; } = string.Empty;

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public string Secuencial { get; set; } = string.Empty;

    public string? ClaveAcceso { get; set; }

    public DateTime FechaEmision { get; set; }

    public byte AmbienteSRI { get; set; }

    public int EstadoSRI { get; set; }

    public string EstadoSRINombre { get; set; } = string.Empty;

    public string? NumeroAutorizacion { get; set; }

    public DateTime? FechaAutorizacion { get; set; }

    public string? MensajeErrorSRI { get; set; }

    public decimal TotalRetenido { get; set; }

    public IReadOnlyCollection<ComprobanteRetencionDetalleResponse> Detalles { get; set; } = Array.Empty<ComprobanteRetencionDetalleResponse>();
}
