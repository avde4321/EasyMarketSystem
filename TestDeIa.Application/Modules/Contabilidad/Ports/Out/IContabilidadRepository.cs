using TestDeIa.Domain.Modules.Contabilidad.Entities;

namespace TestDeIa.Application.Modules.Contabilidad.Ports.Out;

public interface IContabilidadRepository
{
    Task<IReadOnlyCollection<CuentaContable>> GetPlanCuentasAsync(CancellationToken cancellationToken = default);
}