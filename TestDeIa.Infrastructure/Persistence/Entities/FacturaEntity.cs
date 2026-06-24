namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class FacturaEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }

    public long Secuencial { get; set; }

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public Guid ClienteId { get; set; }

    public Guid? EmpresaEmisoraId { get; set; }

    public string RucEmisor { get; set; } = string.Empty;

    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string? NombreComercialEmisor { get; set; }

    public string DireccionMatrizEmisor { get; set; } = string.Empty;

    public string? DireccionEstablecimientoEmisor { get; set; }

    public string AmbienteSri { get; set; } = string.Empty;

    public string TipoEmision { get; set; } = string.Empty;

    public bool ObligadoContabilidad { get; set; }

    public string? ContribuyenteEspecial { get; set; }

    public string? RegimenRimpe { get; set; }

    public string? AgenteRetencionResolucion { get; set; }

    public string ClienteTipoIdentificacion { get; set; } = string.Empty;

    public string ClienteIdentificacion { get; set; } = string.Empty;

    public string ClienteNombre { get; set; } = string.Empty;

    public string? ClienteDireccion { get; set; }

    public string? ClienteEmail { get; set; }

    public string? ClienteTelefono { get; set; }

    public string FormaPago { get; set; } = string.Empty;

    public string FormaPagoSriCodigo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal TotalDescuento { get; set; }

    public decimal SubtotalIva0 { get; set; }

    public decimal SubtotalIva5 { get; set; }

    public decimal SubtotalIva8 { get; set; }

    public decimal SubtotalIva15 { get; set; }

    public decimal IvaTotal { get; set; }

    public decimal Total { get; set; }

    public string? Observacion { get; set; }

    public string ClaveAcceso { get; set; } = string.Empty;

    public string? NumeroAutorizacion { get; set; }

    public string? MensajeEstado { get; set; }

    public string? XmlGenerado { get; set; }

    public string? XmlFirmado { get; set; }

    public DateTimeOffset FechaEmision { get; set; }

    public DateTimeOffset? FechaAutorizacion { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? ProcessingNode { get; set; }

    public DateTimeOffset? ProcessingStartedAt { get; set; }

    public int RetryCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public ICollection<FacturaDetalleEntity> Detalles { get; set; } = [];

    public ICollection<FacturaSriEventoEntity> EventosSri { get; set; } = [];
}
