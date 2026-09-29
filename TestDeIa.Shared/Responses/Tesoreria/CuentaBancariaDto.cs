namespace TestDeIa.Shared.Responses.Tesoreria;

public sealed class CuentaBancariaDto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string BancoNombre { get; set; } = string.Empty;
    public byte TipoCuenta { get; set; }
    public string TipoCuentaNombre { get; set; } = string.Empty;
    public string NumeroCuenta { get; set; } = string.Empty;
    public decimal SaldoContable { get; set; }
    public decimal SaldoConciliado { get; set; }
    public string Moneda { get; set; } = "USD";
    public Guid CuentaContableId { get; set; }
    public string CuentaContableCodigo { get; set; } = string.Empty;
    public string CuentaContableNombre { get; set; } = string.Empty;
    public bool Activa { get; set; }
}
