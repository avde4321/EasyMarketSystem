namespace TestDeIa.Shared.Responses.Retenciones;

public sealed class CompraPendienteRetencionResponse
{
    public Guid CompraId { get; set; }
    public Guid ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public string ProveedorIdentificacion { get; set; } = string.Empty;
    public string NumeroComprobante { get; set; } = string.Empty;
    public DateTimeOffset FechaEmision { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal TotalIva { get; set; }
    public decimal ImporteTotal { get; set; }
    public string CodigoRetencionRenta { get; set; } = string.Empty;
    public string CodigoRetencionIva { get; set; } = string.Empty;
}
