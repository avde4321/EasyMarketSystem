namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class CuentasPorPagarResumen
{
    public decimal TotalPorVencer { get; init; }
    public decimal TotalVencido { get; init; }
    public int TotalPendientes { get; init; }
    public int TotalVencidas { get; init; }
}
