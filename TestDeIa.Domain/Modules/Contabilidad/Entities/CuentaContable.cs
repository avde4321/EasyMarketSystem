using TestDeIa.Domain.Modules.Contabilidad.Enums;

namespace TestDeIa.Domain.Modules.Contabilidad.Entities;

public sealed class CuentaContable
{
    public CuentaContable(
        Guid id,
        Guid empresaId,
        string codigo,
        string nombre,
        int nivel,
        TipoCuentaContable tipoCuenta,
        bool esAceptable,
        decimal saldoActual)
    {
        Id = id;
        EmpresaId = empresaId;
        Codigo = codigo;
        Nombre = nombre;
        Nivel = nivel;
        TipoCuenta = tipoCuenta;
        EsAceptable = esAceptable;
        SaldoActual = saldoActual;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public string Codigo { get; }
    public string Nombre { get; }
    public int Nivel { get; }
    public TipoCuentaContable TipoCuenta { get; }
    public bool EsAceptable { get; }
    public decimal SaldoActual { get; }
}