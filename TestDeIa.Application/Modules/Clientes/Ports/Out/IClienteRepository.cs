using TestDeIa.Domain.Modules.Clientes.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Application.Modules.Clientes.Ports.Out;

public interface IClienteRepository
{
    Task<IReadOnlyCollection<Cliente>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<Cliente>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Cliente?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<Cliente> CreateAsync(Cliente cliente, CancellationToken cancellationToken = default);

    Task<Cliente?> UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
