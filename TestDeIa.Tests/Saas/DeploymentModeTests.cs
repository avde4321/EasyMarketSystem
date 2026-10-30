using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TestDeIa.Api.Middleware;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Adapters.Out.Saas;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Saas;

public sealed class DeploymentModeTests
{
    [Theory]
    [InlineData("OnPremise", true, true)]
    [InlineData("SaaS", false, false)]
    public async Task DisabledRestrictionsPermitOperationsWithoutSubscription(string mode, bool subscription, bool quotas)
    {
        var settings = Options.Create(new DeploymentSettingsOptions { Mode = mode, EnforceSubscription = subscription, EnforceQuotas = quotas });
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new TenantContextAccessor { IsSystemContext = true });
        var service = new SaaSQuotaValidationService(db, settings);
        var empresa = Guid.NewGuid();
        await service.EnsureActiveSubscriptionAsync(empresa);
        await service.CanEmitirFacturaAsync(empresa);
        await service.CanCrearUsuarioAsync(empresa);
        await service.CanCrearSucursalAsync(empresa);
        await service.ReservarCupoFacturaAsync(empresa, Guid.NewGuid());
        await service.ConfirmarFacturaAutorizadaAsync(empresa, Guid.NewGuid());
        Assert.Empty(await db.SaasFacturaCupos.ToListAsync());
        if (mode == "OnPremise") await service.EnsureModuleAsync(empresa, SaasModule.Sri);
        var called = false;
        var middleware = new RequireActiveSubscriptionMiddleware(_ => { called = true; return Task.CompletedTask; },
            NullLogger<RequireActiveSubscriptionMiddleware>.Instance, settings);
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "test"));
        context.Request.Path = "/api/facturacion/monitor";
        await middleware.InvokeAsync(context, new TenantContextAccessor { EmpresaId = empresa }, service);
        Assert.True(called);
    }

    [Fact]
    public async Task DefaultModeRemainsStrictSaas()
    {
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new TenantContextAccessor { IsSystemContext = true });
        await Assert.ThrowsAsync<SubscriptionRequiredException>(() => new SaaSQuotaValidationService(db).EnsureActiveSubscriptionAsync(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(false, false, 200)]
    [InlineData(true, false, 409)]
    [InlineData(false, true, 403)]
    public async Task LocalResolutionRequiresUniqueCompanyAndMembership(bool ambiguous, bool foreignHeader, int expected)
    {
        var company = Guid.NewGuid(); var user = Guid.NewGuid();
        var tenant = new TenantContextAccessor();
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new TenantContextAccessor { IsSystemContext = true });
        db.EmpresasEmisoras.Add(new() { Id = company, IsActive = true });
        if (ambiguous) db.EmpresasEmisoras.Add(new() { Id = Guid.NewGuid(), IsActive = true });
        db.SecurityUserEmpresas.Add(new() { EmpresaId = company, SecurityUserId = user });
        await db.SaveChangesAsync();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.ToString())], "test"));
        if (foreignHeader) context.Request.Headers["X-Empresa-Id"] = Guid.NewGuid().ToString();
        var called = false;
        var middleware = new TenantResolutionMiddleware(_ => { called = true; return Task.CompletedTask; },
            Options.Create(new DeploymentSettingsOptions { Mode = "OnPremise" }));
        await middleware.InvokeAsync(context, tenant, db);
        Assert.Equal(expected, context.Response.StatusCode);
        Assert.Equal(expected == 200, called);
        Assert.False(tenant.IsSystemContext);
        if (called) Assert.Equal(company, tenant.EmpresaId);
    }
}
