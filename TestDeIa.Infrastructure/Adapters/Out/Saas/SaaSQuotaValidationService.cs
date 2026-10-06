using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Saas;

public sealed class SaaSQuotaValidationService(TestDeIaDbContext dbContext) : ISaaSQuotaValidationService
{
    public async Task CanEmitirFacturaAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null)
        {
            return;
        }

        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaVencimiento);

        if (subscription.Plan.LimiteFacturasMensuales > 0 &&
            ResolveMonthlyCounter(subscription.ContadorFacturasPeriodo, subscription.FacturasEmitidasMesActual) >= subscription.Plan.LimiteFacturasMensuales)
        {
            throw new SaaSQuotaExceededException(
                $"La empresa alcanzo el limite mensual de {subscription.Plan.LimiteFacturasMensuales} factura(s) del plan {subscription.Plan.Nombre}.",
                "FACTURAS_MENSUALES");
        }
    }

    public async Task CanCrearUsuarioAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null)
        {
            return;
        }

        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaVencimiento);

        if (subscription.Plan.LimiteUsuarios <= 0)
        {
            return;
        }

        var totalUsuarios = await dbContext.SecurityUserEmpresas
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(current => current.EmpresaId == empresaId, cancellationToken);

        if (totalUsuarios >= subscription.Plan.LimiteUsuarios)
        {
            throw new SaaSQuotaExceededException(
                $"La empresa alcanzo el limite de {subscription.Plan.LimiteUsuarios} usuario(s) del plan {subscription.Plan.Nombre}.",
                "USUARIOS");
        }
    }

    public async Task CanCrearSucursalAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetSubscriptionAsync(empresaId, cancellationToken);
        if (subscription is null)
        {
            return;
        }

        EnsureSubscriptionCanOperate(subscription.EstadoSuscripcion, subscription.FechaVencimiento);

        if (subscription.Plan.LimiteSucursales <= 0)
        {
            return;
        }

        var totalSucursales = await dbContext.EmpresaPuntosEmision
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(current => current.EmpresaEmisoraId == empresaId, cancellationToken);

        if (totalSucursales >= subscription.Plan.LimiteSucursales)
        {
            throw new SaaSQuotaExceededException(
                $"La empresa alcanzo el limite de {subscription.Plan.LimiteSucursales} sucursal(es)/punto(s) del plan {subscription.Plan.Nombre}.",
                "SUCURSALES");
        }
    }

    public async Task IncrementarFacturasEmitidasMesActualAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var subscription = await dbContext.SaasSuscripciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId, cancellationToken);

        if (subscription is null)
        {
            return;
        }

        if (!subscription.ContadorFacturasPeriodo.HasValue ||
            subscription.ContadorFacturasPeriodo.Value.Year != now.Year ||
            subscription.ContadorFacturasPeriodo.Value.Month != now.Month)
        {
            subscription.ContadorFacturasPeriodo = monthStart;
            subscription.FacturasEmitidasMesActual = 0;
        }

        subscription.FacturasEmitidasMesActual += 1;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<SubscriptionSnapshot?> GetSubscriptionAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        return await dbContext.SaasSuscripciones
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId)
            .Select(current => new SubscriptionSnapshot(
                current.EstadoSuscripcion,
                current.FechaVencimiento,
                current.FacturasEmitidasMesActual,
                current.ContadorFacturasPeriodo,
                new PlanSnapshot(
                    current.Plan.Nombre,
                    current.Plan.LimiteFacturasMensuales,
                    current.Plan.LimiteUsuarios,
                    current.Plan.LimiteSucursales)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static void EnsureSubscriptionCanOperate(EstadoSuscripcionSaas estado, DateTime fechaVencimiento)
    {
        if (estado is EstadoSuscripcionSaas.Suspendida or EstadoSuscripcionSaas.Cancelada)
        {
            throw new SaaSQuotaExceededException("La suscripcion de la empresa no permite operar porque se encuentra suspendida o cancelada.", "SUSCRIPCION_INACTIVA");
        }

        if (fechaVencimiento.Date < DateTime.UtcNow.Date)
        {
            throw new SaaSQuotaExceededException("La suscripcion de la empresa se encuentra vencida.", "SUSCRIPCION_VENCIDA");
        }
    }

    private static int ResolveMonthlyCounter(DateTime? period, int counter)
    {
        var now = DateTime.UtcNow;
        return period.HasValue && period.Value.Year == now.Year && period.Value.Month == now.Month ? counter : 0;
    }

    private sealed record SubscriptionSnapshot(
        EstadoSuscripcionSaas EstadoSuscripcion,
        DateTime FechaVencimiento,
        int FacturasEmitidasMesActual,
        DateTime? ContadorFacturasPeriodo,
        PlanSnapshot Plan);

    private sealed record PlanSnapshot(
        string Nombre,
        int LimiteFacturasMensuales,
        int LimiteUsuarios,
        int LimiteSucursales);
}
