using System.Data;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Adapters.Out.Saas;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Saas;

public sealed class SqlQuotaFactAttribute : FactAttribute
{
    public SqlQuotaFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("EASYMARKET_TEST_SQL")))
            Skip = "Configurar EASYMARKET_TEST_SQL con una base de pruebas migrada.";
    }
}

[Collection("SaasSql")]
public sealed class SaasQuotaSqlConcurrencyTests
{
    [SqlQuotaFact]
    public async Task TwentyParallelTerminalsCannotReserveMoreThanOneRemainingSlot()
    {
        await using var fixture = await Fixture.CreateAsync();
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(async _ =>
        {
            await using var db = fixture.Open();
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                await new SaaSQuotaValidationService(db).ReservarCupoFacturaAsync(fixture.Company, Guid.NewGuid());
                await tx.CommitAsync(); return true;
            }
            catch (SaaSQuotaExceededException) { await tx.RollbackAsync(); return false; }
        }));
        Assert.Equal(1, results.Count(x => x));
        await using var verify = fixture.Open();
        Assert.Equal(1, await verify.SaasFacturaCupos.CountAsync(x => x.EmpresaId == fixture.Company && x.Estado == "RESERVADO"));
        Assert.Equal(0, await verify.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FacturasEmitidasMesActual).SingleAsync());
    }

    [SqlQuotaFact]
    public async Task TwelveConcurrentAuthorizationCallbacksCountInvoiceOnlyOnce()
    {
        await using var fixture = await Fixture.CreateAsync();
        var invoice = Guid.NewGuid();
        await using (var db = fixture.Open())
        {
            var warehouse = new BodegaEntity { Id = Guid.NewGuid(), EmpresaId = fixture.Company, Codigo = "999", Nombre = "Prueba cuotas" };
            db.Bodegas.Add(warehouse);
            db.Facturas.Add(new FacturaEntity { Id = invoice, EmpresaId = fixture.Company, BodegaId = warehouse.Id,
                Establecimiento = "999", PuntoEmision = "999", Secuencial = 1, AmbienteSri = "1", Estado = FacturaEstado.NO_FIRMADO,
                InventarioAplicado = true, ClaveAcceso = new string('1', 49), FechaEmision = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
        }
        await Task.WhenAll(Enumerable.Range(0, 12).Select(async _ =>
        {
            await using var db = fixture.Open();
            var tenant = new TenantContextAccessor { IsSystemContext = true, EmpresaId = fixture.Company, UserId = Guid.NewGuid() };
            var repository = new EfFacturacionRepository(db, tenant, null!, new SaaSQuotaValidationService(db));
            await repository.MarkFacturaAsAuthorizedAsync(invoice, new string('1', 49), "TEST-AUTORIZACION", "<firmado/>", "Prueba interna de cuota, sin envio SRI", DateTimeOffset.UtcNow);
        }));
        await using var verify = fixture.Open();
        Assert.Equal(1, await verify.SaasFacturaCupos.CountAsync(x => x.EmpresaId == fixture.Company && x.Estado == "CONFIRMADO"));
        Assert.Equal(1, await verify.SaasSuscripciones.Where(x => x.EmpresaId == fixture.Company).Select(x => x.FacturasEmitidasMesActual).SingleAsync());
        Assert.Equal(1, await verify.FacturaSriEventos.CountAsync(x => x.FacturaId == invoice));
    }

    [SqlQuotaFact]
    public async Task FailedInvoiceTransactionRollsBackItsReservation()
    {
        await using var fixture = await Fixture.CreateAsync();
        await using (var db = fixture.Open())
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            await new SaaSQuotaValidationService(db).ReservarCupoFacturaAsync(fixture.Company, Guid.NewGuid());
            await tx.RollbackAsync();
        }
        await using var verify = fixture.Open();
        Assert.Empty(await verify.SaasFacturaCupos.Where(x => x.EmpresaId == fixture.Company).ToArrayAsync());
        await using var next = await verify.Database.BeginTransactionAsync();
        await new SaaSQuotaValidationService(verify).ReservarCupoFacturaAsync(fixture.Company, Guid.NewGuid());
        await next.RollbackAsync();
    }

    internal sealed class Fixture : IAsyncDisposable
    {
        public Guid Company { get; } = Guid.NewGuid();
        private readonly int planId = Random.Shared.Next(1000000, int.MaxValue);
        public int PlanId => planId;
        public TestDeIaDbContext Open() => new(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable("EASYMARKET_TEST_SQL")!).Options,
            new TenantContextAccessor { IsSystemContext = true, EmpresaId = Company, UserId = Guid.NewGuid() });

        public static async Task<Fixture> CreateAsync(int userLimit = 0, int pointLimit = 0)
        {
            var fixture = new Fixture();
            await using var db = fixture.Open();
            await using var tx = await db.Database.BeginTransactionAsync();
            db.EmpresasEmisoras.Add(new EmpresaEmisoraEntity { Id = fixture.Company, Ruc = Random.Shared.NextInt64(1000000000000, 9999999999999).ToString(), RazonSocial = "TEST-CUOTAS-" + fixture.Company.ToString("N"), CreatedAt = DateTimeOffset.UtcNow });
            db.SaasPlanes.Add(new SaasPlanEntity { Id = fixture.planId, Nombre = "TEST-CUOTAS-" + fixture.Company.ToString("N"), LimiteFacturasMensuales = 1, LimiteUsuarios = userLimit, LimiteSucursales = pointLimit, PermiteModuloSRI = true, EsActivo = true });
            await db.SaveChangesAsync();
            db.SaasSuscripciones.Add(new SaasSuscripcionEntity { Id = Guid.NewGuid(), EmpresaId = fixture.Company, SaasPlanId = fixture.planId,
                FechaInicio = DateTime.UtcNow.AddDays(-1), FechaVencimiento = DateTime.UtcNow.AddDays(1), EstadoSuscripcion = EstadoSuscripcionSaas.Activa });
            await db.SaveChangesAsync(); await tx.CommitAsync();
            return fixture;
        }
        public async ValueTask DisposeAsync()
        {
            for (var attempt = 0; ; attempt++)
            {
                try { await CleanupAsync(); return; }
                catch (Exception ex) when (attempt < 2 && ex.GetBaseException() is Microsoft.Data.SqlClient.SqlException { Number: 1205 })
                { await Task.Delay(100); }
            }
        }
        private async Task CleanupAsync()
        {
            // Solo elimina los datos generados para el Guid exclusivo de esta prueba.
            await using var db = Open();
            await using var tx = await db.Database.BeginTransactionAsync();
            await db.SaasFacturaCupos.Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            await db.SaasPagoRegistros.IgnoreQueryFilters().Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            var ids = db.Facturas.Where(x => x.EmpresaId == Company).Select(x => x.Id);
            await db.FacturaSriEventos.Where(x => ids.Contains(x.FacturaId)).ExecuteDeleteAsync();
            await db.Facturas.Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            await db.EmpresaPuntosEmision.IgnoreQueryFilters().Where(x => x.EmpresaEmisoraId == Company).ExecuteDeleteAsync();
            await db.SecurityAuditLogs.Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            await db.SecurityUserEmpresas.IgnoreQueryFilters().Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            var users = await db.SecurityUsers.IgnoreQueryFilters().Where(x => x.EmpresaId == Company).Select(x => x.Id).ToArrayAsync();
            await db.SecurityUserRoles.IgnoreQueryFilters().Where(x => users.Contains(x.UserId)).ExecuteDeleteAsync();
            await db.SecurityUsers.IgnoreQueryFilters().Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            await db.Personas.IgnoreQueryFilters().Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            await db.Bodegas.Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            await db.SaasSuscripciones.Where(x => x.EmpresaId == Company).ExecuteDeleteAsync();
            await db.EmpresasEmisoras.IgnoreQueryFilters().Where(x => x.Id == Company).ExecuteDeleteAsync();
            await db.SaasPlanes.Where(x => x.Id == planId).ExecuteDeleteAsync();
            await tx.CommitAsync();
        }
    }
}
