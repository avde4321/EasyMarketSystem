namespace TestDeIa.Shared.Responses.Compras;

public sealed class CuentasPorPagarResumenResponse
{
    public decimal TotalPorVencer { get; set; }
    public decimal TotalVencido { get; set; }
    public int TotalPendientes { get; set; }
    public int TotalVencidas { get; set; }
}
