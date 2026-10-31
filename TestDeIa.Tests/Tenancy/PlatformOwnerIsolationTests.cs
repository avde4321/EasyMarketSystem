using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Api.Middleware;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Shared.Security;

namespace TestDeIa.Tests.Tenancy;

public sealed class PlatformOwnerIsolationTests
{
    [Theory]
    [InlineData(true, false, 403)]
    [InlineData(true, true, 200)]
    [InlineData(false, false, 200)]
    public async Task TenantSelectionRequiresOwnershipOnlyForPlatformAdministrator(bool root, bool own, int expected)
    {
        var tenant = new TenantContextAccessor { IsSystemContext = true };
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, tenant);
        var actor = Guid.NewGuid();
        var company = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), OwnerUserId = own ? actor : Guid.NewGuid() };
        db.EmpresasEmisoras.Add(company);
        db.CertificadosDigitalesEmpresa.Add(new CertificadoDigitalEmpresaEntity { Id = Guid.NewGuid(), EmpresaId = company.Id, Empresa = company });
        db.EmpresaPuntosEmision.Add(new EmpresaPuntoEmisionEntity { Id = Guid.NewGuid(), EmpresaEmisoraId = company.Id, Empresa = company });
        var user = new SecurityUserEntity { Id = actor, EmpresaId = company.Id, IsActive = true };
        var role = new SecurityRoleEntity { Id = Guid.NewGuid(), Name = root ? SecurityRoleNames.SuperAdministrador : SecurityRoleNames.Administrador, IsActive = true };
        db.SecurityUserRoles.Add(new SecurityUserRoleEntity { UserId = actor, User = user, RoleId = role.Id, Role = role });
        db.SecurityUserEmpresas.Add(new SecurityUserEmpresaEntity { SecurityUserId = actor, EmpresaId = company.Id, Empresa = company });
        await db.SaveChangesAsync();
        tenant.IsSystemContext = false;
        var http = Context(actor, "/api/dashboard");
        http.Request.Headers["X-Empresa-Id"] = company.Id.ToString();
        var called = false;
        await new TenantResolutionMiddleware(_ => { called = true; return Task.CompletedTask; }).InvokeAsync(http, tenant, db);
        Assert.Equal(expected, http.Response.StatusCode);
        Assert.Equal(expected == 200, called);
        if (called) Assert.Equal(company.Id, tenant.EmpresaId);
        if (root)
        {
            Assert.Equal(own ? 1 : 0, await db.EmpresasEmisoras.AsNoTracking().CountAsync());
            Assert.Equal(own ? 1 : 0, await db.CertificadosDigitalesEmpresa.AsNoTracking().CountAsync());
            Assert.Equal(own ? 1 : 0, await db.EmpresaPuntosEmision.AsNoTracking().CountAsync());
            var repository = new TestDeIa.Infrastructure.Adapters.Out.Empresa.EfEmpresaRepository(db, tenant,
                new TestDeIa.Infrastructure.Adapters.Out.Contabilidad.CatalogoNiifSeed());
            var companies = await repository.GetMineAsync();
            Assert.Equal(own ? 1 : 0, companies.Count);
        }
    }

    [Theory]
    [InlineData("/api/dashboard", false)]
    [InlineData("/api/clientes", false)]
    [InlineData("/api/v1/saas/billing/admin/dashboard", true)]
    [InlineData("/api/v1/saas/billing/admin/payments", true)]
    [InlineData("/api/v1/saas/billing/admin/consumption", true)]
    public async Task PlatformWithoutOwnCompanyCannotReadErpEvenWithLegacyToken(string path, bool allowed)
    {
        var actor = Guid.NewGuid();
        var tenant = new TenantContextAccessor { IsSystemContext = true, UserId = actor };
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, tenant);
        db.SecurityUserRoles.Add(new SecurityUserRoleEntity
        {
            UserId = actor, User = new SecurityUserEntity { Id = actor, IsActive = true },
            RoleId = Guid.NewGuid(), Role = new SecurityRoleEntity { Id = Guid.NewGuid(), Name = SecurityRoleNames.SuperAdministrador, IsActive = true }
        });
        await db.SaveChangesAsync();
        tenant.IsSystemContext = false;
        var http = Context(actor, path);
        var called = false;
        await new PlatformOwnerBoundaryMiddleware(_ => { called = true; return Task.CompletedTask; }).InvokeAsync(http, tenant, db);
        Assert.Equal(allowed, called);
        Assert.Equal(allowed ? 200 : 403, http.Response.StatusCode);
    }

    private static DefaultHttpContext Context(Guid actor, string path)
    {
        var context = new DefaultHttpContext();
        // Sin rol JWT: la restricción se determina también desde la asignación real en BD.
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, actor.ToString())], "test"));
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }
}
