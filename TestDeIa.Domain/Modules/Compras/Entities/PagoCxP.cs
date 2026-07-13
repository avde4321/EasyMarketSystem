namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class PagoCxP
{
    public PagoCxP(
        Guid id,
        Guid cuentaPorPagarId,
        DateTimeOffset fechaPago,
        decimal montoPagado,
        string formaPago,
        string? referenciaTransaccion,
        DateTimeOffset createdAt,
        Guid usuarioCreacionId)
    {
        Id = id;
        CuentaPorPagarId = cuentaPorPagarId;
        FechaPago = fechaPago;
        MontoPagado = montoPagado;
        FormaPago = formaPago;
        ReferenciaTransaccion = referenciaTransaccion;
        CreatedAt = createdAt;
        UsuarioCreacionId = usuarioCreacionId;
    }

    public Guid Id { get; }
    public Guid CuentaPorPagarId { get; }
    public DateTimeOffset FechaPago { get; }
    public decimal MontoPagado { get; }
    public string FormaPago { get; }
    public string? ReferenciaTransaccion { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid UsuarioCreacionId { get; }
}
