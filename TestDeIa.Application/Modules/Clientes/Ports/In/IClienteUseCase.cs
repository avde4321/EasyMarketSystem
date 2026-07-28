using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Clientes;

namespace TestDeIa.Application.Modules.Clientes.Ports.In;

public interface IClienteUseCase
{
    Task<IReadOnlyCollection<ClienteResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<ClienteResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<ClienteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClienteResponse> CreateAsync(ClienteRequest request, CancellationToken cancellationToken = default);

    Task<ClienteResponse?> UpdateAsync(Guid id, ClienteRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
