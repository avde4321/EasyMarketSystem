namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class EstadoFinancieroCuentaResponse
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public decimal Saldo { get; set; }
    public IReadOnlyCollection<EstadoFinancieroCuentaResponse> Hijos { get; set; } = [];
}
