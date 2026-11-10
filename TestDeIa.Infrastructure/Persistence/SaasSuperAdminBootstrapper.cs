using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Persistence;

public sealed class SaasSuperAdminBootstrapper(TestDeIaDbContext db, IConfiguration configuration,
    IPasswordHashService passwords, ILogger<SaasSuperAdminBootstrapper> logger)
{
    private const string Section = "Saas:SuperAdminBootstrap:";
    public const string InstallationKey = "SUPERADMIN";

    /// <summary>
    /// Inicialización privilegiada de plataforma, habilitada solo por configuración o ejecución
    /// local explícita. Los bypasses globales son para comprobar unicidad de Identity/roles o retirar
    /// la cuenta indicada por ID; nunca se usan para consultas operativas ni se devuelven datos tenant.
    /// </summary>
    public async Task<bool> EnsureAsync(bool force = false, CancellationToken cancellationToken = default, Guid? retirarCajeroId = null)
    {
        if (retirarCajeroId.HasValue && !force) throw new InvalidOperationException("Retirar una cuenta previa requiere el modo de instalación local explícito.");
        if (!force && !configuration.GetValue<bool>(Section + "Enabled")) return false;
        if (db.Database.CurrentTransaction is not null) throw new InvalidOperationException("La inicialización administra su propia transacción.");
        Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy strategy = db.Database.IsSqlServer()
            ? new SqlServerRetryingExecutionStrategy(db, 2, TimeSpan.FromSeconds(1), [1205]) : db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (db.Database.IsSqlServer())
                    await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource='SAAS-SUPERADMIN-BOOTSTRAP', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; IF @r<0 THROW 51004, 'No se pudo bloquear la inicializacion segura.', 1;", cancellationToken);
                if (await db.Set<SaasInicializacionEntity>().AsNoTracking().AnyAsync(x => x.Clave == InstallationKey, cancellationToken))
                {
                    logger.LogInformation("El superadministrador ya fue inicializado. No se cambia contraseña, estado ni roles.");
                    await tx.CommitAsync(cancellationToken);
                    return false;
                }
                var role = await db.SecurityRoles.AsNoTracking().SingleOrDefaultAsync(x => x.Name == SecurityRoleNames.SuperAdministrador && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Aplica la migración del superadministrador antes de inicializar la cuenta.");
                if (await db.SecurityUserRoles.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.RoleId == role.Id, cancellationToken))
                    throw new InvalidOperationException("Ya existe una asignación de superadministrador sin marca de instalación. Se requiere revisión manual; no se creará otra cuenta.");
                var userName = configuration[Section + "UserName"]?.Trim();
                var email = configuration[Section + "Email"]?.Trim();
                var displayName = configuration[Section + "NombreCompleto"]?.Trim() ?? "Administrador de la plataforma";
                var password = configuration[Section + "Password"];
                if (string.IsNullOrWhiteSpace(userName) || userName.Length > 180 || userName.Length < 3 || userName.Any(char.IsWhiteSpace)
                    || string.IsNullOrWhiteSpace(email) || email.Length > 180 || !new EmailAddressAttribute().IsValid(email)
                    || displayName.Length is < 2 or > 150)
                    throw new InvalidOperationException("Configura un usuario, correo válido y nombre para Saas:SuperAdminBootstrap. No se usan credenciales predeterminadas.");
                ValidatePassword(password);
                var normalizedName = userName.ToUpperInvariant();
                var normalizedEmail = email.ToUpperInvariant();
                if (retirarCajeroId.HasValue)
                {
                    var previous = await db.SecurityUsers.IgnoreQueryFilters().Include(x => x.UserRoles).ThenInclude(x => x.Role)
                        .Include(x => x.EmpresasAcceso).SingleOrDefaultAsync(x => x.Id == retirarCajeroId.Value, cancellationToken)
                        ?? throw new InvalidOperationException("No existe la cuenta de cajero indicada.");
                    if (previous.NormalizedEmail != normalizedEmail || previous.UserRoles.Count != 1 || previous.UserRoles.Single().Role.Name != SecurityRoleNames.Cajero)
                        throw new InvalidOperationException("La cuenta indicada no es exclusivamente un cajero con el correo del propietario. No se modificó.");
                    // Baja lógica autorizada: conservar el Guid para cajas, facturas y auditoría históricas.
                    var archive = previous.Id.ToString("N");
                    previous.UserName = "Retirado-" + archive;
                    previous.NormalizedUserName = previous.UserName.ToUpperInvariant();
                    previous.Email = "retirado-" + archive + "@cuentas.invalid";
                    previous.NormalizedEmail = previous.Email.ToUpperInvariant();
                    previous.PasswordHash = passwords.Hash(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"));
                    previous.IsActive = false; previous.BloqueadoManualmente = true; previous.TokensInvalidosDesde = DateTimeOffset.UtcNow;
                    previous.UserRoles.Clear(); previous.EmpresasAcceso.Clear();
                    db.RemoveRange(await db.Set<SecurityUserPuntoEmisionEntity>().IgnoreQueryFilters()
                        .Where(x => x.SecurityUserId == previous.Id).ToArrayAsync(cancellationToken));
                    db.SecurityAuditLogs.Add(new SecurityAuditLogEntity { Id = Guid.NewGuid(), EmpresaId = previous.EmpresaId, UsuarioId = previous.Id,
                        FechaEvento = DateTimeOffset.UtcNow, TipoEvento = SecurityAuditEventType.CuentaRetiradaParaSudo,
                        Detalles = "Baja lógica de cajero autorizada por el propietario durante la instalación del superadministrador. Historial financiero preservado." });
                    await db.SaveChangesAsync(cancellationToken);
                }
                if (await db.SecurityUsers.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.NormalizedUserName == normalizedName || x.NormalizedEmail == normalizedEmail, cancellationToken))
                    throw new InvalidOperationException("El usuario o correo ya existe. No se promueven cuentas existentes automáticamente.");
                var company = configuration.GetValue<Guid?>(Section + "EmpresaId") ?? Guid.Parse("22222222-2222-2222-2222-222222222222");
                if (!await db.EmpresasEmisoras.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.Id == company && x.IsActive, cancellationToken))
                    throw new InvalidOperationException("No existe la empresa de instalación configurada.");
                var id = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
                var persona = new PersonaEntity { Id = Guid.NewGuid(), EmpresaId = company, TipoIdentificacion = "00", Identificacion = "ROOT-" + id.ToString("N")[..16],
                    RazonSocialONombresCompletos = displayName, CorreoElectronicoPrincipal = email, DireccionPrincipal = "Administración de plataforma",
                    IsActive = true, IsSystemRecord = true, CreatedAt = now };
                db.SecurityUsers.Add(new SecurityUserEntity { Id = id, EmpresaId = company, PersonaId = persona.Id, Persona = persona,
                    UserName = userName, NormalizedUserName = normalizedName, Email = email, NormalizedEmail = normalizedEmail,
                    DisplayName = displayName, PasswordHash = passwords.Hash(password!), IsActive = true, CreatedAt = now,
                    UserRoles = [new SecurityUserRoleEntity { UserId = id, RoleId = role.Id }],
                    EmpresasAcceso = [new SecurityUserEmpresaEntity { SecurityUserId = id, EmpresaId = company, IsDefault = true, CreatedAt = now }] });
                db.Set<SaasInicializacionEntity>().Add(new SaasInicializacionEntity { Clave = InstallationKey, UsuarioId = id, FechaCreacion = now });
                db.SecurityAuditLogs.Add(new SecurityAuditLogEntity { Id = Guid.NewGuid(), EmpresaId = company, UsuarioId = id,
                    FechaEvento = now, TipoEvento = SecurityAuditEventType.SuperAdminInicializado, Detalles = "Creación inicial controlada de cuenta SuperAdministrador. Sin contraseña en logs." });
                await db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                logger.LogInformation("Superadministrador inicializado. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}", company, id);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(CancellationToken.None);
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }

    public static void ValidatePassword(string? password)
    {
        if (password is null || password.Length is < 16 or > 128 || !password.Any(char.IsUpper) || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit) || !password.Any(c => !char.IsLetterOrDigit(c)) || password.Any(char.IsWhiteSpace))
            throw new InvalidOperationException("La contraseña inicial debe tener 16–128 caracteres, mayúscula, minúscula, número y símbolo, sin espacios.");
    }
}
