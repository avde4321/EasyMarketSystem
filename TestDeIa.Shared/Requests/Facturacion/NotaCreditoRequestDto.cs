namespace TestDeIa.Shared.Requests.Facturacion;

public sealed class NotaCreditoRequestDto
{
    public Guid FacturaId { get; set; }

    public string MotivoModificacion { get; set; } = string.Empty;

    public IReadOnlyCollection<NotaCreditoDetalleRequestDto> Detalles { get; set; } = [];
}
