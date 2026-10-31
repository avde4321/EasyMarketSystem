using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Infrastructure.Adapters.Out.Contabilidad;
using TestDeIa.Infrastructure.Adapters.Out.Empresa;
using TestDeIa.Infrastructure.Adapters.Out.Saas;
using TestDeIa.Infrastructure.Adapters.Out.Security;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Saas;

[Collection("SaasSql")]
public sealed class SaasPlatformSqlTests
{
    [SqlQuotaFact]
    public async Task ConcurrentUserCreationHonorsSeatLimitAndRollsBackRejectedPersonas()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync(userLimit: 1);
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(async _ =>
        {
            await using var db = fixture.Open();
            await using var tx = await db.Database.BeginTransactionAsync();
            try { await CreateUserAsync(db, fixture.Company); await tx.CommitAsync(); return true; }
            catch (SaaSQuotaExceededException) { await tx.RollbackAsync(); return false; }
        }));
        Assert.Equal(1, results.Count(x => x));
        await using var verify = fixture.Open();
        Assert.Equal(1, await verify.SecurityUserEmpresas.IgnoreQueryFilters().CountAsync(x => x.EmpresaId == fixture.Company));
        Assert.Equal(1, await verify.Personas.CountAsync(x => x.EmpresaId == fixture.Company));
    }

    [SqlQuotaFact]
    public async Task ConcurrentPointCreationHonorsTheSameCompanyTransactionLock()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync(pointLimit: 1);
        var results = await Task.WhenAll(Enumerable.Range(1, 12).Select(async number =>
        {
            await using var db = fixture.Open();
            await using var tx = await db.Database.BeginTransactionAsync();
            var quota = new SaaSQuotaValidationService(db);
            try
            {
                await quota.BloquearCuotaAsync(fixture.Company);
                await quota.CanCrearSucursalAsync(fixture.Company);
                db.EmpresaPuntosEmision.Add(new EmpresaPuntoEmisionEntity { Id = Guid.NewGuid(), EmpresaEmisoraId = fixture.Company, Establecimiento = "999", PuntoEmision = number.ToString("D3") });
                await db.SaveChangesAsync(); await tx.CommitAsync(); return true;
            }
            catch (SaaSQuotaExceededException) { await tx.RollbackAsync(); return false; }
        }));
        Assert.Equal(1, results.Count(x => x));
    }

    [SqlQuotaFact]
    public async Task CompanySaveRejectsAnOverLimitBatchWithoutSavingAnyPoint()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync(userLimit: 1, pointLimit: 1);
        await using var db = fixture.Open();
        var owner = await CreateUserAsync(db, fixture.Company);
        var tenant = new TenantContextAccessor { EmpresaId = fixture.Company, UserId = owner.Id, IsSystemContext = true };
        var repo = new EfEmpresaRepository(db, tenant, new CatalogoNiifSeed());
        var current = (await repo.GetByIdAsync(fixture.Company))!;
        var modified = Company(current, current.Id, current.Ruc, owner.Id,
            [new(Guid.NewGuid(), fixture.Company, "001", "001", null, true, null, null), new(Guid.NewGuid(), fixture.Company, "001", "002", null, false, null, null)]);
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => repo.SaveAsync(modified));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.EmpresaPuntosEmision.Where(x => x.EmpresaEmisoraId == fixture.Company).ToArrayAsync());
    }

    [SqlQuotaFact]
    public async Task CompanyCreationAssignsTrialAtomicallyAndInvalidPlanDoesNotLeaveOrphans()
    {
        await using var fixture = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync(userLimit: 1, pointLimit: 1);
        await using var db = fixture.Open();
        var owner = await CreateUserAsync(db, fixture.Company);
        var tenant = new TenantContextAccessor { EmpresaId = fixture.Company, UserId = owner.Id, IsSystemContext = true };
        var repo = new EfEmpresaRepository(db, tenant, new CatalogoNiifSeed());
        var current = (await repo.GetByIdAsync(fixture.Company))!;
        var validId = Guid.NewGuid(); var invalidId = Guid.NewGuid();
        try
        {
            var valid = Company(current, validId, Random.Shared.NextInt64(1000000000000, 9999999999999).ToString(), owner.Id, [], fixture.PlanId);
            await repo.SaveAsync(valid);
            var sub = await db.SaasSuscripciones.IgnoreQueryFilters().SingleAsync(x => x.EmpresaId == validId);
            Assert.Equal(fixture.PlanId, sub.SaasPlanId);
            Assert.Equal(15, (sub.FechaVencimiento - sub.FechaInicio).TotalDays);
            await repo.SaveAsync(valid);
            Assert.Equal(1, await db.SaasSuscripciones.IgnoreQueryFilters().CountAsync(x => x.EmpresaId == validId));
            var invalid = Company(current, invalidId, Random.Shared.NextInt64(1000000000000, 9999999999999).ToString(), owner.Id, [], int.MaxValue);
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.SaveAsync(invalid));
            db.ChangeTracker.Clear();
            Assert.False(await db.EmpresasEmisoras.IgnoreQueryFilters().AnyAsync(x => x.Id == invalidId));
            Assert.False(await db.SecurityUserEmpresas.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == invalidId));
        }
        finally
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync();
            await db.SecurityAuditLogs.IgnoreQueryFilters().Where(x => x.EmpresaId == validId).ExecuteDeleteAsync();
            await db.CuentasContables.IgnoreQueryFilters().Where(x => x.EmpresaId == validId).ExecuteDeleteAsync();
            await db.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == validId).ExecuteDeleteAsync();
            await db.SecurityUserEmpresas.IgnoreQueryFilters().Where(x => x.EmpresaId == validId).ExecuteDeleteAsync();
            await db.EmpresasEmisoras.IgnoreQueryFilters().Where(x => x.Id == validId).ExecuteDeleteAsync();
            await tx.CommitAsync();
        }
    }

    private static EmpresaEmisora Company(EmpresaEmisora c, Guid id, string ruc, Guid owner, IReadOnlyCollection<EmpresaPuntoEmision> points, int? plan = null) => new(id, owner, "TEST-ONBOARD-" + id.ToString("N"), null, ruc,
        "Guayaquil", null, null, null, null, null, "001", "001", "1", true, "1", false, null, null, null, null, null, null, points, true, DateTimeOffset.UtcNow, null) { PlanInicialId = plan };

    private static async Task<SecurityUser> CreateUserAsync(TestDeIaDbContext db, Guid company)
    {
        var id = Guid.NewGuid();
        var persona = new PersonaEntity { Id = Guid.NewGuid(), EmpresaId = company, Identificacion = id.ToString("N")[..13], RazonSocialONombresCompletos = "TEST-USUARIO", TipoIdentificacion = "04" };
        db.Personas.Add(persona); await db.SaveChangesAsync();
        var user = new SecurityUser(id, company, persona.Id, persona.Identificacion, "test-" + id.ToString("N"), "TEST-USUARIO", id.ToString("N") + "@example.invalid", "TEST-NO-LOGIN", [], [], [], [], true, DateTimeOffset.UtcNow, 0, null, null, false, null);
        var tenant = new TenantContextAccessor { EmpresaId = company, UserId = id, IsSystemContext = true };
        return await new EfSecurityUserRepository(db, tenant, new SaaSQuotaValidationService(db)).CreateAsync(user);
    }
}
