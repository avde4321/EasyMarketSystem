namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class FacturaEmissionResponse
{
    public Guid FacturaId { get; set; }

    public long Secuencial { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string NumeroComprobante { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;
}
