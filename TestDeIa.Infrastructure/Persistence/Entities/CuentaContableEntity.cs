using TestDeIa.Domain.Modules.Contabilidad.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class CuentaContableEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public TipoCuentaContable TipoCuenta { get; set; }
    public bool EsAceptable { get; set; }
    public decimal SaldoActual { get; set; }
}
