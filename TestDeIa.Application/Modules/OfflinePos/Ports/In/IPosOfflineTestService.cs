using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Application.Modules.OfflinePos.Ports.In;

public interface IPosOfflineTestService
{
    Task<DiagnosticTestResultResponse> SincronizacionMasivaEnLoteTestAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> ConflictoStockTestAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> IdempotenciaTestAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default);
}
