using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Application.Modules.Contabilidad.UseCases;

public sealed class AjusteInventarioContableService(IContabilidadRepository contabilidadRepository)
{
    public Task<AjusteInventarioContableResponse> ReconciliarAsync(
        bool generarAsiento,
        CancellationToken cancellationToken = default) =>
        contabilidadRepository.AjustarInventarioContableAsync(generarAsiento, cancellationToken);
}
