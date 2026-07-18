using TestDeIa.Application.Modules.Contabilidad.Exceptions;
using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Domain.Modules.Contabilidad.Entities;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.UseCases;

public sealed class ContabilidadUseCase(IContabilidadRepository contabilidadRepository) : IContabilidadUseCase
{
    private const string RootKey = "__root__";

    public async Task<IReadOnlyCollection<CuentaContableResponse>> GetPlanCuentasAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await contabilidadRepository.GetPlanCuentasAsync(cancellationToken);

        var byParent = cuentas
            .GroupBy(current => GetParentCode(current.Codigo))
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        return BuildNodes(RootKey, byParent);
    }

    public async Task<IReadOnlyCollection<CuentaContableResponse>> GetCuentasAceptablesAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await contabilidadRepository.GetCuentasAceptablesAsync(cancellationToken);
        return cuentas
            .Select(current => new CuentaContableResponse
            {
                Id = current.Id,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Nivel = current.Nivel,
                TipoCuenta = current.TipoCuenta.ToString(),
                EsAceptable = current.EsAceptable,
                SaldoActual = current.SaldoActual,
                Hijos = []
            })
            .ToArray();
    }

    public async Task<string> CrearAsientoAsync(CrearAsientoRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Detalles.Count == 0)
        {
            throw new InvalidOperationException("El asiento debe incluir al menos una línea.");
        }

        var totalDebe = decimal.Round(request.Detalles.Sum(current => current.Debe), 2, MidpointRounding.AwayFromZero);
        var totalHaber = decimal.Round(request.Detalles.Sum(current => current.Haber), 2, MidpointRounding.AwayFromZero);
        var diferencia = decimal.Round(totalDebe - totalHaber, 2, MidpointRounding.AwayFromZero);

        if (diferencia != 0m)
        {
            throw new AsientoDescuadradoException("El asiento no cumple partida doble. La suma del Debe debe coincidir exactamente con la suma del Haber.");
        }

        return await contabilidadRepository.CrearAsientoAsync(request, cancellationToken);
    }

    public Task<IReadOnlyCollection<AsientoContableResponse>> GetLibroDiarioAsync(CancellationToken cancellationToken = default) =>
        contabilidadRepository.GetLibroDiarioAsync(cancellationToken);

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
