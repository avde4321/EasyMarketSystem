namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class ConsumoDiarioHistoricoCompra
{
    public DateOnly Fecha { get; init; }
    public decimal Cantidad { get; init; }
}
