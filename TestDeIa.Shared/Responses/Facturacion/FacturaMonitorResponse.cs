namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class FacturaMonitorResponse
{
    public Guid Id { get; set; }

    public long Secuencial { get; set; }

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public string NumeroComprobante => $"{Establecimiento}-{PuntoEmision}-{Secuencial:000000000}";

    public string ClienteIdentificacion { get; set; } = string.Empty;

    public string ClienteTipoIdentificacion { get; set; } = string.Empty;

    public string ClienteNombre { get; set; } = string.Empty;

    public string FormaPago { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal IvaTotal { get; set; }

    public decimal Total { get; set; }

    public string? ClaveAcceso { get; set; }

    public string? NumeroAutorizacion { get; set; }

    public string? MensajeEstado { get; set; }

    public DateTimeOffset FechaEmision { get; set; }

    public DateTimeOffset? FechaAutorizacion { get; set; }
}
