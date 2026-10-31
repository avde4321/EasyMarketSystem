using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.UseCases;

public sealed class CerrarPeriodoFiscalCommandHandler(IContabilidadRepository contabilidadRepository)
{
    public Task<PeriodoContableResponse> HandleAsync(
        CerrarPeriodoFiscalRequest request,
        CancellationToken cancellationToken = default) =>
        contabilidadRepository.CerrarPeriodoFiscalAsync(request, cancellationToken);
}
