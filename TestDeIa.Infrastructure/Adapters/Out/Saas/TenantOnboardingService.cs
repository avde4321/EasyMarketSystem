using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Saas;

public sealed partial class TenantOnboardingService(TestDeIaDbContext db, ISaaSQuotaValidationService quota, IConfiguration configuration,
    TestDeIa.Application.Modules.Security.Ports.Out.IPasswordHashService? passwordHasher = null,
    TestDeIa.Application.Modules.Security.Ports.Out.ISecurityTokenGenerator? tokenGenerator = null) : ITenantOnboardingService
{
    /// <summary>
    /// Completa el alta administrativa de una suscripción para una empresa concreta. Omite filtros
    /// globales porque el proceso puede ejecutarse antes de activar el tenant context, pero cada entidad
    /// transaccional se valida por EmpresaId y propietario/administrador antes de insertar.
    /// </summary>
    public async Task AsignarSuscripcionInicialAsync(Guid empresaId, Guid administradorId, int? planInicialId, CancellationToken cancellationToken = default)
    {
        if (configuration.GetSection(DeploymentSettingsOptions.SectionName).Get<DeploymentSettingsOptions>()?.IsOnPremise == true) return;
        await quota.BloquearCuotaAsync(empresaId, cancellationToken);
        if (await db.SaasSuscripciones.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == empresaId, cancellationToken)) return;
        var company = await db.EmpresasEmisoras.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == empresaId && x.OwnerUserId == administradorId, cancellationToken)
            ?? throw new UnauthorizedAccessException("El administrador no corresponde al propietario de la empresa.");
        if (!await db.SecurityUsers.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.Id == administradorId && x.IsActive && !x.BloqueadoManualmente, cancellationToken)
            || !await db.SecurityUserEmpresas.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.EmpresaId == company.Id && x.SecurityUserId == administradorId, cancellationToken))
            throw new InvalidOperationException("Debe existir un administrador activo y vinculado a la empresa antes de asignar la suscripción.");
        var planId = planInicialId ?? configuration.GetValue<int>("Saas:DemoPlanId", 1);
        var plan = await db.SaasPlanes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == planId && x.EsActivo, cancellationToken)
            ?? throw new InvalidOperationException("El plan inicial no existe o está inactivo.");
        var now = DateTime.UtcNow;
        db.SaasSuscripciones.Add(new SaasSuscripcionEntity
        {
            Id = Guid.NewGuid(), EmpresaId = empresaId, SaasPlanId = plan.Id,
            FechaInicio = now, FechaVencimiento = now.AddDays(15), EstadoSuscripcion = EstadoSuscripcionSaas.Activa,
            ContadorFacturasPeriodo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        db.SecurityAuditLogs.Add(new SecurityAuditLogEntity
        {
            Id = Guid.NewGuid(), EmpresaId = empresaId, UsuarioId = administradorId, FechaEvento = DateTimeOffset.UtcNow,
            TipoEvento = TestDeIa.Domain.Modules.Security.Entities.SecurityAuditEventType.SuscripcionInicial,
            Detalles = $"Prueba inicial de 15 días. PlanId={plan.Id}; FinUtc={now.AddDays(15):O}. No representa pago ni renovación."
        });
        await db.SaveChangesAsync(cancellationToken);
        // El propietario ocupa un asiento y cada punto de emisión consume una unidad del plan.
        var users = await db.SecurityUserEmpresas.IgnoreQueryFilters().CountAsync(x => x.EmpresaId == empresaId, cancellationToken);
        var points = await db.EmpresaPuntosEmision.IgnoreQueryFilters().CountAsync(x => x.EmpresaEmisoraId == empresaId, cancellationToken);
        if ((plan.LimiteUsuarios > 0 && users > plan.LimiteUsuarios) || (plan.LimiteSucursales > 0 && points > plan.LimiteSucursales))
            throw new SaaSQuotaExceededException("La configuración inicial supera las capacidades del plan seleccionado.", "ONBOARDING_LIMITES");
    }
}
