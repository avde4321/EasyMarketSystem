using TestDeIa.Domain.Modules.Contabilidad.Entities;
using TestDeIa.Domain.Modules.Contabilidad.Enums;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.UseCases;

public sealed class ReportesFinancierosService
{
    private const string RootKey = "__root__";

    public BalanceGeneralResponse GenerarBalanceGeneral(IReadOnlyCollection<CuentaContable> cuentas)
    {
        var activos = BuildTree(cuentas, TipoCuentaContable.Activo);
        var pasivos = BuildTree(cuentas, TipoCuentaContable.Pasivo);
        var patrimonio = BuildTree(cuentas, TipoCuentaContable.Patrimonio);
        var totalActivo = SumSaldo(activos);
        var totalPasivo = SumSaldo(pasivos);
        var totalPatrimonio = SumSaldo(patrimonio);
        var totalPasivoPatrimonio = totalPasivo + totalPatrimonio;

        return new BalanceGeneralResponse
        {
            Activos = activos,
            Pasivos = pasivos,
            Patrimonio = patrimonio,
            TotalActivo = totalActivo,
            TotalPasivo = totalPasivo,
            TotalPatrimonio = totalPatrimonio,
            TotalPasivoPatrimonio = totalPasivoPatrimonio,
            DiferenciaEcuacion = totalActivo - totalPasivoPatrimonio
        };
    }

    public EstadoResultadosResponse GenerarEstadoResultados(IReadOnlyCollection<CuentaContable> cuentas)
    {
        var ingresos = BuildTree(cuentas, TipoCuentaContable.Ingreso);
        var costos = BuildTree(cuentas, TipoCuentaContable.Costo);
        var gastos = BuildTree(cuentas, TipoCuentaContable.Gasto);
        var totalIngresos = SumSaldo(ingresos);
        var totalCostos = SumSaldo(costos);
        var totalGastos = SumSaldo(gastos);

        return new EstadoResultadosResponse
        {
            Ingresos = ingresos,
            Costos = costos,
            Gastos = gastos,
            TotalIngresos = totalIngresos,
            TotalCostos = totalCostos,
            TotalGastos = totalGastos,
            UtilidadOPerdida = totalIngresos - totalCostos - totalGastos
        };
    }

    private static IReadOnlyCollection<EstadoFinancieroCuentaResponse> BuildTree(
        IReadOnlyCollection<CuentaContable> cuentas,
        TipoCuentaContable tipoCuenta)
    {
        var filtered = cuentas
            .Where(current => current.TipoCuenta == tipoCuenta)
            .OrderBy(current => current.Codigo)
            .ToArray();

        var byParent = filtered
            .GroupBy(current => GetParentCode(current.Codigo))
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        return BuildNodes(RootKey, byParent);
    }

    private static IReadOnlyCollection<EstadoFinancieroCuentaResponse> BuildNodes(
        string parentCode,
        IReadOnlyDictionary<string, CuentaContable[]> byParent)
    {
        if (!byParent.TryGetValue(parentCode, out var children))
        {
            return [];
        }

        return children
            .Select(current =>
            {
                var childNodes = BuildNodes(current.Codigo, byParent);
                return new EstadoFinancieroCuentaResponse
                {
                    Id = current.Id,
                    Codigo = current.Codigo,
                    Nombre = current.Nombre,
                    Nivel = current.Nivel,
                    TipoCuenta = current.TipoCuenta.ToString(),
                    Saldo = childNodes.Count > 0 ? SumSaldo(childNodes) : current.SaldoActual,
                    Hijos = childNodes
                };
            })
            .ToArray();
    }

    private static decimal SumSaldo(IEnumerable<EstadoFinancieroCuentaResponse> cuentas) =>
        cuentas.Sum(current => current.Saldo);

    private static string GetParentCode(string codigo)
    {
        var lastSeparator = codigo.LastIndexOf('.');
        return lastSeparator > 0 ? codigo[..lastSeparator] : RootKey;
    }
}
