namespace TestDeIa.Shared.Requests.OfflinePos;

public sealed class VentaOfflineQueueDto
{
    public Guid LocalQueueId { get; set; } = Guid.NewGuid();

    public Guid EmpresaId { get; set; }

    public Guid ClienteId { get; set; }

    public Guid BodegaId { get; set; }

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public string FormaPago { get; set; } = string.Empty;

    public decimal? MontoRecibido { get; set; }

    public decimal? VueltoEntregado { get; set; }

    public string? Observacion { get; set; }

    public DateTimeOffset FechaHoraLocal { get; set; } = DateTimeOffset.Now;

    public string FirmaPreliminarLocal { get; set; } = string.Empty;

    public int IntentosSincronizacion { get; set; }

    public string EstadoLocal { get; set; } = "Pendiente";

    public IReadOnlyCollection<VentaOfflineDetalleDto> Items { get; set; } = Array.Empty<VentaOfflineDetalleDto>();
}

public sealed class VentaOfflineDetalleDto
{
    public Guid ProductoId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal TarifaIVA { get; set; }

    public Guid? UsuarioIdOperador { get; set; }
}
