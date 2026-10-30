using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestDeIa.Api.Security;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Adapters.Out.Saas;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Saas;

public sealed class SaasPlatformRulesTests
{
    [Fact]
    public async Task OnboardingAssignsOneActiveTrialToExistingAdministratorAndDoesNotRenewItOnRetry()
    {
        await using var db = Create();
        var (company, owner, plan) = await SeedAsync(db);
        var service = Onboarding(db, plan.Id);
        var before = DateTime.UtcNow;
        await service.AsignarSuscripcionInicialAsync(company.Id, owner.Id, null);
        var sub = await db.SaasSuscripciones.SingleAsync();
        Assert.Equal(EstadoSuscripcionSaas.Activa, sub.EstadoSuscripcion);
        Assert.Equal(plan.Id, sub.SaasPlanId);
        Assert.InRange(sub.FechaVencimiento, before.AddDays(15), DateTime.UtcNow.AddDays(15));
        var deadline = sub.FechaVencimiento;
        await service.AsignarSuscripcionInicialAsync(company.Id, owner.Id, plan.Id);
        Assert.Single(await db.SaasSuscripciones.ToListAsync());
        Assert.Single(await db.SecurityAuditLogs.ToListAsync());
        Assert.Equal(deadline, sub.FechaVencimiento);
        Assert.Single(await db.SecurityUserEmpresas.Where(x => x.EmpresaId == company.Id).ToListAsync());
    }

