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
    private readonly TestDeIa.Application.Modules.Saas.Ports.In.ISaaSQuotaValidationService quota;

    public EfSecurityUserRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor,
        TestDeIa.Application.Modules.Saas.Ports.In.ISaaSQuotaValidationService? quota = null)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
        this.quota = quota ?? new TestDeIa.Infrastructure.Adapters.Out.Saas.SaaSQuotaValidationService(dbContext);
    }

    /// <summary>
    /// Resuelve identidad para el inicio de sesión, antes de establecer el tenant del JWT.
    /// El bypass es solo para búsqueda exacta por nombre normalizado globalmente único; no habilita
    /// listados ni operaciones de usuario y la autorización de empresa se valida después del login.
    /// </summary>
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
            .Where(x => x.Name != SecurityRoleNames.SuperAdministrador)
            .Include(current => current.RolePermissions)
            .OrderBy(current => current.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(role => new SecurityRole(
            role.Id,
            role.Name,
            role.IsActive,
            role.RolePermissions.Select(current => current.PermisoId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray())).ToArray();
    }

    /// <summary>
    /// Recupera auditoría del tenant actual. Se omite el filtro de usuario para resolver el nombre
    /// asociado, pero el join queda restringido a la empresa del evento y al usuario vinculado a ella.
    /// </summary>
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
                (current, user) => new { current.audit, user })
            .Where(current => current.user == null ||
                current.user.EmpresaId == current.audit.EmpresaId ||
                current.user.EmpresasAcceso.Any(access => access.EmpresaId == current.audit.EmpresaId));

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

    /// <summary>
    /// Verifica unicidad global del identificador de inicio de sesión y retorna solo un booleano.
    /// El bypass es necesario porque el nombre de usuario es único en Identity entre tenants; no se leen ni devuelven datos ajenos.
    /// </summary>
    public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim().ToUpperInvariant();
        return dbContext.SecurityUsers.IgnoreQueryFilters().AsNoTracking().AnyAsync(
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
        await using var tx = dbContext.Database.CurrentTransaction is null ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        var empresaId = ResolveEmpresaId(user.EmpresaId);
        await quota.BloquearCuotaAsync(empresaId, cancellationToken);
        await quota.CanCrearUsuarioAsync(empresaId, cancellationToken);
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
        await SaveCredentialsAsync(cancellationToken);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? user;
    }

    /// <summary>Actualiza solo usuarios visibles dentro del filtro de tenant; las asignaciones adicionales se verifican una a una por empresa.</summary>
    public async Task<SecurityUser?> UpdateAsync(SecurityUser user, CancellationToken cancellationToken = default)
    {
        await EnsureNotProtectedAsync(user.Id, cancellationToken);
        await using var tx = dbContext.Database.CurrentTransaction is null ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        var entity = await dbContext.SecurityUsers
            .Include(current => current.UserRoles)
            .Include(current => current.EmpresasAcceso)
            .FirstOrDefaultAsync(current => current.Id == user.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var existingCompanies = entity.EmpresasAcceso.Select(x => x.EmpresaId).ToHashSet();
        var requestedCompanies = user.EmpresasAcceso.Count == 0 ? new[] { ResolveEmpresaId(user.EmpresaId) }
            : user.EmpresasAcceso.Select(x => x.EmpresaId).Distinct().ToArray();
        foreach (var empresaId in requestedCompanies.Order())
        {
            await quota.BloquearCuotaAsync(empresaId, cancellationToken);
            if (!existingCompanies.Contains(empresaId))
            {
                if (!tenantContextAccessor.IsSystemContext && !await dbContext.SecurityUserEmpresas.IgnoreQueryFilters()
                    .AnyAsync(x => x.EmpresaId == empresaId && x.SecurityUserId == tenantContextAccessor.UserId, cancellationToken))
                    throw new UnauthorizedAccessException("No puedes asignar acceso a una empresa ajena.");
                await quota.CanCrearUsuarioAsync(empresaId, cancellationToken);
            }
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
            ? [new UserEmpresaAcceso(entity.EmpresaId, string.Empty, null, string.Empty, "1", true, true)]
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

        await SaveCredentialsAsync(cancellationToken);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
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

    /// <summary>Actualiza el estado de seguridad de la identidad autenticada antes/después de resolver empresa; la búsqueda queda limitada al ID validado por Identity.</summary>
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

    /// <summary>Aplica bloqueo y auditoría al usuario por nombre global de Identity; el bypass solo sirve al flujo de autenticación y no devuelve datos de otros tenants.</summary>
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

    /// <summary>
    /// Construye consultas de usuarios. El parámetro de bypass solo se utiliza para búsqueda exacta
    /// de identidad durante autenticación antes de conocer el tenant; el resto de consultas conserva el filtro global.
    /// </summary>
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

        return ignoreQueryFilters ? query.IgnoreQueryFilters() : query.Where(x => !x.UserRoles.Any(r => r.Role.Name == SecurityRoleNames.SuperAdministrador));
    }

    private async Task<IReadOnlyCollection<Persistence.Entities.SecurityRoleEntity>> ResolveRolesAsync(
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken)
    {
        if (roleNames.Any(x => string.Equals(x.Trim(), SecurityRoleNames.SuperAdministrador, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("El superadministrador solo puede crearse mediante la inicialización segura de plataforma.");
        var normalizedNames = roleNames
            .Select(role => role.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roles = await dbContext.SecurityRoles
            .Where(role => normalizedNames.Contains(role.NormalizedName) && role.IsActive)
            .ToListAsync(cancellationToken);

        var restrictedRoleIds = await dbContext.Set<SecurityRolPermisoEntity>().AsNoTracking()
            .Where(x => x.PermisoId == TestDeIa.Shared.Security.SecurityPermissions.CatalogosAdministrar)
            .Select(x => x.RoleId).ToArrayAsync(cancellationToken);
        if (roles.Any(x => restrictedRoleIds.Contains(x.Id)) && !tenantContextAccessor.IsSystemContext)
        {
            var actor = tenantContextAccessor.UserId;
            var canGrantPlatformRole = actor.HasValue && await dbContext.Set<SecurityUserRoleEntity>().AsNoTracking()
                .AnyAsync(x => x.UserId == actor.Value && restrictedRoleIds.Contains(x.RoleId), cancellationToken);
            if (!canGrantPlatformRole)
                throw new InvalidOperationException("Un administrador de empresa no puede asignar roles de administración global.");
        }

        if (roles.Count != normalizedNames.Length)
        {
            throw new InvalidOperationException("No se pudieron resolver todos los roles del usuario.");
        }

        return roles;
    }

    private async Task SaveCredentialsAsync(CancellationToken cancellationToken)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql
            && sql.Number is 2601 or 2627 && sql.Message.Contains("IX_SecurityUsers_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El usuario, correo o persona ya está registrado. Revisa los datos ingresados.", ex);
        }
    }

    /// <summary>
    /// Carga un usuario para mutación administrativa dentro de la empresa activa. Se desactiva el
    /// filtro global solo para considerar usuarios cuya empresa primaria difiere, pero el predicado
    /// explícito exige pertenencia primaria o vínculo de acceso al tenant actual.
    /// </summary>
    private async Task<SecurityUserEntity?> LoadUserForMutationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var empresaId = tenantContextAccessor.EmpresaId;
        if (!empresaId.HasValue || empresaId.Value == Guid.Empty)
        {
            return null;
        }

        await EnsureNotProtectedAsync(userId, cancellationToken);
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
            .FirstOrDefaultAsync(current => current.Id == userId &&
                (current.EmpresaId == empresaId.Value || current.EmpresasAcceso.Any(access => access.EmpresaId == empresaId.Value)), cancellationToken);
    }

    /// <summary>Comprueba protección de la cuenta de plataforma por su ID para impedir su edición desde el mantenimiento ordinario de empresa.</summary>
    private async Task EnsureNotProtectedAsync(Guid userId, CancellationToken ct)
    {
        if (await dbContext.SecurityUserRoles.IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.Role.Name == SecurityRoleNames.SuperAdministrador, ct))
            throw new InvalidOperationException("La cuenta de superadministrador no puede editarse, bloquearse ni restablecerse desde la administración de una empresa.");
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
        var assignedPermissions = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .SelectMany(userRole => userRole.Role.RolePermissions.Select(current => current.PermisoId))
            .ToArray();
        var permissions = ResolvePermissions(roles, assignedPermissions);

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
                .Where(link => !roles.Contains(SecurityRoleNames.SuperAdministrador) || link.Empresa.OwnerUserId == user.Id)
                .OrderByDescending(link => link.IsDefault)
                .ThenBy(link => link.Empresa.RazonSocial)
                .Select(link => new UserEmpresaAcceso(
                    link.EmpresaId,
                    link.Empresa.RazonSocial,
                    link.Empresa.NombreComercial,
                    link.Empresa.Ruc,
                    link.Empresa.AmbienteSri,
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

    internal static string[] ResolvePermissions(IEnumerable<string> roles, IEnumerable<string> assignedPermissions)
    {
        var permissions = new HashSet<string>(assignedPermissions, StringComparer.OrdinalIgnoreCase);
        foreach (var role in roles)
        {
            // El acceso de SuperAdministrador al ERP propio es predecible incluso
            // si el rol existente fue creado antes del seed de permisos. Los otros
            // perfiles conservan exactamente los permisos asignados en la BD.
            if (string.Equals(role, SecurityRoleNames.SuperAdministrador, StringComparison.OrdinalIgnoreCase) &&
                SecurityPermissionCatalog.RoleMappings.TryGetValue(role, out var mappedPermissions))
            {
                permissions.UnionWith(mappedPermissions);
            }
        }

        return permissions.ToArray();
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


