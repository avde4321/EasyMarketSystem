namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class BalanceGeneralResponse
{
    public IReadOnlyCollection<EstadoFinancieroCuentaResponse> Activos { get; set; } = [];
    public IReadOnlyCollection<EstadoFinancieroCuentaResponse> Pasivos { get; set; } = [];
    public IReadOnlyCollection<EstadoFinancieroCuentaResponse> Patrimonio { get; set; } = [];
    public decimal TotalActivo { get; set; }
    public decimal TotalPasivo { get; set; }
    public decimal TotalPatrimonio { get; set; }
    public decimal TotalPasivoPatrimonio { get; set; }
    public decimal DiferenciaEcuacion { get; set; }
}
