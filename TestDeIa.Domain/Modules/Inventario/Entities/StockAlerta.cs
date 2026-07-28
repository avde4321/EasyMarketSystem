namespace TestDeIa.Domain.Modules.Inventario.Entities;

public sealed class StockAlerta
{
    public StockAlerta(
        Guid productoId,
        string codigo,
        string nombre,
        decimal stockMinimo,
        decimal stockTotal,
        IReadOnlyCollection<StockAlertaBodega> bodegasComprometidas)
    {
        ProductoId = productoId;
        Codigo = codigo;
        Nombre = nombre;
        StockMinimo = stockMinimo;
        StockTotal = stockTotal;
        BodegasComprometidas = bodegasComprometidas;
    }

    public Guid ProductoId { get; }
    public string Codigo { get; }
    public string Nombre { get; }
    public decimal StockMinimo { get; }
    public decimal StockTotal { get; }
    public IReadOnlyCollection<StockAlertaBodega> BodegasComprometidas { get; }
}
