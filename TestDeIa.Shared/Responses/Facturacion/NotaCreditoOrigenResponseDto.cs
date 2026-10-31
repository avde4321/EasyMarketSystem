namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class NotaCreditoOrigenResponseDto
{
    public Guid FacturaId { get; set; }

    public string NumeroComprobante { get; set; } = string.Empty;

    public string ClienteNombre { get; set; } = string.Empty;

    public DateTimeOffset FechaEmision { get; set; }

    public decimal Total { get; set; }

    public IReadOnlyCollection<NotaCreditoOrigenDetalleResponseDto> Detalles { get; set; } = [];
}
