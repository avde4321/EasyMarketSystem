using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.Ports.In;

public interface IContabilidadUseCase
{
    Task<IReadOnlyCollection<CuentaContableResponse>> GetPlanCuentasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CuentaContableResponse>> GetCuentasAceptablesAsync(CancellationToken cancellationToken = default);
    Task<string> CrearAsientoAsync(CrearAsientoRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AsientoContableResponse>> GetLibroDiarioAsync(CancellationToken cancellationToken = default);
}
