using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Application.Modules.Tesoreria.Ports.In;

public interface IConciliacionTestService
{
    Task<DiagnosticTestResultResponse> ValidarMatchingExactoMontoYReferenciaAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> ValidarMatchingPorRangoDeFechasAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> ValidarManejoComisionesYBancosAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> EvitarDuplicadosExtractoTestAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default);
}
