using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Application.Modules.Tesoreria.Ports.In;

public interface IBancoParserTestService
{
    Task<DiagnosticTestResultResponse> ValidarFormatosMultiBancoAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> ValidarParsingArchivoGrandeAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default);
}
