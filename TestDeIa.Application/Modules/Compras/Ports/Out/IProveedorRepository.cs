using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Application.Modules.Compras.Ports.Out;

public interface IProveedorRepository
{
    Task<IReadOnlyCollection<Proveedor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResultResponse<Proveedor>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);
    Task<Proveedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Proveedor?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default);
    Task<Proveedor> CreateAsync(Proveedor proveedor, CancellationToken cancellationToken = default);
    Task<Proveedor?> UpdateAsync(Proveedor proveedor, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
