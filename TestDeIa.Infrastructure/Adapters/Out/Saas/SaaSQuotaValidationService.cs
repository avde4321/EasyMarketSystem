using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Saas;

public sealed class SaaSQuotaValidationService(TestDeIaDbContext dbContext,
    IOptions<DeploymentSettingsOptions>? deployment = null) : ISaaSQuotaValidationService
{
    private DeploymentSettingsOptions Deployment => deployment?.Value ?? new DeploymentSettingsOptions();
    public async Task EnsureModuleAsync(Guid empresaId, SaasModule module, CancellationToken cancellationToken = default)
    {
        if (Deployment.IsOnPremise) return;
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null || !subscription.Plan.EsActivo)
            throw new SaaSQuotaExceededException("La empresa no tiene un plan activo.", "SUSCRIPCION_REQUERIDA");
        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaInicio, subscription.FechaVencimiento);
        var allowed = module switch { SaasModule.Sri => subscription.Plan.PermiteModuloSRI,
            SaasModule.KardexAvanzado => subscription.Plan.PermiteModuloKardexAvanzado, _ => false };
        if (!allowed) throw new SaaSQuotaExceededException($"El plan {subscription.Plan.Nombre} no incluye el módulo {module}.", "MODULO_NO_INCLUIDO");
    }
    public async Task EnsureActiveSubscriptionAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        if (!Deployment.SubscriptionEnabled) return;
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null || !subscription.Plan.EsActivo)
        {
            throw new SubscriptionRequiredException("Su empresa no cuenta con un plan activo o su periodo de suscripcion ha expirado.");
        }

        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaInicio, subscription.FechaVencimiento, useSubscriptionRequiredException: true);
    }

    public async Task CanEmitirFacturaAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        if (!Deployment.QuotasEnabled) return;
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null || !subscription.Plan.EsActivo)
            throw new SubscriptionRequiredException("No existe un plan activo para reservar cupo de facturación.");

        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaInicio, subscription.FechaVencimiento);

        if (!subscription.Plan.PermiteModuloSRI)
            throw new SaaSQuotaExceededException("El plan no incluye emisión de comprobantes electrónicos SRI.", "MODULO_NO_INCLUIDO");

        var period = MonthStart(DateTime.UtcNow);
        var confirmed = await dbContext.SaasFacturaCupos.IgnoreQueryFilters().AsNoTracking()
            .CountAsync(x => x.EmpresaId == empresaId && x.Estado == "CONFIRMADO" && x.PeriodoAutorizacion == period
                && (!subscription.FechaReinicioCupo.HasValue || x.FechaConfirmacion >= subscription.FechaReinicioCupo), cancellationToken);
        var reserved = await dbContext.SaasFacturaCupos.IgnoreQueryFilters().AsNoTracking()
            .CountAsync(x => x.EmpresaId == empresaId && x.Estado == "RESERVADO", cancellationToken);
        var used = Math.Max(confirmed, ResolveMonthlyCounter(subscription.ContadorFacturasPeriodo, subscription.FacturasEmitidasMesActual));
        if (subscription.Plan.LimiteFacturasMensuales > 0 &&
            (long)used + reserved >= subscription.Plan.LimiteFacturasMensuales)
        {
            throw new SaaSQuotaExceededException(
                $"La empresa alcanzo el limite mensual de {subscription.Plan.LimiteFacturasMensuales} factura(s) del plan {subscription.Plan.Nombre}.",
                "FACTURAS_MENSUALES");
        }
    }

    public async Task CanCrearUsuarioAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        if (!Deployment.QuotasEnabled) return;
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null || !subscription.Plan.EsActivo)
            throw new SaaSQuotaExceededException("No existe un plan activo para crear usuarios.", "SUSCRIPCION_REQUERIDA");

        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaInicio, subscription.FechaVencimiento);

        if (subscription.Plan.LimiteUsuarios <= 0)
        {
            return;
        }

        var totalUsuarios = await dbContext.SecurityUserEmpresas
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(current => current.EmpresaId == empresaId, cancellationToken);

        var added = dbContext.ChangeTracker.Entries<SecurityUserEmpresaEntity>().Count(x => x.Entity.EmpresaId == empresaId && x.State == EntityState.Added);
        var removed = dbContext.ChangeTracker.Entries<SecurityUserEmpresaEntity>().Count(x => x.Entity.EmpresaId == empresaId && x.State == EntityState.Deleted);
        if (totalUsuarios + added - removed >= subscription.Plan.LimiteUsuarios)
        {
            throw new SaaSQuotaExceededException(
                $"La empresa alcanzo el limite de {subscription.Plan.LimiteUsuarios} usuario(s) del plan {subscription.Plan.Nombre}.",
                "USUARIOS");
        }
    }

    public async Task CanCrearSucursalAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        if (!Deployment.QuotasEnabled) return;
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null || !subscription.Plan.EsActivo)
            throw new SaaSQuotaExceededException("No existe un plan activo para crear puntos de emisión.", "SUSCRIPCION_REQUERIDA");

        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaInicio, subscription.FechaVencimiento);

        if (subscription.Plan.LimiteSucursales <= 0)
        {
            return;
        }

        var totalSucursales = await dbContext.EmpresaPuntosEmision
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(current => current.EmpresaEmisoraId == empresaId, cancellationToken);

        var added = dbContext.ChangeTracker.Entries<EmpresaPuntoEmisionEntity>().Count(x => x.Entity.EmpresaEmisoraId == empresaId && x.State == EntityState.Added);
        var removed = dbContext.ChangeTracker.Entries<EmpresaPuntoEmisionEntity>().Count(x => x.Entity.EmpresaEmisoraId == empresaId && x.State == EntityState.Deleted);
        if (totalSucursales + added - removed >= subscription.Plan.LimiteSucursales)
        {
            throw new SaaSQuotaExceededException(
                $"La empresa alcanzo el limite de {subscription.Plan.LimiteSucursales} sucursal(es)/punto(s) del plan {subscription.Plan.Nombre}.",
                "SUCURSALES");
        }
    }

    public async Task BloquearCuotaAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory") return; // Solo pruebas de reglas.
        if (!dbContext.Database.IsSqlServer() || dbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("El control de cupos requiere SQL Server y una transacción activa.");
        var resource = $"SAAS-CUPO:{empresaId:N}";
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; IF @r < 0 THROW 51000, 'No se pudo bloquear el cupo SaaS. Reintente la operacion.', 1;", cancellationToken);
    }

    public async Task ReservarCupoFacturaAsync(Guid empresaId, Guid facturaId, CancellationToken cancellationToken = default)
    {
        if (!Deployment.QuotasEnabled) return;
        if (facturaId == Guid.Empty) throw new ArgumentException("La factura necesita un identificador estable.", nameof(facturaId));
        await BloquearCuotaAsync(empresaId, cancellationToken);
        var reservation = await dbContext.SaasFacturaCupos.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.FacturaId == facturaId, cancellationToken);
        if (reservation is not null && reservation.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("La reserva no pertenece a la empresa.");
        if (reservation?.Estado is "RESERVADO" or "CONFIRMADO") return;
        await CanEmitirFacturaAsync(empresaId, cancellationToken);
        if (reservation is null)
        {
            reservation = new SaasFacturaCupoEntity { FacturaId = facturaId, EmpresaId = empresaId };
            dbContext.SaasFacturaCupos.Add(reservation);
        }
        reservation.Estado = "RESERVADO";
        reservation.FechaReserva = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmarFacturaAutorizadaAsync(Guid empresaId, Guid facturaId, CancellationToken cancellationToken = default)
    {
        if (!Deployment.QuotasEnabled) return;
        await BloquearCuotaAsync(empresaId, cancellationToken);
        // La autorización debe estar guardada en la misma transacción, no solo en el ChangeTracker.
        var factura = await dbContext.Facturas.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Id == facturaId && x.Estado == FacturaEstado.AUTORIZADO)
            .Select(x => new { x.FechaAutorizacion, x.CreatedAt }).SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No se puede consumir cupo sin una factura autorizada persistida.");
        var reservation = await dbContext.SaasFacturaCupos.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.FacturaId == facturaId && x.EmpresaId == empresaId, cancellationToken);
        if (reservation?.Estado == "CONFIRMADO") return;
        if (reservation is null)
        {
            reservation = new SaasFacturaCupoEntity { FacturaId = facturaId, EmpresaId = empresaId, FechaReserva = factura.CreatedAt };
            dbContext.SaasFacturaCupos.Add(reservation);
        }
        var authorizedAt = factura.FechaAutorizacion ?? throw new InvalidOperationException("La factura autorizada requiere fecha de autorización.");
        reservation.Estado = "CONFIRMADO";
        reservation.FechaConfirmacion = authorizedAt;
        reservation.PeriodoAutorizacion = MonthStart(authorizedAt.UtcDateTime);
        await dbContext.SaveChangesAsync(cancellationToken);
        await SincronizarContadorAsync(empresaId, cancellationToken);
    }

    public async Task LiberarCupoFacturaAsync(Guid empresaId, Guid facturaId, CancellationToken cancellationToken = default)
    {
        if (!Deployment.QuotasEnabled) return;
        await BloquearCuotaAsync(empresaId, cancellationToken);
        var rejected = await dbContext.Facturas.IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(x => x.EmpresaId == empresaId && x.Id == facturaId && x.Estado == FacturaEstado.RECHAZADO, cancellationToken);
        if (!rejected) throw new InvalidOperationException("Solo puede liberar cupo de una factura rechazada.");
        var reservation = await dbContext.SaasFacturaCupos.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.EmpresaId == empresaId && x.FacturaId == facturaId, cancellationToken);
        if (reservation?.Estado == "RESERVADO")
        {
            reservation.Estado = "LIBERADO";
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SincronizarContadorAsync(Guid empresaId, CancellationToken ct)
    {
        var period = MonthStart(DateTime.UtcNow);
        var reset = await dbContext.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == empresaId)
            .MaxAsync(x => x.FechaReinicioCupo, ct);
        var count = await dbContext.SaasFacturaCupos.IgnoreQueryFilters().AsNoTracking()
            .CountAsync(x => x.EmpresaId == empresaId && x.Estado == "CONFIRMADO" && x.PeriodoAutorizacion == period
                && (!reset.HasValue || x.FechaConfirmacion >= reset), ct);
        // Se actualizan todos los snapshots de la empresa; el consumo no se reinicia al cambiar de plan.
        if (dbContext.Database.IsSqlServer())
            await dbContext.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == empresaId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FacturasEmitidasMesActual, count)
                    .SetProperty(x => x.ContadorFacturasPeriodo, period), ct);
        else
        {
            foreach (var subscription in await dbContext.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == empresaId).ToListAsync(ct))
            {
                subscription.FacturasEmitidasMesActual = count;
                subscription.ContadorFacturasPeriodo = period;
            }
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private static DateTime MonthStart(DateTime date) => new(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);

    private async Task<SubscriptionSnapshot?> GetSubscriptionAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        return await dbContext.SaasSuscripciones
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId)
            .Select(current => new SubscriptionSnapshot(
                current.EstadoSuscripcion,
                current.FechaInicio,
                current.FechaVencimiento,
                current.FacturasEmitidasMesActual,
                current.ContadorFacturasPeriodo,
                current.FechaReinicioCupo,
                new PlanSnapshot(
                    current.Plan.Nombre,
                    current.Plan.LimiteFacturasMensuales,
                    current.Plan.LimiteUsuarios,
                    current.Plan.LimiteSucursales,
                    current.Plan.EsActivo,
                    current.Plan.PermiteModuloSRI,
                    current.Plan.PermiteModuloKardexAvanzado)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void EnsureSubscriptionCanOperate(EstadoSuscripcionSaas estado, DateTime fechaInicio, DateTime fechaVencimiento, bool useSubscriptionRequiredException = false)
    {
        if (!Deployment.SubscriptionEnabled) return;
        if (estado is not (EstadoSuscripcionSaas.Activa or EstadoSuscripcionSaas.Demo))
        {
            if (useSubscriptionRequiredException)
            {
                throw new SubscriptionRequiredException("Su empresa no cuenta con un plan activo o su periodo de suscripcion ha expirado.");
            }

            throw new SaaSQuotaExceededException("La suscripcion de la empresa no permite operar porque se encuentra suspendida o cancelada.", "SUSCRIPCION_INACTIVA");
        }

        if (fechaInicio > DateTime.UtcNow || fechaVencimiento < DateTime.UtcNow)
        {
            if (useSubscriptionRequiredException)
            {
                throw new SubscriptionRequiredException("Su empresa no cuenta con un plan activo o su periodo de suscripcion ha expirado.");
            }

            throw new SaaSQuotaExceededException("La suscripción todavía no inicia o ya se encuentra vencida.", "SUSCRIPCION_NO_VIGENTE");
        }
    }

    private static int ResolveMonthlyCounter(DateTime? period, int counter)
    {
        var now = DateTime.UtcNow;
        return period.HasValue && period.Value.Year == now.Year && period.Value.Month == now.Month ? counter : 0;
    }

    private sealed record SubscriptionSnapshot(
        EstadoSuscripcionSaas EstadoSuscripcion,
        DateTime FechaInicio,
        DateTime FechaVencimiento,
        int FacturasEmitidasMesActual,
        DateTime? ContadorFacturasPeriodo,
        DateTimeOffset? FechaReinicioCupo,
        PlanSnapshot Plan);

    private sealed record PlanSnapshot(
        string Nombre,
        int LimiteFacturasMensuales,
        int LimiteUsuarios,
        int LimiteSucursales,
        bool EsActivo,
        bool PermiteModuloSRI,
        bool PermiteModuloKardexAvanzado);
}
