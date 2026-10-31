namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class TomaFisicaResultado
{
    public TomaFisicaResultado(
        Guid bodegaId,
        string bodegaNombre,
        int productosProcesados,
        int movimientosGenerados)
    {
        BodegaId = bodegaId;
        BodegaNombre = bodegaNombre;
        ProductosProcesados = productosProcesados;
        MovimientosGenerados = movimientosGenerados;
    }

    public Guid BodegaId { get; }
    public string BodegaNombre { get; }
    public int ProductosProcesados { get; }
    public int MovimientosGenerados { get; }
}
