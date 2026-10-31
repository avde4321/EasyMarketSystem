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

    Task<PagedResultResponse<SecurityAuditLogEntry>> GetAuditLogsPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserNameAsync(string userName, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<SecurityUser?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<SecurityUser> CreateAsync(SecurityUser user, CancellationToken cancellationToken = default);

    Task<SecurityUser?> UpdateAsync(SecurityUser user, CancellationToken cancellationToken = default);

    Task<SecurityUser?> UpdatePerfilAsync(Guid userId, IReadOnlyCollection<string> roles, string? ipAddress, CancellationToken cancellationToken = default);

    Task<SecurityUser?> UpdateEstadoAsync(Guid userId, string estado, string? ipAddress, CancellationToken cancellationToken = default);

    Task RecordSuccessfulLoginAsync(Guid userId, string? ipAddress, string? passwordHashToPersist = null, CancellationToken cancellationToken = default);

    Task<bool> RecordFailedLoginAsync(string userName, string? ipAddress, CancellationToken cancellationToken = default);

    Task<SecurityUser?> ResetPasswordAsync(Guid userId, string temporaryPasswordHash, string? ipAddress, CancellationToken cancellationToken = default);

    Task<SecurityUser?> UnlockUserAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default);
}
