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
    public async Task IncrementarFacturasEmitidasMesActualAsync_ResetsCounterWhenMonthChanged()
    {
        var empresaId = Guid.NewGuid();
        await using var dbContext = CreateContext();
        SeedSubscription(dbContext, empresaId, EstadoSuscripcionSaas.Activa, invoiceLimit: 100, emitted: 25, period: DateTime.UtcNow.AddMonths(-1));
        await dbContext.SaveChangesAsync();

        var service = new SaaSQuotaValidationService(dbContext);

        await service.IncrementarFacturasEmitidasMesActualAsync(empresaId);

        var subscription = await dbContext.SaasSuscripciones.IgnoreQueryFilters().SingleAsync(current => current.EmpresaId == empresaId);
        Assert.Equal(1, subscription.FacturasEmitidasMesActual);
        Assert.Equal(DateTime.UtcNow.Month, subscription.ContadorFacturasPeriodo?.Month);
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
