using TestDeIa.Shared.Requests.Tesoreria;
using TestDeIa.Shared.Responses.Tesoreria;

namespace TestDeIa.Application.Modules.Tesoreria.Ports.In;

public interface IConciliacionService
{
    Task<ConciliacionResultadoDto> EjecutarConciliacionAutomaticaAsync(Guid cuentaId, CancellationToken cancellationToken = default);

    Task<ConciliacionMatchDto> ConciliarManualAsync(ConciliacionManualDto request, CancellationToken cancellationToken = default);
}
