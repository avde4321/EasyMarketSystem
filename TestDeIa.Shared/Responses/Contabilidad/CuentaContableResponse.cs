namespace TestDeIa.Shared.Responses.Contabilidad;

public sealed class CuentaContableResponse
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public string TipoCuenta { get; set; } = string.Empty;
    public bool EsAceptable { get; set; }
    public decimal SaldoActual { get; set; }
    public IReadOnlyCollection<CuentaContableResponse> Hijos { get; set; } = [];
}