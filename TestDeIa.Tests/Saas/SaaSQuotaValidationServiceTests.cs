using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Adapters.Out.Saas;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Saas;

public sealed class SaaSQuotaValidationServiceTests
{
    [Fact]
    public async Task RetryKeepsOneReservationAndDoesNotCountAsAuthorized()
    {
        var empresa = Guid.NewGuid(); var id = Guid.NewGuid();
        await using var db = CreateContext();
        SeedSubscription(db, empresa, EstadoSuscripcionSaas.Activa, 1, 0);
        await db.SaveChangesAsync();
        var service = new SaaSQuotaValidationService(db);
        await service.ReservarCupoFacturaAsync(empresa, id);
        await service.ReservarCupoFacturaAsync(empresa, id);
        Assert.Single(await db.SaasFacturaCupos.ToListAsync());
        Assert.Equal(0, (await db.SaasSuscripciones.SingleAsync()).FacturasEmitidasMesActual);
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => service.ReservarCupoFacturaAsync(empresa, Guid.NewGuid()));
    }

    [Fact]
    public async Task RejectionReleasesReservationButConfirmationRequiresAuthorization()
    {
        var empresa = Guid.NewGuid(); var id = Guid.NewGuid();
        await using var db = CreateContext();
        SeedSubscription(db, empresa, EstadoSuscripcionSaas.Activa, 1, 0);
        var invoice = new FacturaEntity { Id = id, EmpresaId = empresa, Estado = TestDeIa.Domain.Modules.Facturacion.Entities.FacturaEstado.PENDIENTE };
        db.Facturas.Add(invoice); await db.SaveChangesAsync();
        var service = new SaaSQuotaValidationService(db);
        await service.ReservarCupoFacturaAsync(empresa, id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConfirmarFacturaAutorizadaAsync(empresa, id));
        invoice.Estado = TestDeIa.Domain.Modules.Facturacion.Entities.FacturaEstado.RECHAZADO;
        await db.SaveChangesAsync();
        await service.LiberarCupoFacturaAsync(empresa, id);
        await service.LiberarCupoFacturaAsync(empresa, id);
        await service.ReservarCupoFacturaAsync(empresa, Guid.NewGuid());
        Assert.Equal(1, await db.SaasFacturaCupos.CountAsync(x => x.Estado == "RESERVADO"));
        Assert.Equal(0, (await db.SaasSuscripciones.SingleAsync()).FacturasEmitidasMesActual);
    }

    [Fact]
    public async Task OldPendingReservationsStillProtectCurrentMonthCapacity()
    {
        var empresa = Guid.NewGuid();
        await using var db = CreateContext();
        SeedSubscription(db, empresa, EstadoSuscripcionSaas.Activa, 1, 20, DateTime.UtcNow.AddMonths(-1));
        db.SaasFacturaCupos.Add(new SaasFacturaCupoEntity { FacturaId = Guid.NewGuid(), EmpresaId = empresa, FechaReserva = DateTimeOffset.UtcNow.AddMonths(-1) });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => new SaaSQuotaValidationService(db).CanEmitirFacturaAsync(empresa));
    }

    [Fact]
    public async Task ReservationCannotBeReusedByAnotherTenant()
    {
        var empresa = Guid.NewGuid(); var id = Guid.NewGuid();
        await using var db = CreateContext();
        SeedSubscription(db, empresa, EstadoSuscripcionSaas.Activa, 1, 0);
        await db.SaveChangesAsync();
        var service = new SaaSQuotaValidationService(db);
        await service.ReservarCupoFacturaAsync(empresa, id);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ReservarCupoFacturaAsync(Guid.NewGuid(), id));

        var reservation = await db.SaasFacturaCupos.IgnoreQueryFilters().SingleAsync(x => x.FacturaId == id);
        Assert.Equal(empresa, reservation.EmpresaId);
        Assert.Equal("RESERVADO", reservation.Estado);
        Assert.Single(await db.SaasFacturaCupos.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task CanEmitirFacturaAsync_BlocksWhenMonthlyInvoiceLimitIsReached()
    {
        var empresaId = Guid.NewGuid();
        await using var dbContext = CreateContext();
        SeedSubscription(dbContext, empresaId, EstadoSuscripcionSaas.Activa, invoiceLimit: 1, emitted: 1);
        await dbContext.SaveChangesAsync();

        var service = new SaaSQuotaValidationService(dbContext);

        var exception = await Assert.ThrowsAsync<SaaSQuotaExceededException>(() =>
            service.CanEmitirFacturaAsync(empresaId));

        Assert.Equal("FACTURAS_MENSUALES", exception.QuotaCode);
    }

    [Fact]
    public async Task CanEmitirFacturaAsync_BlocksWhenSubscriptionIsSuspended()
    {
        var empresaId = Guid.NewGuid();
        await using var dbContext = CreateContext();
        SeedSubscription(dbContext, empresaId, EstadoSuscripcionSaas.Suspendida, invoiceLimit: 100, emitted: 0);
        await dbContext.SaveChangesAsync();

        var service = new SaaSQuotaValidationService(dbContext);

        var exception = await Assert.ThrowsAsync<SaaSQuotaExceededException>(() =>
            service.CanEmitirFacturaAsync(empresaId));

        Assert.Equal("SUSCRIPCION_INACTIVA", exception.QuotaCode);
    }

    [Fact]
    public async Task ConfirmarFacturaAutorizadaAsync_ResetsCounterWhenMonthChangedAndIsIdempotent()
    {
        var empresaId = Guid.NewGuid();
        await using var dbContext = CreateContext();
        SeedSubscription(dbContext, empresaId, EstadoSuscripcionSaas.Activa, invoiceLimit: 100, emitted: 25, period: DateTime.UtcNow.AddMonths(-1));
        await dbContext.SaveChangesAsync();

        var service = new SaaSQuotaValidationService(dbContext);
        var factura = new FacturaEntity { Id = Guid.NewGuid(), EmpresaId = empresaId, Estado = TestDeIa.Domain.Modules.Facturacion.Entities.FacturaEstado.AUTORIZADO, FechaAutorizacion = DateTimeOffset.UtcNow };
        dbContext.Facturas.Add(factura);
        await dbContext.SaveChangesAsync();
        await service.ConfirmarFacturaAutorizadaAsync(empresaId, factura.Id);
        await service.ConfirmarFacturaAutorizadaAsync(empresaId, factura.Id);

        var subscription = await dbContext.SaasSuscripciones.IgnoreQueryFilters().SingleAsync(current => current.EmpresaId == empresaId);
        Assert.Equal(1, subscription.FacturasEmitidasMesActual);
        Assert.Equal(DateTime.UtcNow.Month, subscription.ContadorFacturasPeriodo?.Month);
        Assert.Single(await dbContext.SaasFacturaCupos.ToListAsync());
    }

    [Fact]
    public async Task BillingResetDoesNotEraseHistoryOrReleasePendingReservations()
    {
        var company = Guid.NewGuid();
        await using var db = CreateContext();
        SeedSubscription(db, company, EstadoSuscripcionSaas.Activa, 1, 0);
        await db.SaveChangesAsync();
        var reset = DateTimeOffset.UtcNow;
        (await db.SaasSuscripciones.SingleAsync()).FechaReinicioCupo = reset;
        db.SaasFacturaCupos.Add(new SaasFacturaCupoEntity { EmpresaId = company, FacturaId = Guid.NewGuid(), Estado = "CONFIRMADO",
            FechaConfirmacion = reset.AddDays(-1), PeriodoAutorizacion = new DateTime(reset.Year, reset.Month, 1) });
        await db.SaveChangesAsync();
        var service = new SaaSQuotaValidationService(db);
        await service.CanEmitirFacturaAsync(company);
        db.SaasFacturaCupos.Add(new SaasFacturaCupoEntity { EmpresaId = company, FacturaId = Guid.NewGuid(), Estado = "RESERVADO", FechaReserva = reset.AddDays(-1) });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => service.CanEmitirFacturaAsync(company));
        Assert.Equal(2, await db.SaasFacturaCupos.CountAsync());
    }

    private static TestDeIaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TestDeIaDbContext(options, new TenantContextAccessor { IsSystemContext = true });
    }

    private static void SeedSubscription(
        TestDeIaDbContext dbContext,
        Guid empresaId,
        EstadoSuscripcionSaas estado,
        int invoiceLimit,
        int emitted,
        DateTime? period = null)
    {
        var plan = new SaasPlanEntity
        {
            Id = Random.Shared.Next(10_000, 99_999),
            Nombre = $"Plan-{Guid.NewGuid():N}",
            LimiteFacturasMensuales = invoiceLimit,
            LimiteUsuarios = 10,
            LimiteSucursales = 2,
            PermiteModuloSRI = true,
            PermiteModuloKardexAvanzado = true,
            PrecioMensual = 10m,
            EsActivo = true
        };

        dbContext.SaasPlanes.Add(plan);
        dbContext.SaasSuscripciones.Add(new SaasSuscripcionEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            SaasPlanId = plan.Id,
            FechaInicio = DateTime.UtcNow.AddMonths(-1),
            FechaVencimiento = DateTime.UtcNow.AddMonths(1),
            EstadoSuscripcion = estado,
            FacturasEmitidasMesActual = emitted,
            ContadorFacturasPeriodo = period ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
