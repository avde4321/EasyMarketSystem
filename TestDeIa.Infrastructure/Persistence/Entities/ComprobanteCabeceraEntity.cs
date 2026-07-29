using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ComprobanteCabeceraEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string TipoDocumentoId { get; set; } = string.Empty;

    public long Secuencial { get; set; }

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public Guid? ComprobanteModificadoId { get; set; }

    public FacturaEntity? ComprobanteModificado { get; set; }

    public string? MotivoModificacion { get; set; }

    public string? CodDocModificado { get; set; }

    public string? NumDocModificado { get; set; }

    public DateTimeOffset? FechaEmisionDocSustento { get; set; }

    public string RucEmisor { get; set; } = string.Empty;

    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string? NombreComercialEmisor { get; set; }

    public string DireccionMatrizEmisor { get; set; } = string.Empty;

    public string? DireccionEstablecimientoEmisor { get; set; }

    public string AmbienteSri { get; set; } = string.Empty;

    public string TipoEmision { get; set; } = string.Empty;

    public bool ObligadoContabilidad { get; set; }

    public string ClienteTipoIdentificacion { get; set; } = string.Empty;

    public string ClienteIdentificacion { get; set; } = string.Empty;

    public string ClienteNombre { get; set; } = string.Empty;

    public string? ClienteDireccion { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TotalDescuento { get; set; }

    public decimal IvaTotal { get; set; }

    public decimal Total { get; set; }

    public string ClaveAcceso { get; set; } = string.Empty;

    public string? NumeroAutorizacion { get; set; }

    public string? XmlGenerado { get; set; }

    public string? XmlFirmado { get; set; }

    public FacturaEstado Estado { get; set; }

    public DateTimeOffset FechaEmision { get; set; }

    public DateTimeOffset? FechaAutorizacion { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ComprobanteDetalleEntity> Detalles { get; set; } = [];
}
