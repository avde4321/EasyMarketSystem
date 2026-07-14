using TestDeIa.Domain.Modules.Contabilidad.Enums;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Contabilidad;

public sealed class CatalogoNiifSeed
{
    private static readonly SeedCuenta[] BaseCuentas =
    [
        new("1", "Activo", 1, TipoCuentaContable.Activo, false),
        new("1.1", "Activo Corriente", 2, TipoCuentaContable.Activo, false),
        new("1.1.01", "Efectivo y Equivalentes", 3, TipoCuentaContable.Activo, false),
        new("1.1.01.01", "Caja General", 4, TipoCuentaContable.Activo, true),
        new("1.1.02", "Cuentas por Cobrar Tributarias", 3, TipoCuentaContable.Activo, false),
        new("1.1.02.01", "Credito Tributario IVA Compras", 4, TipoCuentaContable.Activo, true),
        new("1.1.04", "Inventarios", 3, TipoCuentaContable.Activo, false),
        new("1.1.04.01", "Inventario de Mercaderias", 4, TipoCuentaContable.Activo, true),
        new("2", "Pasivo", 1, TipoCuentaContable.Pasivo, false),
        new("2.1", "Pasivo Corriente", 2, TipoCuentaContable.Pasivo, false),
        new("2.1.03", "Impuestos por Pagar", 3, TipoCuentaContable.Pasivo, false),
        new("2.1.03.01", "IVA Ventas por Pagar", 4, TipoCuentaContable.Pasivo, true),
        new("3", "Patrimonio", 1, TipoCuentaContable.Patrimonio, false),
        new("3.1", "Capital", 2, TipoCuentaContable.Patrimonio, false),
        new("4", "Ingresos", 1, TipoCuentaContable.Ingreso, false),
        new("4.1", "Ingresos Operacionales", 2, TipoCuentaContable.Ingreso, false),
        new("5", "Gastos", 1, TipoCuentaContable.Gasto, false),
        new("5.1", "Gastos Administrativos", 2, TipoCuentaContable.Gasto, false),
        new("6", "Costos", 1, TipoCuentaContable.Costo, false),
        new("6.1", "Costo de Ventas", 2, TipoCuentaContable.Costo, false)
    ];

    public IReadOnlyCollection<CuentaContableEntity> BuildForEmpresa(Guid empresaId)
    {
        return BaseCuentas
            .Select(current => new CuentaContableEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Nivel = current.Nivel,
                TipoCuenta = current.TipoCuenta,
                EsAceptable = current.EsAceptable,
                SaldoActual = 0m
            })
            .ToArray();
    }

    private sealed record SeedCuenta(string Codigo, string Nombre, int Nivel, TipoCuentaContable TipoCuenta, bool EsAceptable);
}