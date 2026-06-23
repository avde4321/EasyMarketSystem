using TestDeIa.Domain.Modules.Security.Entities;

namespace TestDeIa.Application.Modules.Security.Ports.Out;

public interface ISecurityUserRepository
{
    Task<SecurityUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);
}
