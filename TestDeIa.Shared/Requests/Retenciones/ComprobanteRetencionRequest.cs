namespace TestDeIa.Shared.Requests.Retenciones;

public sealed class ComprobanteRetencionRequest
{
    public Guid? CompraId { get; set; }

    public Guid ProveedorId { get; set; }

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public string? Secuencial { get; set; }

    public string? ClaveAcceso { get; set; }

    public DateTime FechaEmision { get; set; } = DateTime.Today;

    public byte AmbienteSRI { get; set; } = 1;

    public int EstadoSRI { get; set; }

    public string? NumeroAutorizacion { get; set; }

    public DateTime? FechaAutorizacion { get; set; }

    public string? MensajeErrorSRI { get; set; }

    public decimal TotalRetenido { get; set; }

    public IReadOnlyCollection<ComprobanteRetencionDetalleRequest> Detalles { get; set; } = Array.Empty<ComprobanteRetencionDetalleRequest>();
}
