namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class ProductoBodega
{
    public ProductoBodega(
        Guid productoId,
        Guid bodegaId,
        decimal stockActual)
    {
        ProductoId = productoId;
        BodegaId = bodegaId;
        StockActual = stockActual;
    }

    public Guid ProductoId { get; }

    public Guid BodegaId { get; }

    public decimal StockActual { get; }
}
