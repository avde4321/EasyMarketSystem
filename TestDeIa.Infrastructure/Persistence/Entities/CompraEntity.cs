using TestDeIa.Domain.Modules.Compras.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CompraEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid ProveedorId { get; set; }
    public ProveedorEntity Proveedor { get; set; } = default!;
    public Guid BodegaId { get; set; }
    public BodegaEntity Bodega { get; set; } = default!;
    public NaturalezaCompra NaturalezaCompra { get; set; } = NaturalezaCompra.MercaderiaInventario;
    public string TipoDocumentoCodigo { get; set; } = string.Empty;
    public string TipoComprobanteSRI { get; set; } = string.Empty;
    public string SustentoTributarioSRI { get; set; } = string.Empty;
    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public string Secuencial { get; set; } = string.Empty;
    public string? ClaveAccesoProveedor { get; set; }
    public string? ClaveAccesoGenerada { get; set; }
    public string? NumeroAutorizacion { get; set; }
    public TestDeIa.Domain.Modules.Facturacion.Entities.FacturaEstado? EstadoSri { get; set; }
    public string? MensajeEstado { get; set; }
    public string FormaPagoSriCodigo { get; set; } = string.Empty;
    public FormaPagoCompra FormaPagoCompra { get; set; } = FormaPagoCompra.ContadoEfectivo;
    public bool RequiereBancarizacion { get; set; }
    public string? Observacion { get; set; }
    public string? XmlGenerado { get; set; }
    public string? XmlFirmado { get; set; }
    public string? ProcessingNode { get; set; }
    public DateTimeOffset? ProcessingStartedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
    public DateTimeOffset FechaEmision { get; set; }
    public decimal SubtotalIva0 { get; set; }
    public decimal SubtotalIva5 { get; set; }
    public decimal SubtotalIva8 { get; set; }
    public decimal SubtotalIva15 { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal TotalImpuestos { get; set; }
    public decimal ImporteTotal { get; set; }
    public string EstadoCompra { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
    public ICollection<CompraDetalleEntity> Detalles { get; set; } = [];
}




