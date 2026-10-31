using TestDeIa.Domain.Modules.Tesoreria.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CuentaBancariaEntity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string BancoNombre { get; set; } = string.Empty;
    public TipoCuentaBancaria TipoCuenta { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public decimal SaldoContable { get; set; }
    public decimal SaldoConciliado { get; set; }
    public string Moneda { get; set; } = "USD";
    public Guid CuentaContableId { get; set; }
    public bool Activa { get; set; } = true;

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;
    public CuentaContableEntity CuentaContable { get; set; } = default!;
    public ICollection<ExtractoBancarioHeaderEntity> Extractos { get; set; } = [];
    public ICollection<MovimientoTesoreriaEntity> MovimientosTesoreria { get; set; } = [];
}
