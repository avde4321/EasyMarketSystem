namespace TestDeIa.Domain.Modules.Dashboard.Entities;

public sealed class ConsumoDiarioHistorico
{
    public DateOnly Fecha { get; init; }
    public decimal Cantidad { get; init; }
}
