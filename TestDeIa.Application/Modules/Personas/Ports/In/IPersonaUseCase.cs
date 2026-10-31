using TestDeIa.Shared.Requests.Personas;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Personas;

namespace TestDeIa.Application.Modules.Personas.Ports.In;

public interface IPersonaUseCase
{
    Task<IReadOnlyCollection<PersonaResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<PersonaResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<PersonaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PersonaResponse?> FindByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);

    Task<PersonaResponse> CreateAsync(PersonaRequest request, CancellationToken cancellationToken = default);

    Task<PersonaResponse?> UpdateAsync(Guid id, PersonaRequest request, CancellationToken cancellationToken = default);
}
