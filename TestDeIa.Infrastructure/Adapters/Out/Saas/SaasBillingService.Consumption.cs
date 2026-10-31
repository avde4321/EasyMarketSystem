using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Domain.Modules.Saas.Services;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Infrastructure.Adapters.Out.Saas;

public sealed partial class SaasBillingService
{
    public async Task<PagedResultResponse<SaasConsumoResponse>> ConsultarConsumoAsync(SaasConsumoQuery query, CancellationToken cancellationToken = default)
    {
        await EnsureSuperAdministratorAsync(cancellationToken);
        ArgumentNullException.ThrowIfNull(query);
        Validator.ValidateObject(query, new ValidationContext(query), true);
        var now = clock.GetUtcNow().UtcDateTime;
        // Debe coincidir con el mes UTC utilizado por el control transaccional de cuotas existente.
        var period = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previous = period.AddMonths(-1);
        var nowOffset = new DateTimeOffset(now);
        var fromOffset = new DateTimeOffset(period);
        var previousOffset = new DateTimeOffset(previous);
        // Lectura global de metadatos SaaS autorizada arriba. Nunca se proyectan detalles comerciales.
        var companies = db.EmpresasEmisoras.IgnoreQueryFilters().AsNoTracking();
        var term = query.Term?.Trim();
        if (!string.IsNullOrEmpty(term)) companies = companies.Where(x => x.Ruc.Contains(term) || x.RazonSocial.Contains(term));
        var count = await companies.CountAsync(cancellationToken);
        var subscriptions = db.SaasSuscripciones.IgnoreQueryFilters().AsNoTracking();
        var cups = db.SaasFacturaCupos.IgnoreQueryFilters().AsNoTracking();
        var page = await companies.OrderBy(x => x.RazonSocial).ThenBy(x => x.Id).Skip(query.Skip).Take(query.Take)
            .Select(c => new
            {
                c.Id, c.RazonSocial, c.Ruc, c.IsActive,
                SubscriptionCount = subscriptions.Count(s => s.EmpresaId == c.Id),
                Reserved = cups.Count(x => x.EmpresaId == c.Id && x.Estado == "RESERVADO"),
                Subscription = subscriptions.Where(s => s.EmpresaId == c.Id).OrderByDescending(s => s.FechaInicio).ThenBy(s => s.Id)
                    .Select(s => new
                    {
                        s.EstadoSuscripcion, s.FechaInicio, s.FechaVencimiento, s.FacturasEmitidasMesActual, s.ContadorFacturasPeriodo,
                        s.Plan.Nombre, s.Plan.EsActivo, s.Plan.LimiteFacturasMensuales, s.Plan.LimiteUsuarios, s.Plan.LimiteSucursales,
                        s.Plan.PermiteModuloSRI, s.Plan.PermiteModuloKardexAvanzado,
                        Confirmed = cups.Count(x => x.EmpresaId == c.Id && x.Estado == "CONFIRMADO" && x.PeriodoAutorizacion == period
                            && (!s.FechaReinicioCupo.HasValue || x.FechaConfirmacion >= s.FechaReinicioCupo))
                    }).FirstOrDefault()
            }).ToListAsync(cancellationToken);
        var ids = page.Select(x => x.Id).ToArray();
        // Solo agregados SQL de hasta 50 empresas y dos meses. No se cargan facturas ni líneas en memoria.
        var volumes = await db.Facturas.IgnoreQueryFilters().AsNoTracking()
            .Where(x => ids.Contains(x.EmpresaId) && ((x.CreatedAt >= fromOffset && x.CreatedAt <= nowOffset)
                || (x.FechaAutorizacion >= previousOffset && x.FechaAutorizacion <= nowOffset)))
            .GroupBy(x => x.EmpresaId)
            .Select(g => new
            {
                Id = g.Key,
                Generated = g.Count(x => x.CreatedAt >= fromOffset && x.CreatedAt <= nowOffset),
                Authorized = g.Count(x => x.Estado == FacturaEstado.AUTORIZADO && x.FechaAutorizacion >= fromOffset && x.FechaAutorizacion <= nowOffset),
                Previous = g.Count(x => x.Estado == FacturaEstado.AUTORIZADO && x.FechaAutorizacion >= previousOffset && x.FechaAutorizacion < fromOffset)
            }).ToDictionaryAsync(x => x.Id, cancellationToken);
        // Capacidad suficiente más cercana; ilimitados al final. Un precio 0 personalizado no debe desplazar a todos los planes.
        var plans = await db.SaasPlanes.AsNoTracking().Where(x => x.EsActivo).OrderBy(x => x.LimiteFacturasMensuales == 0)
            .ThenBy(x => x.LimiteFacturasMensuales).ThenBy(x => x.PrecioMensual).ThenBy(x => x.Id)
            .Select(x => new { x.Nombre, x.LimiteFacturasMensuales, x.LimiteUsuarios, x.LimiteSucursales, x.PermiteModuloSRI, x.PermiteModuloKardexAvanzado })
            .ToListAsync(cancellationToken);
        var rows = page.Select(c =>
        {
            var s = c.Subscription;
            volumes.TryGetValue(c.Id, out var volume);
            var counter = s?.ContadorFacturasPeriodo is DateTime counterPeriod && counterPeriod.Year == period.Year && counterPeriod.Month == period.Month
                ? Math.Max(0, s.FacturasEmitidasMesActual) : 0;
            var used = Math.Max(counter, s?.Confirmed ?? 0);
            var limit = s?.LimiteFacturasMensuales ?? 0;
            var analysis = SaasConsumptionAnalysis.Evaluate(used, c.Reserved, limit, volume?.Authorized ?? 0, volume?.Previous ?? 0,
                now.Day, DateTime.DaysInMonth(now.Year, now.Month));
            var valid = c.IsActive && s is { EsActivo: true } && c.SubscriptionCount == 1
                && s.EstadoSuscripcion is EstadoSuscripcionSaas.Activa or EstadoSuscripcionSaas.Demo
                && s.FechaInicio <= now && s.FechaVencimiento > now;
            var target = Math.Max((long)used + c.Reserved + 1, analysis.Forecast);
            var suggested = valid && analysis.SuggestUpgrade && s is not null ? plans.FirstOrDefault(p => SaasConsumptionAnalysis.CanUpgrade(
                new(limit, s.LimiteUsuarios, s.LimiteSucursales, s.PermiteModuloSRI, s.PermiteModuloKardexAvanzado),
                new(p.LimiteFacturasMensuales, p.LimiteUsuarios, p.LimiteSucursales, p.PermiteModuloSRI, p.PermiteModuloKardexAvanzado), target))?.Nombre : null;
            return new SaasConsumoResponse(c.Id, c.RazonSocial, c.Ruc, s?.Nombre ?? "Sin plan", s?.EstadoSuscripcion.ToString() ?? "Sin suscripción",
                s?.FechaVencimiento ?? DateTime.MinValue, valid, deploymentSettings.QuotasEnabled, limit, used, c.Reserved,
                s is null || limit == 0 ? null : (int)Math.Max(0L, (long)limit - used - c.Reserved), analysis.Usage,
                volume?.Generated ?? 0, volume?.Authorized ?? 0, volume?.Previous ?? 0, analysis.Growth, analysis.Forecast,
                !valid ? "Regularizar suscripción" : analysis.SuggestUpgrade ? "Revisar ampliación" : "Dentro del plan",
                !valid ? "Revisa vigencia, estado, plan activo y unicidad de la suscripción." : analysis.Reason,
                suggested, period, now, s is not null && counter != s.Confirmed, c.SubscriptionCount);
        }).ToArray();
        return new PagedResultResponse<SaasConsumoResponse> { Items = rows, TotalCount = count, Skip = query.Skip, Take = query.Take };
    }
}
