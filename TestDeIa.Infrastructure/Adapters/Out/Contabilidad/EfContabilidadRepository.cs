using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Domain.Modules.Contabilidad.Entities;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Contabilidad;

public sealed class EfContabilidadRepository(TestDeIaDbContext dbContext) : IContabilidadRepository
{
    public async Task<IReadOnlyCollection<CuentaContable>> GetPlanCuentasAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await dbContext.CuentasContables
            .AsNoTracking()
            .OrderBy(current => current.Codigo)
            .ToListAsync(cancellationToken);

        return cuentas
            .Select(current => new CuentaContable(
                current.Id,
                current.EmpresaId,
                current.Codigo,
                current.Nombre,
                current.Nivel,
                current.TipoCuenta,
                current.EsAceptable,
                current.SaldoActual))
            .ToArray();
    }
}