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

    public async Task<SecurityUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim().ToUpperInvariant();
        var user = await BaseQuery().FirstOrDefaultAsync(current => current.NormalizedUserName == normalizedUserName, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<IReadOnlyCollection<SecurityUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await BaseQuery()
            .OrderBy(current => current.DisplayName)
            .ToListAsync(cancellationToken);

        return users.Select(MapUser).ToArray();
    }

    public async Task<SecurityUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await BaseQuery().FirstOrDefaultAsync(current => current.Id == id, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<IReadOnlyCollection<SecurityRole>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.SecurityRoles
            .AsNoTracking()
            .OrderBy(current => current.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(role => new SecurityRole(role.Id, role.Name, role.IsActive)).ToArray();
    }

    public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim().ToUpperInvariant();
        return dbContext.SecurityUsers.AnyAsync(
            current => current.NormalizedUserName == normalizedUserName &&
                       (!excludedId.HasValue || current.Id != excludedId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        return dbContext.SecurityUsers.AnyAsync(
            current => current.PersonaId == personaId &&
                       (!excludedId.HasValue || current.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task<SecurityUser?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var user = await BaseQuery()
            .FirstOrDefaultAsync(
                current => current.PersonaId == personaId && (!excludedId.HasValue || current.Id != excludedId.Value),
                cancellationToken);

        return user is null ? null : MapUser(user);
    }

    public async Task<SecurityUser> CreateAsync(SecurityUser user, CancellationToken cancellationToken = default)
    {
        var roleEntities = await ResolveRolesAsync(user.Roles, cancellationToken);

        var entity = new Persistence.Entities.SecurityUserEntity
        {
            Id = user.Id,
            PersonaId = user.PersonaId,
            UserName = user.UserName,
            NormalizedUserName = user.UserName.ToUpperInvariant(),
            DisplayName = user.DisplayName,
            Email = user.Email,
            NormalizedEmail = user.Email.ToUpperInvariant(),
            PasswordHash = user.PasswordHash,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        foreach (var role in roleEntities)
        {
            entity.UserRoles.Add(new Persistence.Entities.SecurityUserRoleEntity
            {
                UserId = entity.Id,
                RoleId = role.Id
            });
        }

        dbContext.SecurityUsers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? user;
    }

    public async Task<SecurityUser?> UpdateAsync(SecurityUser user, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecurityUsers
            .Include(current => current.UserRoles)
            .FirstOrDefaultAsync(current => current.Id == user.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var roleEntities = await ResolveRolesAsync(user.Roles, cancellationToken);

        entity.PersonaId = user.PersonaId;
        entity.UserName = user.UserName;
        entity.NormalizedUserName = user.UserName.ToUpperInvariant();
        entity.DisplayName = user.DisplayName;
        entity.Email = user.Email;
        entity.NormalizedEmail = user.Email.ToUpperInvariant();
        entity.PasswordHash = user.PasswordHash;
        entity.IsActive = user.IsActive;
        entity.UserRoles.Clear();

        foreach (var role in roleEntities)
        {
            entity.UserRoles.Add(new Persistence.Entities.SecurityUserRoleEntity
            {
                UserId = entity.Id,
                RoleId = role.Id
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    private IQueryable<Persistence.Entities.SecurityUserEntity> BaseQuery()
    {
        return dbContext.SecurityUsers
            .AsNoTracking()
            .Include(current => current.Persona)
            .ThenInclude(persona => persona.Cliente)
            .Include(current => current.Persona)
            .ThenInclude(persona => persona.Empleado)
            .Include(current => current.UserRoles)
            .ThenInclude(userRole => userRole.Role);
    }

    private async Task<IReadOnlyCollection<Persistence.Entities.SecurityRoleEntity>> ResolveRolesAsync(
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken)
    {
        var normalizedNames = roleNames
            .Select(role => role.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roles = await dbContext.SecurityRoles
            .Where(role => normalizedNames.Contains(role.NormalizedName) && role.IsActive)
            .ToListAsync(cancellationToken);

        if (roles.Count != normalizedNames.Length)
        {
            throw new InvalidOperationException("No se pudieron resolver todos los roles del usuario.");
        }

        return roles;
    }

    private static SecurityUser MapUser(Persistence.Entities.SecurityUserEntity user)
    {
        var roles = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .Select(userRole => userRole.Role.Name)
            .ToArray();

        var personaRoles = new List<string> { "Usuario" };

        if (user.Persona.Cliente is not null)
        {
            personaRoles.Add("Cliente");
        }

        if (user.Persona.Empleado is not null)
        {
            personaRoles.Add("Empleado");
        }

        return new SecurityUser(
            user.Id,
            user.PersonaId,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.PasswordHash,
            roles,
            personaRoles.ToArray(),
            user.IsActive,
            user.CreatedAt);
    }
}
