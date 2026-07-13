namespace TestDeIa.Shared.Responses.Compras;

public sealed class PagoCxPResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public string? ReferenciaTransaccion { get; set; }
}
