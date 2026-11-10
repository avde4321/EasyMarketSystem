using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Saas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Adapters.Out.Saas;

public sealed partial class SaasBillingService
{
    /// <summary>Lectura transversal de métricas SaaS de plataforma, sin datos ERP; solo accesible tras validar al SuperAdministrador.</summary>
    public async Task<SaasDashboardResponse> ObtenerDashboardAdministracionAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSuperAdministratorAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var local = now.AddHours(-5);
        var from = new DateTime(local.Year, local.Month, 1).AddHours(5);
        // Únicamente información de suscripciones y cobros de la plataforma, nunca ventas de sus clientes.
        var subscriptions = db.SaasSuscripciones.IgnoreQueryFilters().AsNoTracking();
        var payments = db.SaasPagoRegistros.IgnoreQueryFilters().AsNoTracking();
        var active = await subscriptions.CountAsync(x => x.EstadoSuscripcion == EstadoSuscripcionSaas.Activa && x.FechaVencimiento > now, cancellationToken);
        var demos = await subscriptions.CountAsync(x => x.EstadoSuscripcion == EstadoSuscripcionSaas.Demo && x.FechaVencimiento > now, cancellationToken);
        var expired = await subscriptions.CountAsync(x => x.FechaVencimiento <= now, cancellationToken);
        var pending = await payments.CountAsync(x => x.EstadoPago == EstadoPagoSaas.Pendiente, cancellationToken);
        var income = await payments.Where(x => x.EstadoPago == EstadoPagoSaas.Aprobado && x.FechaConfirmacion >= from && x.FechaConfirmacion <= now)
            .SumAsync(x => (decimal?)x.Monto, cancellationToken) ?? 0m;
        return new(active, demos, expired, pending, income, from, now);
    }

    /// <summary>Resuelve y valida la identidad global del propietario de plataforma; el bypass no retorna información de empresas.</summary>
    private async Task<Guid> EnsureSuperAdministratorAsync(CancellationToken ct)
    {
        var actor = tenant.UserId ?? throw new UnauthorizedAccessException("Se requiere un administrador autorizado.");
        // Excepción global intencionada: solo la identidad de plataforma puede administrar pagos entre empresas.
        // No se modifica IsSystemContext ni se confía únicamente en el rol del JWT.
        var admin = await db.SecurityUserRoles.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.UserId == actor
            && x.Role.Name == SecurityRoleNames.SuperAdministrador && x.Role.IsActive
            && x.User.IsActive && !x.User.BloqueadoManualmente, ct);
        if (!admin) throw new UnauthorizedAccessException("Solo el superadministrador de la plataforma puede gestionar pagos SaaS.");
        return actor;
    }

    /// <summary>Consulta global de pagos SaaS (no movimientos ERP), autorizada explícitamente para el SuperAdministrador y paginada.</summary>
    public async Task<PagedResultResponse<SaasPagoAdminResponse>> ConsultarPagosAdministracionAsync(SaasPagosAdminQuery query, CancellationToken cancellationToken = default)
    {
        await EnsureSuperAdministratorAsync(cancellationToken);
        ArgumentNullException.ThrowIfNull(query);
        Validator.ValidateObject(query, new ValidationContext(query), true);
        // Bandeja global autorizada, no un listado ERP. Un administrador de empresa nunca llega a esta consulta.
        var payments = db.SaasPagoRegistros.IgnoreQueryFilters().AsNoTracking().AsQueryable();
        if (query.EmpresaId.HasValue) payments = payments.Where(x => x.EmpresaId == query.EmpresaId.Value);
        var state = query.Estado?.Trim();
        if (!string.IsNullOrEmpty(state) && !state.Equals("Todos", StringComparison.OrdinalIgnoreCase))
        {
            var status = Enum.Parse<EstadoPagoSaas>(state, true);
            payments = payments.Where(x => x.EstadoPago == status);
        }
        var term = query.Term?.Trim();
        if (!string.IsNullOrEmpty(term))
            payments = payments.Where(x => x.Empresa.Ruc.Contains(term) || x.Empresa.RazonSocial.Contains(term)
                || x.ComprobanteReferencia.Contains(term));
        // FechaRegistro se almacena UTC. Los filtros diarios se interpretan como Ecuador continental.
        if (query.FechaDesde.HasValue)
        {
            var from = query.FechaDesde.Value.ToDateTime(TimeOnly.MinValue).AddHours(5);
            payments = payments.Where(x => x.FechaPago >= from);
        }
        if (query.FechaHasta.HasValue)
        {
            var until = query.FechaHasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(5);
            payments = payments.Where(x => x.FechaPago < until);
        }
        var count = await payments.CountAsync(cancellationToken);
        var items = await payments.OrderByDescending(x => x.FechaPago).ThenBy(x => x.Id)
            .Skip(query.Skip).Take(query.Take)
            .Select(x => new SaasPagoAdminResponse(x.Id, x.EmpresaId, x.Empresa.RazonSocial, x.Empresa.Ruc,
                x.SaasPlanId, x.Plan.Nombre, x.Plan.EsActivo, x.Monto, x.FechaPago, x.FechaPagoInformada,
                x.MetodoPago.ToString(), x.EstadoPago.ToString(), x.ComprobanteReferencia, x.Observaciones,
                x.FechaConfirmacion, x.ObservacionesConfirmacion, x.ConfirmadoPorUsuarioId, x.PeriodoMeses))
            .ToArrayAsync(cancellationToken);
        return new PagedResultResponse<SaasPagoAdminResponse> { Items = items, TotalCount = count, Skip = query.Skip, Take = query.Take };
    }
}
