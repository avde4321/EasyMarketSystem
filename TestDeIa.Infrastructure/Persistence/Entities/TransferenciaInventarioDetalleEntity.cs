namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class TransferenciaInventarioDetalleEntity
{
    public Guid Id { get; set; }

    public Guid TransferenciaInventarioId { get; set; }

    public TransferenciaInventarioEntity TransferenciaInventario { get; set; } = default!;

    public Guid ProductoId { get; set; }

    public ProductoEntity Producto { get; set; } = default!;

    public decimal CantidadEnviada { get; set; }

    public decimal CantidadRecibida { get; set; }

    public decimal CostoUnitario { get; set; }
}
