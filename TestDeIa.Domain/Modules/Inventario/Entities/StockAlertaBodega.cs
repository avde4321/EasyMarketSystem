namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class StockAlertaBodega
{
    public StockAlertaBodega(Guid bodegaId, string bodegaNombre, decimal stockActual)
    {
        BodegaId = bodegaId;
        BodegaNombre = bodegaNombre;
        StockActual = stockActual;
    }

    public Guid BodegaId { get; }
    public string BodegaNombre { get; }
    public decimal StockActual { get; }
}
