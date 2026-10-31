namespace TestDeIa.Shared.Reports.Facturacion;

public sealed class NotaCreditoRideReportDto
{
    public Guid ComprobanteId { get; set; }

    public string NumeroComprobante { get; set; } = string.Empty;

    public string ClaveAcceso { get; set; } = string.Empty;

    public string NumeroAutorizacion { get; set; } = string.Empty;

    public DateTimeOffset FechaEmision { get; set; }

    public string RucEmisor { get; set; } = string.Empty;

    public string RazonSocialEmisor { get; set; } = string.Empty;

    public string NombreComercialEmisor { get; set; } = string.Empty;

    public string DireccionMatrizEmisor { get; set; } = string.Empty;

    public string DireccionEstablecimientoEmisor { get; set; } = string.Empty;

    public string ClienteIdentificacion { get; set; } = string.Empty;

    public string ClienteNombre { get; set; } = string.Empty;

    public string ClienteDireccion { get; set; } = string.Empty;

    public string CodDocModificado { get; set; } = string.Empty;

    public string NumDocModificado { get; set; } = string.Empty;

    public DateTimeOffset FechaEmisionDocSustento { get; set; }

    public string MotivoModificacion { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal TotalDescuento { get; set; }

    public decimal IvaTotal { get; set; }

    public decimal Total { get; set; }

    public byte[] QrImage { get; set; } = [];

    public IReadOnlyCollection<NotaCreditoRideDetalleDto> Detalles { get; set; } = [];

    public IReadOnlyCollection<NotaCreditoRideTotalImpuestoDto> TotalesImpuesto { get; set; } = [];
}
