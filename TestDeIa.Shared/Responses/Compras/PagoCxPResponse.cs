namespace TestDeIa.Shared.Responses.Compras;

public sealed class PagoCxPResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public Guid? CuentaContableSalidaId { get; set; }
    public string? NumeroComprobantePago { get; set; }
    public string? ReferenciaTransaccion { get; set; }
}
