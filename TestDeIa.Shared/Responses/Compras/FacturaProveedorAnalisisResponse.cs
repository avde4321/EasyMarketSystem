namespace TestDeIa.Shared.Responses.Compras;

public sealed class FacturaProveedorAnalisisResponse
{
    public bool Succeeded { get; set; }

    public string Message { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string DocumentoTipo { get; set; } = "01";

    public string? ProveedorRuc { get; set; }

    public string? ProveedorNombre { get; set; }

    public string? NumeroComprobante { get; set; }

    public string? ClaveAcceso { get; set; }

    public string? NumeroAutorizacion { get; set; }

    public DateTime? FechaEmision { get; set; }

    public decimal BaseIva0 { get; set; }

    public decimal BaseIva15 { get; set; }

    public decimal Iva { get; set; }

    public decimal Total { get; set; }

    public decimal Confidence { get; set; }

    public List<FacturaProveedorAnalisisDetalleResponse> Detalles { get; set; } = [];
}
