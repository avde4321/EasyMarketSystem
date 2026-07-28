using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.Ports.In;

public interface IProveedorUseCase
{
    Task<IReadOnlyCollection<ProveedorResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResultResponse<ProveedorResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);
    Task<ProveedorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProveedorResponse> CreateAsync(ProveedorRequest request, CancellationToken cancellationToken = default);
    Task<ProveedorResponse?> UpdateAsync(Guid id, ProveedorRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
