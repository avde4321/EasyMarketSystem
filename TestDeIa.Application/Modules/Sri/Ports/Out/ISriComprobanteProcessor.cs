using TestDeIa.Application.Modules.Sri.Models;

namespace TestDeIa.Application.Modules.Sri.Ports.Out;

public interface ISriComprobanteProcessor
{
    Task<SriOutboxProcessingResult> ProcessAsync(
        Guid colaProcesamientoId,
        string workerId,
        CancellationToken cancellationToken = default);
}
