using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Application.Modules.Reporteria.Ports.In;

public interface IConsolidacionContableService
{
    Task<ConsolidacionContableDiariaResponse> ObtenerResumenDiarioAsync(Guid empresaId, DateTime fecha, CancellationToken cancellationToken = default);
}
