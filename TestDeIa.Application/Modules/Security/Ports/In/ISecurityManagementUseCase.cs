using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Application.Modules.Security.Ports.In;

public interface ISecurityManagementUseCase
{
    Task<IReadOnlyCollection<SecurityUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<SecurityUserResponse>> GetUsersPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SecurityRoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<SecurityAuditLogResponse>> GetAuditLogsPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<SecurityUserResponse> CreateUserAsync(SecurityUserRequest request, CancellationToken cancellationToken = default);

    Task<SecurityUserResponse?> UpdateUserAsync(Guid id, SecurityUserRequest request, CancellationToken cancellationToken = default);

    Task<SecurityUserResponse?> UpdatePerfilAsync(Guid id, UpdateUserPerfilRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<SecurityUserResponse?> UpdateEstadoAsync(Guid id, UpdateUserEstadoRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<SecurityUserResponse?> ResetPasswordAsync(Guid id, ResetPasswordRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<SecurityUserResponse?> UnlockUserAsync(Guid id, string? ipAddress, CancellationToken cancellationToken = default);
}
