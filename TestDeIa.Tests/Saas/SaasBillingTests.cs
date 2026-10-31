using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TestDeIa.Api.Controllers;
using TestDeIa.Api.Middleware;
using TestDeIa.Application.Common;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Adapters.Out.Saas;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Tests.Saas;

[Collection("SaasSql")]
public sealed class SaasBillingTests
{
    [SqlQuotaFact]
    public async Task ConsumptionIsPagedAggregateOnlyAndDoesNotChangeContractsOrCounters()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        await using var db = fixture.Open();
        var now = new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);
        var period = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var company = await db.EmpresasEmisoras.SingleAsync(x => x.Id == fixture.Company);
        company.IsActive = true;
        var plan = await db.SaasPlanes.SingleAsync(x => x.Id == fixture.PlanId);
        plan.LimiteFacturasMensuales = 10;
        var subscription = await db.SaasSuscripciones.SingleAsync(x => x.EmpresaId == fixture.Company);
        subscription.FechaInicio = now.UtcDateTime.AddMonths(-1);
        subscription.FechaVencimiento = now.UtcDateTime.AddMonths(1);
        subscription.FacturasEmitidasMesActual = 8;
        subscription.ContadorFacturasPeriodo = period;
        subscription.FechaReinicioCupo = now.AddDays(-1);
        var warehouse = new BodegaEntity { Id = Guid.NewGuid(), EmpresaId = fixture.Company, Codigo = "999", Nombre = "TEST-CONSUMPTION" };
        db.Bodegas.Add(warehouse);
        for (var i = 0; i < 5; i++)
        {
            var date = i < 3 ? now.AddHours(-1) : now.AddMonths(-1);
            db.Facturas.Add(new FacturaEntity { Id = Guid.NewGuid(), EmpresaId = fixture.Company, BodegaId = warehouse.Id,
                Establecimiento = "999", PuntoEmision = "999", Secuencial = i + 1, AmbienteSri = "1", Estado = FacturaEstado.AUTORIZADO,
                ClaveAcceso = i.ToString().PadLeft(49, '0'), FechaEmision = date, FechaAutorizacion = date, CreatedAt = date,
                ClienteNombre = "CONFIDENCIAL-NO-DEBE-SALIR", Total = 987654m });
        }
        for (var i = 0; i < 3; i++) db.SaasFacturaCupos.Add(new SaasFacturaCupoEntity
        {
            FacturaId = Guid.NewGuid(), EmpresaId = fixture.Company, Estado = "CONFIRMADO", PeriodoAutorizacion = period,
            FechaConfirmacion = i < 2 ? now.AddHours(-1) : now.AddDays(-3), FechaReserva = now.AddDays(-3)
        });
        db.SaasFacturaCupos.Add(new SaasFacturaCupoEntity { FacturaId = Guid.NewGuid(), EmpresaId = fixture.Company, Estado = "RESERVADO", FechaReserva = now });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = Service(db, fixture.Company, actor, now);
        var page = await service.ConsultarConsumoAsync(new SaasConsumoQuery { Term = company.Ruc, Take = 1 });
        var row = Assert.Single(page.Items);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(3, row.GeneradasMes); Assert.Equal(3, row.AutorizadasMes); Assert.Equal(2, row.AutorizadasMesAnterior);
        Assert.Equal(8, row.ConsumoCupo); Assert.Equal(1, row.Reservadas); Assert.Equal(1, row.Disponibles); Assert.Equal(90m, row.PorcentajeUso);
        Assert.True(row.ContadorDifiere); Assert.True(row.PuedeOperar); Assert.Equal("Revisar ampliación", row.Evaluacion);
        Assert.NotNull(row.PlanSugerido);
        Assert.DoesNotContain("CONFIDENCIAL", System.Text.Json.JsonSerializer.Serialize(row));
        Assert.DoesNotContain("987654", System.Text.Json.JsonSerializer.Serialize(row));
        Assert.False(db.ChangeTracker.HasChanges());
        Assert.Equal(fixture.PlanId, await db.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.SaasPlanId).SingleAsync());
        Assert.Empty((await service.ConsultarConsumoAsync(new SaasConsumoQuery { Term = company.Ruc, Skip = 1, Take = 1 })).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service(db, fixture.Company, Guid.NewGuid()).ConsultarConsumoAsync(new()));
        await Assert.ThrowsAsync<ValidationException>(() => service.ConsultarConsumoAsync(new SaasConsumoQuery { Take = 51 }));
    }

    [SqlQuotaFact]
    public async Task PlatformDashboardUsesBillingReadModelAndRequiresRealPlatformRole()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        await using var db = fixture.Open();
        var service = Service(db, fixture.Company, actor);
        await service.RegistrarTransferenciaAsync(Request(fixture.PlanId));
        var summary = await service.ObtenerDashboardAdministracionAsync();
        Assert.True(summary.PagosPendientes >= 1);
        Assert.True(summary.HastaUtc > summary.DesdeUtc);
        Assert.Equal(5, summary.DesdeUtc.Hour); // Primer día local de Ecuador, expresado en UTC.
        Assert.Equal(1, summary.DesdeUtc.Day);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service(db, fixture.Company, Guid.NewGuid()).ObtenerDashboardAdministracionAsync());
    }

    [Theory]
    [InlineData("2026-01-31", "2026-01-01", "2026-02-28")]
    [InlineData("2028-01-31", "2028-01-01", "2028-02-29")]
    [InlineData("2026-01-01", "2026-05-18", "2026-06-18")]
    [InlineData("2026-12-20", "2026-11-01", "2027-01-20")]
    public void RenewalUsesCalendarMonthAndNeverShortensPaidTime(string old, string now, string expected)
        => Assert.Equal(DateTime.Parse(expected, CultureInfo.InvariantCulture),
            SaasBillingService.CalcularVencimiento(DateTime.Parse(old, CultureInfo.InvariantCulture), DateTime.Parse(now, CultureInfo.InvariantCulture)));

    [Fact]
    public void TransferAmountValidationIsSafeUnderSpanishCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-EC");
            var valid = Request(1);
            Validator.ValidateObject(valid, new ValidationContext(valid), true);
            var invalid = Request(1); invalid.Monto = 0;
            Assert.Throws<ValidationException>(() => Validator.ValidateObject(invalid, new ValidationContext(invalid), true));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void ConfirmationEndpointDoesNotAllowAnonymousOrTenantAdminApproval()
    {
        var method = typeof(SaasBillingController).GetMethod(nameof(SaasBillingController.ConfirmPayment))!;
        Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        var auth = Assert.Single(method.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal(SecurityRoleNames.SuperAdministrador, auth.Roles);
    }

    [Fact]
    public void AdminQueueEndpointRequiresPlatformRole()
    {
        var method = typeof(SaasBillingController).GetMethod(nameof(SaasBillingController.AdminPayments))!;
        Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        Assert.Equal(SecurityRoleNames.SuperAdministrador,
            Assert.Single(method.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Roles);
    }

    [SqlQuotaFact]
    public async Task TenantAndLegacyAdminsCannotReadGlobalPaymentQueue()
    {
        foreach (var role in new[] { SecurityRoleNames.Administrador, SecurityRoleNames.AdministradorEmpresa })
        {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture, role);
        await using var db = fixture.Open();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service(db, fixture.Company, actor)
            .ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service(db, fixture.Company, actor)
            .ConsultarConsumoAsync(new SaasConsumoQuery()));
        }
    }

    [SqlQuotaFact]
    public async Task SuperAdminQueueIsGlobalPagedAndApprovalUsesTheSelectedCompanyWithoutChangingTenant()
    {
        await using var first = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        await using var second = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(first);
        var customer = await PrepareAsync(second, SecurityRoleNames.AdministradorEmpresa);
        var reference = "TEST-ADMIN-" + Guid.NewGuid().ToString("N");
        Guid secondPayment;
        await using (var db = first.Open())
        {
            var request = Request(first.PlanId); request.ComprobanteReferencia = reference;
            await Service(db, first.Company, actor).RegistrarTransferenciaAsync(request);
        }
        await using (var db = second.Open())
        {
            var request = Request(second.PlanId); request.ComprobanteReferencia = reference;
            secondPayment = (await Service(db, second.Company, customer).RegistrarTransferenciaAsync(request)).Id;
        }
        var tenant = new TenantContextAccessor { EmpresaId = first.Company, UserId = actor, IsSystemContext = false };
        await using var scoped = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable("EASYMARKET_TEST_SQL")).Options, tenant);
        var service = new SaasBillingService(scoped, new SaaSQuotaValidationService(scoped), tenant, NullLogger<SaasBillingService>.Instance);
        var page1 = await service.ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery { Term = reference, Take = 1 });
        var page2 = await service.ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery { Term = reference, Take = 1, Skip = 1 });
        Assert.Equal(2, page1.TotalCount); Assert.Equal(2, page2.TotalCount);
        Assert.NotEqual(Assert.Single(page1.Items).Id, Assert.Single(page2.Items).Id);
        Assert.Equal(new[] { first.Company, second.Company }.Order(), page1.Items.Concat(page2.Items).Select(x => x.EmpresaId).Order());
        Assert.Empty(scoped.ChangeTracker.Entries());
        Assert.False(await scoped.SecurityUserEmpresas.IgnoreQueryFilters().AnyAsync(x => x.SecurityUserId == actor && x.EmpresaId == second.Company));
        await service.AprobarPagoAsync(secondPayment, second.Company, "Acreditación comprobada en prueba.");
        var expiration = await scoped.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == second.Company).Select(x => x.FechaVencimiento).SingleAsync();
        await service.AprobarPagoAsync(secondPayment, second.Company, "Reintento de la misma aprobación.");
        Assert.Equal(expiration, await scoped.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == second.Company).Select(x => x.FechaVencimiento).SingleAsync());
        Assert.Equal(1, await scoped.SecurityAuditLogs.IgnoreQueryFilters().CountAsync(x => x.EmpresaId == second.Company
            && x.UsuarioId == actor && x.TipoEvento == TestDeIa.Domain.Modules.Security.Entities.SecurityAuditEventType.PagoSaasAprobado));
        var approved = await service.ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery { Term = reference, Estado = "Aprobado" });
        Assert.Equal(second.Company, Assert.Single(approved.Items).EmpresaId);
        var pending = await service.ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery { Term = reference });
        Assert.Equal(first.Company, Assert.Single(pending.Items).EmpresaId);
        Assert.False(tenant.IsSystemContext); Assert.Equal(first.Company, tenant.EmpresaId);
        await using var customerDb = second.Open();
        var portal = await Service(customerDb, second.Company, customer).ObtenerPortalAsync();
        Assert.True(portal.SuscripcionActiva); Assert.Empty(portal.PagosPendientes);
    }

    [SqlQuotaFact]
    public async Task BlockedSuperAdminCannotReadQueue()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        await using var db = fixture.Open();
        (await db.SecurityUsers.SingleAsync(x => x.Id == actor)).BloqueadoManualmente = true;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service(db, fixture.Company, actor)
            .ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery()));
    }

    [Fact]
    public async Task PlatformQueueDoesNotDependOnSelectedTenantSubscription()
    {
        var reached = false;
        var middleware = new RequireActiveSubscriptionMiddleware(_ => { reached = true; return Task.CompletedTask; }, NullLogger<RequireActiveSubscriptionMiddleware>.Instance);
        var context = new DefaultHttpContext(); context.Request.Method = "GET"; context.Request.Path = "/api/v1/saas/billing/admin/payments";
        context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("sub", "test")], "Test"));
        await middleware.InvokeAsync(context, new TenantContextAccessor { EmpresaId = Guid.NewGuid(), UserId = Guid.NewGuid() }, null!);
        Assert.True(reached); // Autorización por rol y DB se evalúa aparte, no se omite.
    }

    [Fact]
    public void AdminFiltersRejectInvalidStatesDatesAndUnboundedPages()
    {
        foreach (var query in new[] { new SaasPagosAdminQuery { Take = 500 }, new SaasPagosAdminQuery { Skip = -1 },
            new SaasPagosAdminQuery { Estado = "99" }, new SaasPagosAdminQuery { EmpresaId = Guid.Empty },
            new SaasPagosAdminQuery { FechaDesde = new DateOnly(2026, 11, 1), FechaHasta = new DateOnly(2026, 10, 1) } })
            Assert.Throws<ValidationException>(() => Validator.ValidateObject(query, new ValidationContext(query), true));
    }

    [SqlQuotaFact]
    public async Task AdminDailyFilterUsesEcuadorDateRatherThanUtcDay()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        await using var db = fixture.Open();
        var time = new DateTimeOffset(2026, 10, 31, 4, 59, 0, TimeSpan.Zero); // 30/10 23:59 Ecuador.
        var request = Request(fixture.PlanId); request.ComprobanteReferencia = "TEST-DATE-" + Guid.NewGuid().ToString("N");
        var service = Service(db, fixture.Company, actor, time);
        var payment = await service.RegistrarTransferenciaAsync(request);
        var localDay = await service.ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery { EmpresaId = fixture.Company,
            FechaDesde = new DateOnly(2026, 10, 30), FechaHasta = new DateOnly(2026, 10, 30) });
        Assert.Equal(payment.Id, Assert.Single(localDay.Items).Id);
        var nextDay = await service.ConsultarPagosAdministracionAsync(new SaasPagosAdminQuery { EmpresaId = fixture.Company,
            FechaDesde = new DateOnly(2026, 10, 31), FechaHasta = new DateOnly(2026, 10, 31) });
        Assert.Empty(nextDay.Items);
    }

    [SqlQuotaFact]
    public async Task ApprovalRenewsOnceResetsRealQuotaAndPreservesHistory()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        var now = DateTimeOffset.UtcNow;
        var oldEnd = now.UtcDateTime.AddDays(10);
        await using var db = fixture.Open();
        var subscription = await db.SaasSuscripciones.SingleAsync(x => x.EmpresaId == fixture.Company);
        subscription.FechaVencimiento = oldEnd;
        subscription.FacturasEmitidasMesActual = 1;
        subscription.ContadorFacturasPeriodo = new DateTime(now.Year, now.Month, 1);
        db.SaasFacturaCupos.Add(new SaasFacturaCupoEntity { EmpresaId = fixture.Company, FacturaId = Guid.NewGuid(), Estado = "CONFIRMADO",
            FechaConfirmacion = now.AddSeconds(-1), PeriodoAutorizacion = new DateTime(now.Year, now.Month, 1) });
        await db.SaveChangesAsync();
        var quota = new SaaSQuotaValidationService(db);
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => quota.CanEmitirFacturaAsync(fixture.Company));
        var service = Service(db, fixture.Company, actor, now);
        var pending = await service.RegistrarTransferenciaAsync(Request(fixture.PlanId));
        Assert.Equal("Pendiente", pending.EstadoPago);
        Assert.Equal(oldEnd, subscription.FechaVencimiento);
        await service.AprobarPagoAsync(pending.Id, fixture.Company);
        var end = subscription.FechaVencimiento;
        Assert.Equal(oldEnd.AddMonths(1), end);
        Assert.Equal(now.UtcDateTime, subscription.FechaInicio);
        Assert.Equal(EstadoSuscripcionSaas.Activa, subscription.EstadoSuscripcion);
        Assert.Equal(0, subscription.FacturasEmitidasMesActual);
        Assert.Equal(now, subscription.FechaReinicioCupo);
        await quota.CanEmitirFacturaAsync(fixture.Company);
        await service.AprobarPagoAsync(pending.Id, fixture.Company);
        Assert.Equal(end, subscription.FechaVencimiento);
        Assert.Equal(1, await db.SaasFacturaCupos.CountAsync(x => x.EmpresaId == fixture.Company));

        // Una nueva autorización vuelve a contar sin resucitar el consumo anterior al pago.
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            var warehouse = new BodegaEntity { Id = Guid.NewGuid(), EmpresaId = fixture.Company, Codigo = "999", Nombre = "TEST-BILLING" };
            db.Bodegas.Add(warehouse);
            var invoiceId = Guid.NewGuid();
            await quota.ReservarCupoFacturaAsync(fixture.Company, invoiceId);
            db.Facturas.Add(new FacturaEntity { Id = invoiceId, EmpresaId = fixture.Company, BodegaId = warehouse.Id, Establecimiento = "999",
                PuntoEmision = "999", Secuencial = 1, AmbienteSri = "1", Estado = FacturaEstado.AUTORIZADO, FechaAutorizacion = DateTimeOffset.UtcNow,
                ClaveAcceso = new string('1', 49), FechaEmision = now, CreatedAt = now });
            await db.SaveChangesAsync();
            await quota.ConfirmarFacturaAutorizadaAsync(fixture.Company, invoiceId);
            await tx.CommitAsync();
        }
        db.ChangeTracker.Clear();
        Assert.Equal(1, await db.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FacturasEmitidasMesActual).SingleAsync());
        Assert.Equal(2, await db.SaasFacturaCupos.CountAsync(x => x.EmpresaId == fixture.Company && x.Estado == "CONFIRMADO"));
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => quota.CanEmitirFacturaAsync(fixture.Company));
    }

    [SqlQuotaFact]
    public async Task ConcurrentReferenceAndApprovalAreIdempotent()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        var now = DateTimeOffset.UtcNow;
        var requests = await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            await using var db = fixture.Open();
            return await Service(db, fixture.Company, actor, now).RegistrarTransferenciaAsync(Request(fixture.PlanId));
        }));
        var id = Assert.Single(requests.Select(x => x.Id).Distinct());
        DateTime previousEnd;
        await using (var db = fixture.Open()) previousEnd = await db.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FechaVencimiento).SingleAsync();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            await using var db = fixture.Open();
            return await Service(db, fixture.Company, actor, now).AprobarPagoAsync(id, fixture.Company);
        }));
        await using var verify = fixture.Open();
        Assert.Equal(previousEnd.AddMonths(1), await verify.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FechaVencimiento).SingleAsync());
        Assert.Equal(1, await verify.SaasPagoRegistros.CountAsync(x => x.EmpresaId == fixture.Company));
        Assert.Equal(1, await verify.SecurityAuditLogs.CountAsync(x => x.EmpresaId == fixture.Company && x.TipoEvento == TestDeIa.Domain.Modules.Security.Entities.SecurityAuditEventType.PagoSaasAprobado));
    }

    [SqlQuotaFact]
    public async Task RejectionDoesNotActivateAndDecisionsCannotBeReversed()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        await using var db = fixture.Open();
        var subscription = await db.SaasSuscripciones.SingleAsync(x => x.EmpresaId == fixture.Company);
        subscription.EstadoSuscripcion = EstadoSuscripcionSaas.PendientePago;
        subscription.FechaVencimiento = DateTime.UtcNow.AddDays(-1);
        var end = subscription.FechaVencimiento;
        await db.SaveChangesAsync();
        var service = Service(db, fixture.Company, actor);
        var payment = await service.RegistrarTransferenciaAsync(Request(fixture.PlanId));
        await service.RechazarPagoAsync(payment.Id, fixture.Company, "Referencia no localizada en el banco.");
        await service.RechazarPagoAsync(payment.Id, fixture.Company);
        Assert.Equal(EstadoSuscripcionSaas.PendientePago, subscription.EstadoSuscripcion);
        Assert.Equal(end, subscription.FechaVencimiento);
        await Assert.ThrowsAsync<SaasBillingException>(() => service.AprobarPagoAsync(payment.Id, fixture.Company));
    }

    [SqlQuotaFact]
    public async Task TenantAdminCannotSelfApproveAndForeignPaymentCannotBeSelected()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var owner = await PrepareAsync(fixture, SecurityRoleNames.AdministradorEmpresa);
        await using var db = fixture.Open();
        var service = Service(db, fixture.Company, owner);
        var payment = await service.RegistrarTransferenciaAsync(Request(fixture.PlanId));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AprobarPagoAsync(payment.Id, fixture.Company));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service(db, Guid.NewGuid(), owner).RegistrarTransferenciaAsync(Request(fixture.PlanId)));
        var platformAdmin = await PrepareAsync(fixture);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service(db, fixture.Company, platformAdmin).AprobarPagoAsync(payment.Id, Guid.NewGuid()));
        Assert.Equal("Pendiente", (await service.RegistrarTransferenciaAsync(Request(fixture.PlanId))).EstadoPago);
    }

    [SqlQuotaFact]
    public async Task InvalidAmountAndApprovalFailureCannotChangeSubscription()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        await using var db = fixture.Open();
        var service = Service(db, fixture.Company, actor);
        var bad = Request(fixture.PlanId); bad.Monto = 1;
        await Assert.ThrowsAsync<SaasBillingException>(() => service.RegistrarTransferenciaAsync(bad));
        var payment = await service.RegistrarTransferenciaAsync(Request(fixture.PlanId));
        var plan = await db.SaasPlanes.SingleAsync(x => x.Id == fixture.PlanId); plan.EsActivo = false;
        await db.SaveChangesAsync();
        var oldEnd = await db.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FechaVencimiento).SingleAsync();
        await Assert.ThrowsAsync<SaasBillingException>(() => service.AprobarPagoAsync(payment.Id, fixture.Company));
        db.ChangeTracker.Clear();
        Assert.Equal(EstadoPagoSaas.Pendiente, await db.SaasPagoRegistros.Where(x => x.Id == payment.Id).Select(x => x.EstadoPago).SingleAsync());
        Assert.Equal(oldEnd, await db.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FechaVencimiento).SingleAsync());
    }

    [Theory]
    [InlineData("/api/v1/saas/billing/register-transfer")]
    [InlineData("/api/v1/saas/billing/confirm-payment")]
    public async Task BillingIsReachableWithExpiredSubscriptionButOtherApisStayGuarded(string path)
    {
        var tenant = new TenantContextAccessor { EmpresaId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var reached = false;
        var middleware = new RequireActiveSubscriptionMiddleware(_ => { reached = true; return Task.CompletedTask; }, NullLogger<RequireActiveSubscriptionMiddleware>.Instance);
        var context = new DefaultHttpContext(); context.Request.Method = "POST"; context.Request.Path = path;
        context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, tenant.UserId!.Value.ToString())], "Test"));
        await middleware.InvokeAsync(context, tenant, null!); // Si intenta validar una suscripción, falla.
        Assert.True(reached);
    }

    [SqlQuotaFact]
    public async Task StorageFailureRollsBackPaymentSubscriptionAndApprovalAudit()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture);
        Guid paymentId;
        DateTime oldEnd;
        await using (var db = fixture.Open())
        {
            paymentId = (await Service(db, fixture.Company, actor).RegistrarTransferenciaAsync(Request(fixture.PlanId))).Id;
            oldEnd = await db.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FechaVencimiento).SingleAsync();
        }
        var options = new DbContextOptionsBuilder<TestDeIaDbContext>().UseSqlServer(Environment.GetEnvironmentVariable("EASYMARKET_TEST_SQL")!)
            .AddInterceptors(new FailApprovalSave()).Options;
        await using (var db = new TestDeIaDbContext(options, new TenantContextAccessor { IsSystemContext = true, EmpresaId = fixture.Company, UserId = actor }))
            await Assert.ThrowsAsync<DbUpdateException>(() => Service(db, fixture.Company, actor).AprobarPagoAsync(paymentId, fixture.Company));
        await using var verify = fixture.Open();
        Assert.Equal(EstadoPagoSaas.Pendiente, await verify.SaasPagoRegistros.Where(x => x.Id == paymentId).Select(x => x.EstadoPago).SingleAsync());
        var subscription = await verify.SaasSuscripciones.SingleAsync(x => x.EmpresaId == fixture.Company);
        Assert.Equal(oldEnd, subscription.FechaVencimiento); Assert.Null(subscription.FechaReinicioCupo);
        Assert.False(await verify.SecurityAuditLogs.AnyAsync(x => x.EmpresaId == fixture.Company && x.TipoEvento == TestDeIa.Domain.Modules.Security.Entities.SecurityAuditEventType.PagoSaasAprobado));
    }

    private sealed class FailApprovalSave : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>> SavingChangesAsync(
            Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData eventData, Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<SaasPagoRegistroEntity>().Any(x => x.Entity.EstadoPago == EstadoPagoSaas.Aprobado))
                throw new DbUpdateException("Fallo simulado de persistencia de aprobación.");
            return ValueTask.FromResult(result);
        }
    }

    private static SaasBillingService Service(TestDeIaDbContext db, Guid company, Guid actor, DateTimeOffset? now = null)
        => new(db, new SaaSQuotaValidationService(db), new TenantContextAccessor { EmpresaId = company, UserId = actor },
            NullLogger<SaasBillingService>.Instance, now.HasValue ? new FrozenClock(now.Value) : null);

    [SqlQuotaFact]
    public async Task PortalPersistsPendingDateAndNeverExposesAnotherCompany()
    {
        await using var first = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        await using var second = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(first, SecurityRoleNames.AdministradorEmpresa);
        var other = await PrepareAsync(second, SecurityRoleNames.AdministradorEmpresa);
        var request = Request(first.PlanId);
        request.FechaPago = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5).AddDays(-1));
        await using (var db = first.Open())
        {
            var response = await Service(db, first.Company, actor).RegistrarTransferenciaAsync(request);
            Assert.Equal(request.FechaPago, response.FechaPagoInformada);
        }
        await using (var db = second.Open()) await Service(db, second.Company, other).RegistrarTransferenciaAsync(Request(second.PlanId));
        await using var verify = first.Open();
        var service = Service(verify, first.Company, actor);
        var portal = await service.ObtenerPortalAsync();
        Assert.Equal(first.Company, Assert.Single(portal.PagosPendientes).EmpresaId);
        Assert.Equal(request.FechaPago, portal.PagosPendientes.Single().FechaPagoInformada);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service(verify, second.Company, actor).ObtenerPortalAsync());
        var checkout = await service.PrepararCheckoutAsync(first.PlanId);
        Assert.False(checkout.Disponible); Assert.Null(checkout.UrlPago);
        Assert.Equal(1, await verify.SaasPagoRegistros.CountAsync(x => x.EmpresaId == first.Company));
        var future = Request(first.PlanId); future.ComprobanteReferencia = "TEST-FECHA-FUTURA"; future.FechaPago = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        await Assert.ThrowsAsync<SaasBillingException>(() => service.RegistrarTransferenciaAsync(future));
    }

    [Fact]
    public async Task ExpiredSubscriptionCanReadPortalSummary()
    {
        var middleware = new RequireActiveSubscriptionMiddleware(_ => Task.CompletedTask, NullLogger<RequireActiveSubscriptionMiddleware>.Instance);
        var context = new DefaultHttpContext(); context.Request.Method = "GET"; context.Request.Path = "/api/v1/saas/billing/portal";
        context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("sub", "test")], "Test"));
        await middleware.InvokeAsync(context, new TenantContextAccessor { EmpresaId = Guid.NewGuid(), UserId = Guid.NewGuid() }, null!);
        Assert.Equal(200, context.Response.StatusCode);
    }
    private sealed class FrozenClock(DateTimeOffset now) : TimeProvider { public override DateTimeOffset GetUtcNow() => now; }
    private static RegistrarTransferenciaSaasDto Request(int plan) => new() { SaasPlanId = plan, Monto = 29.99m, ComprobanteReferencia = "TEST-BANCO-001" };
    private static async Task<Guid> PrepareAsync(SaasQuotaSqlConcurrencyTests.Fixture fixture, string roleName = SecurityRoleNames.SuperAdministrador)
    {
        await using var db = fixture.Open();
        var plan = await db.SaasPlanes.SingleAsync(x => x.Id == fixture.PlanId); plan.PrecioMensual = 29.99m;
        var userId = Guid.NewGuid();
        var roleId = await db.SecurityRoles.Where(x => x.Name == roleName).Select(x => x.Id).SingleAsync();
        var name = userId.ToString("N");
        var persona = new PersonaEntity { Id = Guid.NewGuid(), EmpresaId = fixture.Company, Identificacion = name[..20], TipoIdentificacion = "00", RazonSocialONombresCompletos = "TEST-BILLING" };
        db.SecurityUsers.Add(new SecurityUserEntity { Id = userId, EmpresaId = fixture.Company, PersonaId = persona.Id, Persona = persona,
            UserName = name, NormalizedUserName = name.ToUpperInvariant(), Email = name + "@example.invalid", NormalizedEmail = (name + "@example.invalid").ToUpperInvariant(),
            PasswordHash = "TEST-NO-LOGIN", IsActive = true, CreatedAt = DateTimeOffset.UtcNow,
            UserRoles = [new SecurityUserRoleEntity { UserId = userId, RoleId = roleId }],
            EmpresasAcceso = [new SecurityUserEmpresaEntity { SecurityUserId = userId, EmpresaId = fixture.Company, IsDefault = true, CreatedAt = DateTimeOffset.UtcNow }] });
        await db.SaveChangesAsync();
        return userId;
    }

    [SqlQuotaFact]
    public async Task LegacyAdministratorCannotApprovePaymentsAfterSuperAdminSeparation()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var actor = await PrepareAsync(fixture, SecurityRoleNames.Administrador);
        await using var db = fixture.Open();
        var service = Service(db, fixture.Company, actor);
        var payment = await service.RegistrarTransferenciaAsync(Request(fixture.PlanId));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AprobarPagoAsync(payment.Id, fixture.Company));
    }
}
