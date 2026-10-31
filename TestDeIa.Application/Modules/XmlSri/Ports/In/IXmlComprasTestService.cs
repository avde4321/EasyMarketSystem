using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Application.Modules.XmlSri.Ports.In;

public interface IXmlComprasTestService
{
    Task<DiagnosticTestResultResponse> ValidarEstructuraXmlSriTestAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> EvitarDuplicadosXmlTestAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticTestResultResponse> ConversionACompraEInventarioTestAsync(CancellationToken cancellationToken = default);

    Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default);
}
