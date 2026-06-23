using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Application.Modules.Security.Ports.In;

public interface ISecurityManagementUseCase
{
    Task<IReadOnlyCollection<SecurityUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SecurityRoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);

    Task<SecurityUserResponse> CreateUserAsync(SecurityUserRequest request, CancellationToken cancellationToken = default);

    Task<SecurityUserResponse?> UpdateUserAsync(Guid id, SecurityUserRequest request, CancellationToken cancellationToken = default);
}
