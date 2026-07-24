namespace TestDeIa.Shared.Responses.Compras;

public sealed class CompraResponse
{
    public Guid Id { get; set; }
    public Guid ProveedorId { get; set; }
    public Guid BodegaId { get; set; }
    public string NaturalezaCompra { get; set; } = string.Empty;
    public string FormaPagoCompra { get; set; } = string.Empty;
    public bool RequiereBancarizacion { get; set; }
    public string TipoDocumentoCodigo { get; set; } = string.Empty;
    public string TipoComprobanteSRI { get; set; } = string.Empty;
    public string SustentoTributarioSRI { get; set; } = string.Empty;
    public string TipoDocumentoNombre { get; set; } = string.Empty;
    public string NumeroComprobante { get; set; } = string.Empty;
    public string? ClaveAccesoProveedor { get; set; }
    public string? ClaveAccesoGenerada { get; set; }
    public string? NumeroAutorizacion { get; set; }
    public string? EstadoSri { get; set; }
    public string? MensajeEstado { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public string? Observacion { get; set; }
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
    public IReadOnlyCollection<CompraDetalleResponse> Detalles { get; set; } = Array.Empty<CompraDetalleResponse>();
}
