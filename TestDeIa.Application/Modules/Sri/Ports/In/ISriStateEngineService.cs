using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Shared.Responses.Sri;

namespace TestDeIa.Application.Modules.Sri.Ports.In;

public interface ISriStateEngineService
{
    Task<SriTransitionResult> ApplyTransitionAsync(
        SriTransitionRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SriCatalogoErrorResponse>> GetErroresAsync(
        string? term = null,
        CancellationToken cancellationToken = default);

    Task<SriCatalogoErrorResponse?> GetErrorByCodigoAsync(
        string codigoSri,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SriEstadoComprobanteResponse>> GetEstadosAsync(
        CancellationToken cancellationToken = default);
}
