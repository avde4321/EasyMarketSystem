namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class EstadoResultadosResponse
{
    public IReadOnlyCollection<EstadoFinancieroCuentaResponse> Ingresos { get; set; } = [];
    public IReadOnlyCollection<EstadoFinancieroCuentaResponse> Costos { get; set; } = [];
    public IReadOnlyCollection<EstadoFinancieroCuentaResponse> Gastos { get; set; } = [];
    public decimal TotalIngresos { get; set; }
    public decimal TotalCostos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal UtilidadOPerdida { get; set; }
}
