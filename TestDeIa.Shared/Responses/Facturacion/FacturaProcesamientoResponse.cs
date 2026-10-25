namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class FacturaProcesamientoResponse
{
    public Guid Id { get; set; }
    public string Establecimiento { get; set; } = "";
    public string PuntoEmision { get; set; } = "";
    public long Secuencial { get; set; }
    public string Estado { get; set; } = "";
    public string? MensajeEstado { get; set; }
    public int RetryCount { get; set; }
    public string? ClaveAcceso { get; set; }
    public string? NumeroAutorizacion { get; set; }
}
