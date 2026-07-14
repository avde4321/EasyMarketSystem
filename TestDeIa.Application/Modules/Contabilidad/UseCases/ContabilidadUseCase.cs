using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Domain.Modules.Contabilidad.Entities;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.UseCases;

public sealed class ContabilidadUseCase(IContabilidadRepository contabilidadRepository) : IContabilidadUseCase
{
    private const string RootKey = "__root__";

    public async Task<IReadOnlyCollection<CuentaContableResponse>> GetPlanCuentasAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = (await contabilidadRepository.GetPlanCuentasAsync(cancellationToken))
            .OrderBy(current => current.Codigo, StringComparer.Ordinal)
            .ToArray();

        var byParent = cuentas
            .GroupBy(current => GetParentCode(current.Codigo))
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        return BuildNodes(RootKey, byParent);
    }

    private static IReadOnlyCollection<CuentaContableResponse> BuildNodes(
        string parentCode,
        IReadOnlyDictionary<string, CuentaContable[]> byParent)
    {
        if (!byParent.TryGetValue(parentCode, out var children))
        {
            return [];
        }

        return children
            .Select(current => new CuentaContableResponse
            {
                Id = current.Id,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Nivel = current.Nivel,
                TipoCuenta = current.TipoCuenta.ToString(),
                EsAceptable = current.EsAceptable,
                SaldoActual = current.SaldoActual,
                Hijos = BuildNodes(current.Codigo, byParent)
            })
            .ToArray();
    }

    private static string GetParentCode(string codigo)
    {
        var lastSeparator = codigo.LastIndexOf('.');
        return lastSeparator > 0 ? codigo[..lastSeparator] : RootKey;
    }
}