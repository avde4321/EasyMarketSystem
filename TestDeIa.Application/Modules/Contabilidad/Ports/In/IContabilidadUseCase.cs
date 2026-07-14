using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.Ports.In;

public interface IContabilidadUseCase
{
    Task<IReadOnlyCollection<CuentaContableResponse>> GetPlanCuentasAsync(CancellationToken cancellationToken = default);
}