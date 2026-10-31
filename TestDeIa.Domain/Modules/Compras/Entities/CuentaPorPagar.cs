namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class CuentaPorPagar
{
    public CuentaPorPagar(
        Guid id,
        Guid empresaId,
        Guid? compraId,
        Guid proveedorId,
        string proveedorIdentificacion,
        string proveedorNombre,
        string numeroComprobante,
        DateTimeOffset fechaEmision,
        DateTimeOffset fechaVence,
        decimal montoOriginal,
        decimal saldoActual,
        EstadoDeuda estadoDeuda,
        DateTimeOffset createdAt,
        Guid usuarioCreacionId,
        DateTimeOffset? updatedAt,
        Guid? usuarioModificacionId,
        IReadOnlyCollection<PagoCxP> pagos)
    {
        Id = id;
        EmpresaId = empresaId;
        CompraId = compraId;
        ProveedorId = proveedorId;
        ProveedorIdentificacion = proveedorIdentificacion;
        ProveedorNombre = proveedorNombre;
        NumeroComprobante = numeroComprobante;
        FechaEmision = fechaEmision;
        FechaVence = fechaVence;
        MontoOriginal = montoOriginal;
        SaldoActual = saldoActual;
        EstadoDeuda = estadoDeuda;
        CreatedAt = createdAt;
        UsuarioCreacionId = usuarioCreacionId;
        UpdatedAt = updatedAt;
        UsuarioModificacionId = usuarioModificacionId;
        Pagos = pagos;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public Guid? CompraId { get; }
    public Guid ProveedorId { get; }
    public string ProveedorIdentificacion { get; }
    public string ProveedorNombre { get; }
    public string NumeroComprobante { get; }
    public DateTimeOffset FechaEmision { get; }
    public DateTimeOffset FechaVence { get; }
    public decimal MontoOriginal { get; }
    public decimal SaldoActual { get; }
    public EstadoDeuda EstadoDeuda { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid UsuarioCreacionId { get; }
    public DateTimeOffset? UpdatedAt { get; }
    public Guid? UsuarioModificacionId { get; }
    public IReadOnlyCollection<PagoCxP> Pagos { get; }
}