    [Fact]
    public async Task OnboardingDoesNotAssignAPlanToAnotherOwnerOrAnInactivePlan()
    {
        await using var db = Create();
        var (company, owner, plan) = await SeedAsync(db);
        var service = Onboarding(db, plan.Id);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AsignarSuscripcionInicialAsync(company.Id, Guid.NewGuid(), plan.Id));
        plan.EsActivo = false; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AsignarSuscripcionInicialAsync(company.Id, owner.Id, plan.Id));
        Assert.Empty(await db.SaasSuscripciones.ToListAsync());
    }

    [Theory]
    [InlineData(SaasModule.Sri, false)]
    [InlineData(SaasModule.Sri, true)]
    [InlineData(SaasModule.KardexAvanzado, false)]
    [InlineData(SaasModule.KardexAvanzado, true)]
    public async Task ModuleFilterReturns403WhenPlanDoesNotIncludeModule(SaasModule module, bool allowed)
    {
        await using var db = Create();
        var (company, owner, plan) = await SeedAsync(db);
        plan.PermiteModuloSRI = allowed; plan.PermiteModuloKardexAvanzado = allowed;
        await Onboarding(db, plan.Id).AsignarSuscripcionInicialAsync(company.Id, owner.Id, null);
        var tenant = new TenantContextAccessor { EmpresaId = company.Id, UserId = owner.Id };
        var services = new ServiceCollection().AddSingleton<ITenantContextAccessor>(tenant)
            .AddSingleton<ISaaSQuotaValidationService>(new SaaSQuotaValidationService(db)).BuildServiceProvider();
        using (services)
        {
            var http = new DefaultHttpContext { RequestServices = services, User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrador")], "Test")) };
            var context = new AuthorizationFilterContext(new ActionContext(http, new RouteData(), new ActionDescriptor()), []);
            await new RequirePlanModuleAttribute(module).OnAuthorizationAsync(context);
            if (allowed) Assert.Null(context.Result);
            else
            {
                var result = Assert.IsType<ObjectResult>(context.Result);
                Assert.Equal(403, result.StatusCode);
                Assert.Equal("MODULO_NO_INCLUIDO", Assert.IsType<ProblemDetails>(result.Value).Extensions["quotaCode"]);
            }
        }
    }

    [Fact]
    public async Task PlanChangesTakeEffectWithoutRefreshingJwtAndElectronicBillingCannotBypassThem()
    {
        await using var db = Create();
        var (company, owner, plan) = await SeedAsync(db);
        await Onboarding(db, plan.Id).AsignarSuscripcionInicialAsync(company.Id, owner.Id, null);
        var quota = new SaaSQuotaValidationService(db);
        plan.PermiteModuloSRI = false; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => quota.CanEmitirFacturaAsync(company.Id));
        plan.PermiteModuloSRI = true; await db.SaveChangesAsync();
        await quota.CanEmitirFacturaAsync(company.Id);
    }

    [Fact]
    public async Task UsersAndPointsRespectLimitsIncludingUnsavedBatchAdditions()
    {
        await using var db = Create();
        var (company, owner, plan) = await SeedAsync(db);
        await Onboarding(db, plan.Id).AsignarSuscripcionInicialAsync(company.Id, owner.Id, null);
        var quota = new SaaSQuotaValidationService(db);
        var ex = await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => quota.CanCrearUsuarioAsync(company.Id));
        Assert.Equal("USUARIOS", ex.QuotaCode); // El administrador ya ocupa el asiento.
        await quota.CanCrearSucursalAsync(company.Id);
        db.EmpresaPuntosEmision.Add(new EmpresaPuntoEmisionEntity { Id = Guid.NewGuid(), EmpresaEmisoraId = company.Id, Establecimiento = "001", PuntoEmision = "001" });
        ex = await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => quota.CanCrearSucursalAsync(company.Id));
        Assert.Equal("SUCURSALES", ex.QuotaCode);
    }

    [Fact]
    public async Task ExpiredTrialOrFutureSubscriptionCannotEnableModules()
    {
        await using var db = Create();
        var (company, owner, plan) = await SeedAsync(db);
        await Onboarding(db, plan.Id).AsignarSuscripcionInicialAsync(company.Id, owner.Id, null);
        var sub = await db.SaasSuscripciones.SingleAsync();
        sub.FechaVencimiento = DateTime.UtcNow.AddSeconds(-1); await db.SaveChangesAsync();
        var quota = new SaaSQuotaValidationService(db);
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => quota.EnsureModuleAsync(company.Id, SaasModule.Sri));
        sub.FechaInicio = DateTime.UtcNow.AddDays(1); sub.FechaVencimiento = DateTime.UtcNow.AddDays(2); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<SaaSQuotaExceededException>(() => quota.CanCrearUsuarioAsync(company.Id));
    }

    private static TestDeIaDbContext Create() => new(new DbContextOptionsBuilder<TestDeIaDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new TenantContextAccessor { IsSystemContext = true, UserId = Guid.NewGuid(), EmpresaId = Guid.NewGuid() });
    private static TenantOnboardingService Onboarding(TestDeIaDbContext db, int plan) => new(db, new SaaSQuotaValidationService(db), new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Saas:DemoPlanId"] = plan.ToString() }).Build());
    private static async Task<(EmpresaEmisoraEntity, SecurityUserEntity, SaasPlanEntity)> SeedAsync(TestDeIaDbContext db)
    {
        var owner = new SecurityUserEntity { Id = Guid.NewGuid(), IsActive = true, Persona = new PersonaEntity { Id = Guid.NewGuid() } };
        owner.PersonaId = owner.Persona.Id;
        var company = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), OwnerUserId = owner.Id, Ruc = "0991111111001" };
        owner.EmpresaId = company.Id; owner.Persona.EmpresaId = company.Id;
        var plan = new SaasPlanEntity { Id = 9876, Nombre = "Prueba", EsActivo = true, LimiteUsuarios = 1, LimiteSucursales = 1, PermiteModuloSRI = true };
        db.EmpresasEmisoras.Add(company); db.SecurityUsers.Add(owner); db.SaasPlanes.Add(plan);
        db.SecurityUserEmpresas.Add(new SecurityUserEmpresaEntity { EmpresaId = company.Id, Empresa = company, SecurityUserId = owner.Id });
        await db.SaveChangesAsync(); return (company, owner, plan);
    }
}
