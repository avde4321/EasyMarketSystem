using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Security;

public sealed class EfSecurityUserRepository : ISecurityUserRepository
{
    private readonly TestDeIaDbContext dbContext;

    public EfSecurityUserRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<SecurityUser?> FindByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim().ToUpperInvariant();
        var user = await dbContext.SecurityUsers
            .AsNoTracking()
            .Include(current => current.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                current => current.NormalizedUserName == normalizedUserName,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .Select(userRole => userRole.Role.Name)
            .ToArray();

        return new SecurityUser(
            user.Id,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.PasswordHash,
            roles,
            user.IsActive);
    }
}
