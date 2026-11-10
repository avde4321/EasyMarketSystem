using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Security.Models;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Infrastructure.Adapters.Out.Contabilidad;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Empresa;
using TestDeIa.Shared.Responses.Security;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Adapters.Out.Saas;

public sealed partial class TenantOnboardingService
{
    /// <summary>
    /// Registra un tenant aún inexistente. Antes de crear la empresa realiza búsquedas globales
    /// exclusivamente booleanas para impedir duplicidad de correo/RUC; no devuelve ni modifica datos de otros tenants.
    /// </summary>
    public async Task<LoginResponse> RegistrarPublicoAsync(PublicTenantRegisterDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        if (db.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("El registro público administra su propia transacción.");
        var hasher = passwordHasher ?? throw new InvalidOperationException("Servicio de contraseñas no configurado.");
        var tokens = tokenGenerator ?? throw new InvalidOperationException("Servicio de sesión no configurado.");
        var email = dto.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        // El hash costoso se calcula antes de tomar bloqueos en la base de datos.
        var hash = hasher.Hash(dto.Password);
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Búsquedas globales intencionales: todavía no existe tenant autenticado.
                // Los índices únicos son la última defensa ante dos registros simultáneos.
                if (await db.SecurityUsers.IgnoreQueryFilters().AsNoTracking().AnyAsync(
                    x => x.NormalizedEmail == normalizedEmail || x.NormalizedUserName == normalizedEmail, cancellationToken)
                    || await db.EmpresasEmisoras.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.Ruc == dto.Ruc, cancellationToken))
                    throw new PublicTenantRegistrationException("No es posible registrar estos datos. Revisa si ya tienes una cuenta o contacta al administrador.");

                var plan = await db.SaasPlanes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == dto.PlanId && x.EsActivo, cancellationToken)
                    ?? throw new PublicTenantRegistrationException("El plan seleccionado no está disponible.");
                var role = await db.SecurityRoles.AsNoTracking().SingleOrDefaultAsync(
                    x => x.NormalizedName == SecurityRoleNames.AdministradorEmpresa.ToUpperInvariant() && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Debe aplicar la migración de auto-registro antes de habilitarlo.");
                var permissions = await db.Set<SecurityRolPermisoEntity>().AsNoTracking()
                    .Where(x => x.RoleId == role.Id).Select(x => x.PermisoId).ToArrayAsync(cancellationToken);
                if (permissions.Length == 0 || permissions.Contains(SecurityPermissions.CatalogosAdministrar))
                    throw new InvalidOperationException("Los permisos del administrador de empresa no están configurados correctamente.");

                var now = DateTimeOffset.UtcNow;
                var companyId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var pointId = Guid.NewGuid();
                // Identidad interna provisional, NO es un RUC ni una identificación fiscal SRI.
                var ownerIdentification = "REG" + userId.ToString("N")[..24];
                var persona = new PersonaEntity
                {
                    Id = Guid.NewGuid(), EmpresaId = companyId, TipoIdentificacion = "00", Identificacion = ownerIdentification,
                    RazonSocialONombresCompletos = dto.NombreCompleto.Trim(), DireccionPrincipal = dto.Direccion.Trim(),
                    CorreoElectronicoPrincipal = email, TelefonoCelular = dto.Telefono.Trim(), IsActive = true, CreatedAt = now
                };
                db.EmpresasEmisoras.Add(new EmpresaEmisoraEntity
                {
                    Id = companyId, OwnerUserId = userId, Ruc = dto.Ruc, RazonSocial = dto.RazonSocial.Trim(),
                    NombreComercial = dto.NombreComercial?.Trim(), DireccionMatriz = dto.Direccion.Trim(),
                    DireccionEstablecimiento = dto.Direccion.Trim(), Establecimiento = "001", PuntoEmision = "001",
                    AmbienteSri = "1", TipoEmision = "1", ModoDesarrollo = false,
                    ObligadoContabilidad = dto.ObligadoLlevarContabilidad, IsActive = true, CreatedAt = now
                });
                db.SecurityUsers.Add(new SecurityUserEntity
                {
                    Id = userId, EmpresaId = companyId, PersonaId = persona.Id, Persona = persona,
                    UserName = email, NormalizedUserName = normalizedEmail, Email = email, NormalizedEmail = normalizedEmail,
                    DisplayName = dto.NombreCompleto.Trim(), PasswordHash = hash, IsActive = true, CreatedAt = now,
                    UserRoles = [new SecurityUserRoleEntity { UserId = userId, RoleId = role.Id }],
                    EmpresasAcceso = [new SecurityUserEmpresaEntity { SecurityUserId = userId, EmpresaId = companyId, IsDefault = true, CreatedAt = now }]
                });
                db.EmpresaPuntosEmision.Add(new EmpresaPuntoEmisionEntity
                {
                    Id = pointId, EmpresaEmisoraId = companyId, Establecimiento = "001", PuntoEmision = "001",
                    NombreCaja = "Caja General", DireccionEstablecimiento = dto.Direccion.Trim(), IsDefault = true
                });
                db.Set<SecurityUserPuntoEmisionEntity>().Add(new SecurityUserPuntoEmisionEntity
                {
                    EmpresaId = companyId, SecurityUserId = userId, EmpresaPuntoEmisionId = pointId, CreatedAt = now
                });
                db.SaasSuscripciones.Add(new SaasSuscripcionEntity
                {
                    Id = Guid.NewGuid(), EmpresaId = companyId, SaasPlanId = plan.Id, FechaInicio = now.UtcDateTime,
                    FechaVencimiento = now.AddDays(dto.EsDemo ? 15 : 3).UtcDateTime,
                    EstadoSuscripcion = dto.EsDemo ? EstadoSuscripcionSaas.Demo : EstadoSuscripcionSaas.PendientePago,
                    ContadorFacturasPeriodo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                });
                foreach (var document in new[] { "01", "03", "04", "07" })
                    db.Set<FacturaSecuencialEntity>().Add(new FacturaSecuencialEntity
                    {
                        Id = Guid.NewGuid(), EmpresaId = companyId, AmbienteSri = "1", CodigoDocumento = document,
                        Establecimiento = "001", PuntoEmision = "001", UltimoSecuencial = 0, CreatedAt = now
                    });
                db.CuentasContables.AddRange(new CatalogoNiifSeed().BuildForEmpresa(companyId));
                db.SecurityAuditLogs.Add(new SecurityAuditLogEntity
                {
                    Id = Guid.NewGuid(), EmpresaId = companyId, UsuarioId = userId, FechaEvento = now,
                    TipoEvento = SecurityAuditEventType.SuscripcionInicial,
                    Detalles = $"Auto-registro público. PlanId={plan.Id}; Demo={dto.EsDemo}. Sin confirmación de pago ni apertura de caja."
                });
                await db.SaveChangesAsync(cancellationToken);
                var roles = new[] { SecurityRoleNames.AdministradorEmpresa };
                var token = tokens.Generate(new AuthenticatedUser(userId, email, dto.NombreCompleto.Trim(), ownerIdentification, email, companyId, roles, permissions));
                await transaction.CommitAsync(cancellationToken);
                return new LoginResponse
                {
                    Succeeded = true, Token = token.AccessToken, ExpiresAt = token.ExpiresAt, UserName = email,
                    DisplayName = dto.NombreCompleto.Trim(), ActiveEmpresaId = companyId, Roles = roles, Permissions = permissions,
                    Empresas = [new EmpresaOptionResponse { Id = companyId, Ruc = dto.Ruc, RazonSocial = dto.RazonSocial.Trim(),
                        NombreComercial = dto.NombreComercial?.Trim(), AmbienteSri = "1", IsActive = true, IsDefault = true }]
                };
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number is 2601 or 2627)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                db.ChangeTracker.Clear();
                throw new PublicTenantRegistrationException("No es posible registrar estos datos. Revisa si ya tienes una cuenta o contacta al administrador.");
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }
}
