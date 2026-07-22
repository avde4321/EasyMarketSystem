using TestDeIa.Domain.Modules.Contabilidad.Entities;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.Ports.Out;

public interface IContabilidadRepository
{
    Task<IReadOnlyCollection<CuentaContable>> GetPlanCuentasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CuentaContable>> GetCuentasAceptablesAsync(CancellationToken cancellationToken = default);
    Task<string> CrearAsientoAsync(CrearAsientoRequest request, CancellationToken cancellationToken = default);
    Task<string> GenerarAsientoDesdeOrigenAsync(Guid transaccionId, string moduloOrigen, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AsientoContableResponse>> GetLibroDiarioAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LibroDiarioLineaResponse>> GetLibroDiarioAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
    Task<LibroMayorResponse> GetLibroMayorAsync(Guid cuentaContableId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CuentaContable>> GetCuentasParaEstadosFinancierosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PeriodoContableResponse>> GetPeriodosAsync(int anio, CancellationToken cancellationToken = default);
    Task<PeriodoContableResponse> CerrarPeriodoFiscalAsync(CerrarPeriodoFiscalRequest request, CancellationToken cancellationToken = default);
    Task<AjusteInventarioContableResponse> AjustarInventarioContableAsync(bool generarAsiento, CancellationToken cancellationToken = default);
}
