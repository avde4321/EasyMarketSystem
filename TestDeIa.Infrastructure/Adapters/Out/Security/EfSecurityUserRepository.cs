using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Adapters.Out.Security;

public sealed class EfSecurityUserRepository : ISecurityUserRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfSecurityUserRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<SecurityUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim().ToUpperInvariant();
        var user = await BaseQuery(ignoreQueryFilters: true)
            .FirstOrDefaultAsync(current => current.NormalizedUserName == normalizedUserName, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<IReadOnlyCollection<SecurityUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await BaseQuery()
            .OrderBy(current => current.DisplayName)
            .ToListAsync(cancellationToken);

        return users.Select(MapUser).ToArray();
    }

    public async Task<PagedResultResponse<SecurityUser>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term?.Trim();
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(current =>
                current.UserName.Contains(normalizedTerm) ||
                current.DisplayName.Contains(normalizedTerm) ||
                current.Email.Contains(normalizedTerm) ||
                current.Persona.Identificacion.Contains(normalizedTerm) ||
                current.Persona.RazonSocialONombresCompletos.Contains(normalizedTerm) ||
                (current.Persona.NombreComercial != null && current.Persona.NombreComercial.Contains(normalizedTerm)) ||
                current.UserRoles.Any(userRole => userRole.Role.Name.Contains(normalizedTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(current => current.DisplayName)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<SecurityUser>
        {
            Items = users.Select(MapUser).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
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
            .Include(current => current.RolePermissions)
            .OrderBy(current => current.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(role => new SecurityRole(
            role.Id,
            role.Name,
            role.IsActive,
            role.RolePermissions.Select(current => current.PermisoId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray())).ToArray();
    }

    public async Task<PagedResultResponse<SecurityAuditLogEntry>> GetAuditLogsPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term?.Trim();
        var query = dbContext.SecurityAuditLogs
            .AsNoTracking()
            .GroupJoin(
                dbContext.SecurityUsers.IgnoreQueryFilters().AsNoTracking(),
                audit => audit.UsuarioId,
                user => user.Id,
                (audit, users) => new { audit, users })
            .SelectMany(
                current => current.users.DefaultIfEmpty(),
                (current, user) => new { current.audit, user });

        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(current =>
                current.audit.TipoEvento.ToString().Contains(normalizedTerm) ||
                (current.audit.Detalles != null && current.audit.Detalles.Contains(normalizedTerm)) ||
                (current.audit.DireccionIP != null && current.audit.DireccionIP.Contains(normalizedTerm)) ||
                (current.user != null && (
                    current.user.UserName.Contains(normalizedTerm) ||
                    current.user.DisplayName.Contains(normalizedTerm) ||
                    current.user.Email.Contains(normalizedTerm))));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(current => current.audit.FechaEvento)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<SecurityAuditLogEntry>
        {
            Items = items.Select(current => new SecurityAuditLogEntry(
                current.audit.Id,
                current.audit.EmpresaId,
                current.audit.UsuarioId,
                current.user?.UserName ?? "Sistema",
                current.user?.DisplayName ?? "Proceso interno",
                current.audit.FechaEvento,
                current.audit.TipoEvento,
                current.audit.DireccionIP,
                current.audit.Detalles)).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
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
            EmpresaId = ResolveEmpresaId(user.EmpresaId),
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

        entity.EmpresasAcceso.Add(new Persistence.Entities.SecurityUserEmpresaEntity
        {
            SecurityUserId = entity.Id,
            EmpresaId = entity.EmpresaId,
            IsDefault = true,
            CreatedAt = user.CreatedAt
        });

        dbContext.SecurityUsers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? user;
    }

    public async Task<SecurityUser?> UpdateAsync(SecurityUser user, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecurityUsers
            .Include(current => current.UserRoles)
            .Include(current => current.EmpresasAcceso)
            .FirstOrDefaultAsync(current => current.Id == user.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var roleEntities = await ResolveRolesAsync(user.Roles, cancellationToken);

        entity.PersonaId = user.PersonaId;
        entity.EmpresaId = ResolveEmpresaId(user.EmpresaId);
        entity.UserName = user.UserName;
        entity.NormalizedUserName = user.UserName.ToUpperInvariant();
        entity.DisplayName = user.DisplayName;
        entity.Email = user.Email;
        entity.NormalizedEmail = user.Email.ToUpperInvariant();
        entity.PasswordHash = user.PasswordHash;
        entity.IsActive = user.IsActive;
        entity.BloqueadoManualmente = user.BloqueadoManualmente;
        entity.TokensInvalidosDesde = user.TokensInvalidosDesde;
        entity.UserRoles.Clear();
        entity.EmpresasAcceso.Clear();

        foreach (var role in roleEntities)
        {
            entity.UserRoles.Add(new Persistence.Entities.SecurityUserRoleEntity
            {
                UserId = entity.Id,
                RoleId = role.Id
            });
        }

        var accessCompanies = user.EmpresasAcceso.Count == 0
            ? [new UserEmpresaAcceso(entity.EmpresaId, string.Empty, null, string.Empty, true, true)]
            : user.EmpresasAcceso;

        foreach (var empresa in accessCompanies)
        {
            entity.EmpresasAcceso.Add(new Persistence.Entities.SecurityUserEmpresaEntity
            {
                SecurityUserId = entity.Id,
                EmpresaId = empresa.EmpresaId,
                IsDefault = empresa.IsDefault,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<SecurityUser?> UpdatePerfilAsync(Guid userId, IReadOnlyCollection<string> roles, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var entity = await LoadUserForMutationAsync(userId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var roleEntities = await ResolveRolesAsync(roles, cancellationToken);
        entity.UserRoles.Clear();

        foreach (var role in roleEntities)
        {
            entity.UserRoles.Add(new SecurityUserRoleEntity
            {
                UserId = entity.Id,
                RoleId = role.Id
            });
        }

        dbContext.SecurityAuditLogs.Add(BuildAudit(
            entity.EmpresaId,
            entity.Id,
            SecurityAuditEventType.CambioPerfil,
            ipAddress,
            $"Perfil actualizado. Roles activos: {string.Join(", ", roleEntities.Select(current => current.Name))}."));

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<SecurityUser?> UpdateEstadoAsync(Guid userId, string estado, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var entity = await LoadUserForMutationAsync(userId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var updatedAt = DateTimeOffset.UtcNow;
        string details;

        switch (SecurityUserEstados.Normalize(estado))
        {
            case SecurityUserEstados.Activo:
                entity.IsActive = true;
                entity.BloqueadoManualmente = false;
                entity.BloqueadoHasta = null;
                entity.IntentosFallidos = 0;
                details = "Usuario reactivado y habilitado para volver a ingresar.";
                break;

            case SecurityUserEstados.Inactivo:
                entity.IsActive = false;
                entity.BloqueadoManualmente = false;
                entity.BloqueadoHasta = null;
                entity.IntentosFallidos = 0;
                entity.TokensInvalidosDesde = updatedAt;
                details = "Usuario marcado como inactivo. Se revocaron las sesiones activas.";
                break;

            case SecurityUserEstados.Bloqueado:
                entity.IsActive = true;
                entity.BloqueadoManualmente = true;
                entity.BloqueadoHasta = null;
                entity.TokensInvalidosDesde = updatedAt;
                details = "Usuario bloqueado manualmente. Se revocaron las sesiones activas.";
                break;

            default:
                throw new InvalidOperationException("El estado solicitado para el usuario no es valido.");
        }

        dbContext.SecurityAuditLogs.Add(BuildAudit(
            entity.EmpresaId,
            entity.Id,
            SecurityAuditEventType.CambioEstado,
            ipAddress,
            details));

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task RecordSuccessfulLoginAsync(Guid userId, string? ipAddress, string? passwordHashToPersist = null, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro el usuario de seguridad.");

        entity.IntentosFallidos = 0;
        entity.BloqueadoHasta = null;
        entity.UltimoAcceso = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(passwordHashToPersist))
        {
            entity.PasswordHash = passwordHashToPersist;
        }

        dbContext.SecurityAuditLogs.Add(BuildAudit(
            entity.EmpresaId,
            entity.Id,
            SecurityAuditEventType.LoginExitoso,
            ipAddress,
            "Ingreso exitoso al sistema."));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RecordFailedLoginAsync(string userName, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim().ToUpperInvariant();
        var entity = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.NormalizedUserName == normalizedUserName, cancellationToken);

        if (entity is null)
        {
            dbContext.SecurityAuditLogs.Add(BuildAudit(
                Guid.Empty,
                Guid.Empty,
                SecurityAuditEventType.LoginFallido,
                ipAddress,
                $"Intento fallido para usuario no registrado: {userName.Trim()}."));

            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        entity.IntentosFallidos += 1;
        var blocked = false;
        var details = "Contrasena incorrecta.";

        if (entity.IntentosFallidos >= 5)
        {
            entity.BloqueadoHasta = DateTimeOffset.UtcNow.AddMinutes(15);
            blocked = true;
            details = "Usuario bloqueado temporalmente por superar el maximo de intentos.";
        }

        dbContext.SecurityAuditLogs.Add(BuildAudit(
            entity.EmpresaId,
            entity.Id,
            SecurityAuditEventType.LoginFallido,
            ipAddress,
            details));

        if (blocked)
        {
            dbContext.SecurityAuditLogs.Add(BuildAudit(
                entity.EmpresaId,
                entity.Id,
                SecurityAuditEventType.BloqueoUsuario,
                ipAddress,
                "Bloqueo automatico por 5 intentos fallidos consecutivos."));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return blocked;
    }

    public async Task<SecurityUser?> ResetPasswordAsync(Guid userId, string temporaryPasswordHash, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var entity = await LoadUserForMutationAsync(userId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.PasswordHash = temporaryPasswordHash;
        entity.IntentosFallidos = 0;
        entity.BloqueadoHasta = null;
        entity.BloqueadoManualmente = false;
        entity.TokensInvalidosDesde = DateTimeOffset.UtcNow;

        dbContext.SecurityAuditLogs.Add(BuildAudit(
            entity.EmpresaId,
            entity.Id,
            SecurityAuditEventType.ResetClaveAdministrative,
            ipAddress,
            "Clave temporal reasignada por un administrador."));

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<SecurityUser?> UnlockUserAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var entity = await LoadUserForMutationAsync(userId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.IntentosFallidos = 0;
        entity.BloqueadoHasta = null;
        entity.BloqueadoManualmente = false;

        dbContext.SecurityAuditLogs.Add(BuildAudit(
            entity.EmpresaId,
            entity.Id,
            SecurityAuditEventType.DesbloqueoUsuario,
            ipAddress,
            "Usuario desbloqueado manualmente por un administrador."));

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    private IQueryable<Persistence.Entities.SecurityUserEntity> BaseQuery(bool ignoreQueryFilters = false)
    {
        var query = dbContext.SecurityUsers
            .AsNoTracking()
            .Include(current => current.Persona)
            .ThenInclude(persona => persona.Cliente)
            .Include(current => current.Persona)
            .ThenInclude(persona => persona.Empleado)
            .Include(current => current.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permiso)
            .Include(current => current.EmpresasAcceso)
            .ThenInclude(link => link.Empresa);

        return ignoreQueryFilters ? query.IgnoreQueryFilters() : query;
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

    private async Task<SecurityUserEntity?> LoadUserForMutationAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .Include(current => current.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permiso)
            .Include(current => current.EmpresasAcceso)
            .ThenInclude(link => link.Empresa)
            .Include(current => current.Persona)
            .ThenInclude(persona => persona.Cliente)
            .Include(current => current.Persona)
            .ThenInclude(persona => persona.Empleado)
            .FirstOrDefaultAsync(current => current.Id == userId, cancellationToken);
    }

    private static SecurityAuditLogEntity BuildAudit(
        Guid empresaId,
        Guid usuarioId,
        SecurityAuditEventType tipoEvento,
        string? ipAddress,
        string? details)
    {
        return new SecurityAuditLogEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            FechaEvento = DateTimeOffset.UtcNow,
            TipoEvento = tipoEvento,
            DireccionIP = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim(),
            Detalles = details
        };
    }

    private static SecurityUser MapUser(Persistence.Entities.SecurityUserEntity user)
    {
        var roles = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .Select(userRole => userRole.Role.Name)
            .ToArray();
        var permissions = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .SelectMany(userRole => userRole.Role.RolePermissions.Select(current => current.PermisoId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
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
            user.EmpresaId,
            user.PersonaId,
            user.Persona.Identificacion,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.PasswordHash,
            roles,
            permissions,
            user.EmpresasAcceso
                .OrderByDescending(link => link.IsDefault)
                .ThenBy(link => link.Empresa.RazonSocial)
                .Select(link => new UserEmpresaAcceso(
                    link.EmpresaId,
                    link.Empresa.RazonSocial,
                    link.Empresa.NombreComercial,
                    link.Empresa.Ruc,
                    link.Empresa.IsActive,
                    link.IsDefault))
                .ToArray(),
            personaRoles.ToArray(),
            user.IsActive,
            user.CreatedAt,
            user.IntentosFallidos,
            user.BloqueadoHasta,
            user.UltimoAcceso,
            user.BloqueadoManualmente,
            user.TokensInvalidosDesde);
    }

    private Guid ResolveEmpresaId(Guid empresaId)
    {
        if (empresaId != Guid.Empty)
        {
            return empresaId;
        }

        if (tenantContextAccessor.EmpresaId.HasValue)
        {
            return tenantContextAccessor.EmpresaId.Value;
        }

        throw new InvalidOperationException("No existe una empresa activa para registrar el usuario.");
    }
}


