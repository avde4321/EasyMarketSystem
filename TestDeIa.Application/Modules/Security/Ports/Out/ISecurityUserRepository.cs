using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Application.Modules.Security.Ports.Out;

public interface ISecurityUserRepository
{
    Task<SecurityUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SecurityUser>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<SecurityUser>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<SecurityUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SecurityRole>> GetRolesAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserNameAsync(string userName, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<SecurityUser?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<SecurityUser> CreateAsync(SecurityUser user, CancellationToken cancellationToken = default);

    Task<SecurityUser?> UpdateAsync(SecurityUser user, CancellationToken cancellationToken = default);
}
