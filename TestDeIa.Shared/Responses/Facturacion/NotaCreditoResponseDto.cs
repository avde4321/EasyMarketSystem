namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class NotaCreditoResponseDto
{
    public Guid ComprobanteId { get; set; }

    public Guid FacturaOrigenId { get; set; }

    public string TipoDocumentoId { get; set; } = "04";

    public string NumeroComprobante { get; set; } = string.Empty;

    public string ClaveAcceso { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public decimal Total { get; set; }
}
