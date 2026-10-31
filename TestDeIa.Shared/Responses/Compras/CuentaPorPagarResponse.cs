namespace TestDeIa.Shared.Responses.Compras;

public sealed class CuentaPorPagarResponse
{
    public Guid Id { get; set; }
    public Guid? CompraId { get; set; }
    public Guid ProveedorId { get; set; }
    public string ProveedorIdentificacion { get; set; } = string.Empty;
    public string ProveedorNombre { get; set; } = string.Empty;
    public DateTimeOffset FechaEmision { get; set; }
    public DateTimeOffset FechaVence { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoActual { get; set; }
    public string EstadoDeuda { get; set; } = string.Empty;
    public string NumeroComprobante { get; set; } = string.Empty;
    public bool EstaVencida { get; set; }
    public IReadOnlyCollection<PagoCxPResponse> Pagos { get; set; } = Array.Empty<PagoCxPResponse>();
}
