using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.Ports.In;

public interface IContabilidadUseCase
{
    Task<IReadOnlyCollection<CuentaContableResponse>> GetPlanCuentasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CuentaContableResponse>> GetCuentasAceptablesAsync(CancellationToken cancellationToken = default);
    Task<string> CrearAsientoAsync(CrearAsientoRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AsientoContableResponse>> GetLibroDiarioAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LibroDiarioLineaResponse>> GetLibroDiarioAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
    Task<LibroMayorResponse> GetLibroMayorAsync(Guid cuentaContableId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
    Task<BalanceGeneralResponse> GetBalanceGeneralAsync(CancellationToken cancellationToken = default);
    Task<EstadoResultadosResponse> GetEstadoResultadosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PeriodoContableResponse>> GetPeriodosAsync(int anio, CancellationToken cancellationToken = default);
    Task<PeriodoContableResponse> CerrarPeriodoFiscalAsync(CerrarPeriodoFiscalRequest request, CancellationToken cancellationToken = default);
    Task<AjusteInventarioContableResponse> AjustarInventarioContableAsync(bool generarAsiento, CancellationToken cancellationToken = default);
}
