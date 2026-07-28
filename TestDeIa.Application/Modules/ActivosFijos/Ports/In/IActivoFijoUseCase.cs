using TestDeIa.Shared.ActivosFijos;
using TestDeIa.Shared.Requests.ActivosFijos;
using TestDeIa.Shared.Responses.ActivosFijos;

namespace TestDeIa.Application.Modules.ActivosFijos.Ports.In;

public interface IActivoFijoUseCase
{
    Task<IReadOnlyCollection<ActivoFijoResponse>> GetAsync(
        CategoriaSriActivoFijo? categoria,
        string? custodio,
        EstadoActivoFijo? estado,
        CancellationToken cancellationToken = default);

    Task<ActivoFijoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActivoFijoResponse> CreateAsync(ActivoFijoRequest request, CancellationToken cancellationToken = default);

    Task<ActivoFijoResponse?> UpdateAsync(Guid id, ActivoFijoRequest request, CancellationToken cancellationToken = default);
}
