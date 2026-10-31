using TestDeIa.Domain.Modules.Tesoreria.Enums;

namespace TestDeIa.Domain.Modules.Tesoreria.Entities;

public sealed class CuentaBancaria
{
    public Guid Id { get; init; }
    public Guid EmpresaId { get; init; }
    public string BancoNombre { get; init; } = string.Empty;
    public TipoCuentaBancaria TipoCuenta { get; init; }
    public string NumeroCuenta { get; init; } = string.Empty;
    public decimal SaldoContable { get; init; }
    public decimal SaldoConciliado { get; init; }
    public string Moneda { get; init; } = "USD";
    public Guid CuentaContableId { get; init; }
    public bool Activa { get; init; }
}
