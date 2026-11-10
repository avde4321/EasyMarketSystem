using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Adapters.Out.Saas;

public sealed partial class SaasBillingService(TestDeIaDbContext db, ISaaSQuotaValidationService quota,
    ITenantContextAccessor tenant, ILogger<SaasBillingService> logger, TimeProvider? timeProvider = null,
    Microsoft.Extensions.Options.IOptions<DeploymentSettingsOptions>? deploymentOptions = null) : ISaasBillingService
{
    private readonly DeploymentSettingsOptions deploymentSettings = deploymentOptions?.Value ?? new();
    private readonly TimeProvider clock = timeProvider ?? TimeProvider.System;

    /// <summary>Comprueba pertenencia al tenant activo mediante un bypass mínimo y predicados explícitos por EmpresaId y UsuarioId.</summary>
    private async Task<Guid> EnsureMemberAsync(CancellationToken ct)
    {
        var company = tenant.EmpresaId ?? throw new UnauthorizedAccessException("Selecciona la empresa del pago.");
        var actor = tenant.UserId ?? throw new UnauthorizedAccessException("Se requiere una sesión autenticada.");
        if (!await db.SecurityUserEmpresas.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.EmpresaId == company && x.SecurityUserId == actor, ct)
            || !await db.SecurityUsers.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.Id == actor && x.IsActive && !x.BloqueadoManualmente, ct))
            throw new UnauthorizedAccessException("No tienes acceso a esta empresa.");
        return company;
    }

    /// <summary>Consulta solo la suscripción y pagos SaaS de la empresa autenticada; el bypass existe para leer sus registros aun si el tenant-filter del contexto no está inicializado.</summary>
    public async Task<SaasBillingPortalResponse> ObtenerPortalAsync(CancellationToken cancellationToken = default)
    {
        var company = await EnsureMemberAsync(cancellationToken);
        var subscription = await db.SaasSuscripciones.IgnoreQueryFilters().AsNoTracking().Where(x => x.EmpresaId == company)
            .OrderByDescending(x => x.FechaInicio).Select(x => new { x.SaasPlanId, PlanNombre = x.Plan.Nombre, x.EstadoSuscripcion,
                x.FechaInicio, x.FechaVencimiento, PlanActivo = x.Plan.EsActivo }).FirstOrDefaultAsync(cancellationToken);
        var query = db.SaasPagoRegistros.IgnoreQueryFilters().AsNoTracking().Where(x => x.EmpresaId == company);
        var pending = await query.Where(x => x.EstadoPago == EstadoPagoSaas.Pendiente).OrderByDescending(x => x.FechaPago).ThenBy(x => x.Id)
            .Take(10).Select(x => new SaasPagoResponse(x.Id, x.EmpresaId, x.SaasPlanId, x.Monto, x.FechaPago,
                x.MetodoPago.ToString(), x.EstadoPago.ToString(), x.ComprobanteReferencia, x.FechaConfirmacion, x.FechaPagoInformada, x.PeriodoMeses)).ToArrayAsync(cancellationToken);
        var latest = await query.OrderByDescending(x => x.FechaPago).ThenBy(x => x.Id).Select(x => new SaasPagoResponse(x.Id, x.EmpresaId, x.SaasPlanId,
            x.Monto, x.FechaPago, x.MetodoPago.ToString(), x.EstadoPago.ToString(), x.ComprobanteReferencia, x.FechaConfirmacion, x.FechaPagoInformada, x.PeriodoMeses)).FirstOrDefaultAsync(cancellationToken);
        var now = clock.GetUtcNow().UtcDateTime;
        var active = subscription is not null && subscription.PlanActivo
            && subscription.EstadoSuscripcion is EstadoSuscripcionSaas.Activa or EstadoSuscripcionSaas.Demo
            && subscription.FechaInicio <= now && subscription.FechaVencimiento >= now;
        return new SaasBillingPortalResponse(active, subscription?.EstadoSuscripcion.ToString(), subscription?.FechaVencimiento,
            pending, latest, subscription?.SaasPlanId, subscription?.PlanNombre);
    }

    public async Task<SaasCheckoutResponse> PrepararCheckoutAsync(int saasPlanId, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(cancellationToken);
        if (!await db.SaasPlanes.AsNoTracking().AnyAsync(x => x.Id == saasPlanId && x.EsActivo, cancellationToken))
            throw new SaasBillingException("El plan no está disponible.");
        // No se simula dinero ni se recoge información de tarjeta sin proveedor contratado.
        return new SaasCheckoutResponse(false, "El pago con tarjeta aún no está habilitado. Puedes registrar una transferencia para revisión administrativa. No se ha realizado ningún cobro.");
    }

    /// <summary>Registra el pago de la empresa autenticada; cualquier desactivación del filtro global se limita con empresa y actor comprobados.</summary>
    public async Task<SaasPagoResponse> RegistrarTransferenciaAsync(RegistrarTransferenciaSaasDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        var empresaId = tenant.EmpresaId ?? throw new UnauthorizedAccessException("Selecciona la empresa del pago.");
        var actor = tenant.UserId ?? throw new UnauthorizedAccessException("Se requiere una sesión autenticada.");
        var reference = string.Concat(dto.ComprobanteReferencia.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
        var todayEcuador = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime.AddHours(-5));
        if (dto.FechaPago > todayEcuador) throw new SaasBillingException("La fecha del pago no puede ser posterior a hoy (hora de Ecuador).");
        if (reference.Length == 0 || decimal.Round(dto.Monto, 4) != dto.Monto)
            throw new SaasBillingException("Revisa la referencia y el monto del comprobante.");
        return await InTransactionAsync(empresaId, async () =>
        {
            var member = await db.SecurityUserEmpresas.IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(x => x.EmpresaId == empresaId && x.SecurityUserId == actor, cancellationToken);
            var activeUser = await db.SecurityUsers.IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(x => x.Id == actor && x.IsActive && !x.BloqueadoManualmente, cancellationToken);
            if (!member || !activeUser) throw new UnauthorizedAccessException("No tienes acceso a esta empresa.");
            var existing = await db.SaasPagoRegistros.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(
                x => x.EmpresaId == empresaId && x.MetodoPago == MetodoPagoSaas.TransferenciaBancaria && x.ComprobanteReferencia == reference, cancellationToken);
            if (existing is not null)
            {
                if (existing.SaasPlanId != dto.SaasPlanId || existing.PeriodoMeses != dto.PeriodoMeses || existing.Monto != dto.Monto
                    || (existing.FechaPagoInformada.HasValue && dto.FechaPago.HasValue && existing.FechaPagoInformada != dto.FechaPago))
                    throw new SaasBillingException("La referencia ya fue registrada con otro monto, período o plan.");
                return Map(existing);
            }
            var plan = await db.SaasPlanes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == dto.SaasPlanId && x.EsActivo, cancellationToken)
                ?? throw new SaasBillingException("El plan no está disponible.");
            if (plan.PrecioMensual <= 0)
                throw new SaasBillingException("Este plan requiere una cotización comercial antes de registrar el pago.");
            var montoEsperado = plan.PrecioMensual * dto.PeriodoMeses;
            if (dto.Monto != montoEsperado)
                throw new SaasBillingException($"El monto debe corresponder a {dto.PeriodoMeses} mes(es) del plan seleccionado: USD {montoEsperado:N2}.");
            var now = clock.GetUtcNow();
            var payment = new SaasPagoRegistroEntity
            {
                Id = Guid.NewGuid(), EmpresaId = empresaId, SaasPlanId = plan.Id, PeriodoMeses = dto.PeriodoMeses, Monto = dto.Monto,
                FechaPago = now.UtcDateTime, FechaPagoInformada = dto.FechaPago, MetodoPago = MetodoPagoSaas.TransferenciaBancaria,
                EstadoPago = EstadoPagoSaas.Pendiente, ComprobanteReferencia = reference,
                Observaciones = dto.Observaciones?.Trim(), RegistradoPorUsuarioId = actor
            };
            db.SaasPagoRegistros.Add(payment);
            AddAudit(payment, actor, SecurityAuditEventType.PagoSaasRegistrado, now, "Comprobante informado; pendiente de verificación bancaria.");
            await db.SaveChangesAsync(cancellationToken);
            return Map(payment);
        }, cancellationToken);
    }

    public Task<SaasPagoResponse> AprobarPagoAsync(Guid pagoId, Guid empresaId, string? observaciones = null, CancellationToken cancellationToken = default)
        => ConfirmarAsync(pagoId, empresaId, true, observaciones, cancellationToken);

    public Task<SaasPagoResponse> RechazarPagoAsync(Guid pagoId, Guid empresaId, string? observaciones = null, CancellationToken cancellationToken = default)
        => ConfirmarAsync(pagoId, empresaId, false, observaciones, cancellationToken);

    /// <summary>Operación administrativa global, autorizada por EnsureSuperAdministratorAsync; cada pago, suscripción, miembro y punto se restringe al EmpresaId solicitado.</summary>
    private async Task<SaasPagoResponse> ConfirmarAsync(Guid pagoId, Guid empresaId, bool approve, string? observaciones, CancellationToken ct)
    {
        if (pagoId == Guid.Empty || empresaId == Guid.Empty) throw new SaasBillingException("Identifica el pago y su empresa.");
        if (observaciones?.Length > 2000) throw new SaasBillingException("La observación no puede superar 2000 caracteres.");
        var actor = await EnsureSuperAdministratorAsync(ct);
        return await InTransactionAsync(empresaId, async () =>
        {
            var payment = await db.SaasPagoRegistros.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Id == pagoId && x.EmpresaId == empresaId, ct)
                ?? throw new KeyNotFoundException("No se encontró el pago para la empresa indicada.");
            var desired = approve ? EstadoPagoSaas.Aprobado : EstadoPagoSaas.Rechazado;
            if (payment.EstadoPago == desired) return Map(payment); // Un reintento nunca renueva otra vez.
            if (payment.EstadoPago != EstadoPagoSaas.Pendiente)
                throw new SaasBillingException("El pago ya tiene una decisión definitiva. No se puede modificar ni activar otra vez.");
            var now = clock.GetUtcNow();
            var auditDetail = string.Empty;
            if (approve)
            {
                var plan = await db.SaasPlanes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == payment.SaasPlanId && x.EsActivo, ct)
                    ?? throw new SaasBillingException("No se puede activar un plan inactivo.");
                var subscriptions = await db.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == empresaId).Take(2).ToListAsync(ct);
                if (subscriptions.Count > 1)
                    throw new SaasBillingException("La empresa tiene más de una suscripción configurada. Corrige ese dato antes de aprobar el pago.");
                var users = await db.SecurityUserEmpresas.IgnoreQueryFilters().CountAsync(x => x.EmpresaId == empresaId, ct);
                var points = await db.EmpresaPuntosEmision.IgnoreQueryFilters().CountAsync(x => x.EmpresaEmisoraId == empresaId, ct);
                if ((plan.LimiteUsuarios > 0 && users > plan.LimiteUsuarios) || (plan.LimiteSucursales > 0 && points > plan.LimiteSucursales))
                    throw new SaasBillingException("La empresa supera los límites de usuarios o puntos de emisión del plan seleccionado.");

                var subscription = subscriptions.Count == 0
                    ? new SaasSuscripcionEntity
                    {
                        Id = Guid.NewGuid(),
                        EmpresaId = empresaId,
                        FechaInicio = now.UtcDateTime,
                        FechaVencimiento = CalcularVencimiento(now.UtcDateTime, now.UtcDateTime, payment.PeriodoMeses)
                    }
                    : subscriptions[0];
                var esNuevaSuscripcion = subscriptions.Count == 0;
                subscription.SaasPlanId = plan.Id;
                subscription.EstadoSuscripcion = EstadoSuscripcionSaas.Activa;
                subscription.FechaInicio = now.UtcDateTime;
                if (!esNuevaSuscripcion)
                    subscription.FechaVencimiento = CalcularVencimiento(subscription.FechaVencimiento, now.UtcDateTime, payment.PeriodoMeses);
                subscription.FacturasEmitidasMesActual = 0;
                subscription.ContadorFacturasPeriodo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                subscription.FechaReinicioCupo = now;
                if (esNuevaSuscripcion)
                    db.SaasSuscripciones.Add(subscription);
                auditDetail = esNuevaSuscripcion
                    ? $"Nueva suscripción del plan {plan.Id} activada por {payment.PeriodoMeses} mes(es); nuevo corte de cupo sin borrar historial."
                    : $"Suscripción del plan {plan.Id} renovada/actualizada por {payment.PeriodoMeses} mes(es); nuevo corte de cupo sin borrar historial.";
            }
            else
            {
                auditDetail = "Pago rechazado; suscripción sin cambios.";
            }
            payment.EstadoPago = desired;
            payment.FechaConfirmacion = now.UtcDateTime;
            payment.ConfirmadoPorUsuarioId = actor;
            payment.ObservacionesConfirmacion = observaciones?.Trim();
            AddAudit(payment, actor, approve ? SecurityAuditEventType.PagoSaasAprobado : SecurityAuditEventType.PagoSaasRechazado, now,
                auditDetail);
            await db.SaveChangesAsync(ct);
            return Map(payment);
        }, ct);
    }

    public static DateTime CalcularVencimiento(DateTime actual, DateTime nowUtc, int periodoMeses = 1)
    {
        if (periodoMeses is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(periodoMeses));
        return (actual > nowUtc ? actual : nowUtc).AddMonths(periodoMeses);
    }

    private async Task<T> InTransactionAsync<T>(Guid empresaId, Func<Task<T>> operation, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is not null) throw new InvalidOperationException("Billing administra su propia transacción.");
        Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy strategy = db.Database.IsSqlServer()
            ? new SqlServerRetryingExecutionStrategy(db, 2, TimeSpan.FromSeconds(1), [1205])
            : db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                await quota.BloquearCuotaAsync(empresaId, ct);
                var result = await operation();
                await tx.CommitAsync(ct);
                logger.LogInformation("Operación de billing SaaS completada. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}", empresaId, tenant.UserId);
                return result;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(CancellationToken.None);
                db.ChangeTracker.Clear();
                logger.LogError(ex, "Error en billing SaaS. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}, Error: {Mensaje}", empresaId, tenant.UserId, ex.Message);
                throw;
            }
        });
    }

    private void AddAudit(SaasPagoRegistroEntity payment, Guid actor, SecurityAuditEventType type, DateTimeOffset now, string detail)
        => db.SecurityAuditLogs.Add(new SecurityAuditLogEntity { Id = Guid.NewGuid(), EmpresaId = payment.EmpresaId, UsuarioId = actor,
            FechaEvento = now, TipoEvento = type, Detalles = $"PagoId={payment.Id}; PlanId={payment.SaasPlanId}; {detail}" });
    private static SaasPagoResponse Map(SaasPagoRegistroEntity x) => new(x.Id, x.EmpresaId, x.SaasPlanId, x.Monto,
        x.FechaPago, x.MetodoPago.ToString(), x.EstadoPago.ToString(), x.ComprobanteReferencia, x.FechaConfirmacion, x.FechaPagoInformada, x.PeriodoMeses);
}
