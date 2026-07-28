namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CuentaPorPagarEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid? CompraId { get; set; }
    public CompraEntity? Compra { get; set; }
    public Guid ProveedorId { get; set; }
    public ProveedorEntity Proveedor { get; set; } = default!;
    public DateTimeOffset FechaEmision { get; set; }
    public DateTimeOffset FechaVence { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoActual { get; set; }
    public string EstadoDeuda { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid UsuarioCreacionId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UsuarioModificacionId { get; set; }
    public ICollection<PagoCxPEntity> Pagos { get; set; } = [];
}
