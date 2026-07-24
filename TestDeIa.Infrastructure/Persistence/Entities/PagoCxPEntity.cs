namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class PagoCxPEntity
{
    public Guid Id { get; set; }
    public Guid CuentaPorPagarId { get; set; }
    public CuentaPorPagarEntity CuentaPorPagar { get; set; } = default!;
    public DateTimeOffset FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public Guid? CuentaContableSalidaId { get; set; }
    public CuentaContableEntity? CuentaContableSalida { get; set; }
    public string? NumeroComprobantePago { get; set; }
    public string? ReferenciaTransaccion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
}
