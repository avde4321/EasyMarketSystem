using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestDeIa.Api.Controllers;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Security.Models;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Saas.Enums;
using TestDeIa.Infrastructure.Adapters.Out.Saas;
using TestDeIa.Infrastructure.Adapters.Out.Security;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Tests.Saas;

[Collection("SaasSql")]
public sealed class PublicTenantRegistrationTests
{
    private static IConfiguration Configuration => new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string, string?> { ["Security:Jwt:Secret"] = "TEST-ONLY-" + new string('X', 48) }).Build();

    [Theory]
    [InlineData("123456789012")]
    [InlineData("12345678901234")]
    [InlineData("123456789012A")]
    public void RucMustBeExactlyThirteenAsciiDigits(string ruc)
    {
        var dto = Request(1); dto.Ruc = ruc;
        Assert.Throws<ValidationException>(() => Validator.ValidateObject(dto, new ValidationContext(dto), true));
    }

    [Fact]
    public void PublicOwnerNeverGetsPlatformCatalogPermissions()
    {
        Assert.DoesNotContain(SecurityPermissions.CatalogosAdministrar, SecurityPermissionCatalog.RoleMappings[SecurityRoleNames.AdministradorEmpresa]);
        Assert.Contains(SecurityPermissions.SeguridadUsuarios, SecurityPermissionCatalog.RoleMappings[SecurityRoleNames.AdministradorEmpresa]);
        var method = typeof(SaasPublicController).GetMethod(nameof(SaasPublicController.Register))!;
        Assert.NotNull(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).SingleOrDefault());
        Assert.Equal(SecurityRateLimitPolicyNames.PublicRegistration, ((EnableRateLimitingAttribute)method.GetCustomAttributes(typeof(EnableRateLimitingAttribute), false).Single()).PolicyName);
    }

    [SqlQuotaFact]
    public async Task DemoRegistrationCreatesAllRecordsAndUsableLoginAtomically()
    {
        await using var plan = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync(userLimit: 1, pointLimit: 1);
        var dto = Request(plan.PlanId);
        try
        {
            await using var db = Open();
            var response = await Service(db).RegistrarPublicoAsync(dto);
            Assert.True(response.Succeeded);
            var companyId = response.ActiveEmpresaId!.Value;
            var company = await db.EmpresasEmisoras.IgnoreQueryFilters().SingleAsync(x => x.Id == companyId);
            var user = await db.SecurityUsers.IgnoreQueryFilters().SingleAsync(x => x.Id == company.OwnerUserId);
            Assert.NotEqual(dto.Password, user.PasswordHash);
            Assert.True(new Sha256PasswordHashService().Verify(dto.Password, user.PasswordHash).Succeeded);
            Assert.Equal(companyId, user.EmpresaId);
            Assert.True(await db.SecurityUserEmpresas.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == companyId && x.SecurityUserId == user.Id && x.IsDefault));
            var subscription = await db.SaasSuscripciones.IgnoreQueryFilters().SingleAsync(x => x.EmpresaId == companyId);
            Assert.Equal(EstadoSuscripcionSaas.Demo, subscription.EstadoSuscripcion);
            Assert.Equal(TimeSpan.FromDays(15), subscription.FechaVencimiento - subscription.FechaInicio);
            var point = await db.EmpresaPuntosEmision.IgnoreQueryFilters().SingleAsync(x => x.EmpresaEmisoraId == companyId);
            Assert.Equal("001", point.Establecimiento); Assert.Equal("001", point.PuntoEmision);
            Assert.Equal("Caja General", point.NombreCaja); Assert.Null(point.BodegaId);
            Assert.False(await db.CajaSesiones.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == companyId));
            Assert.False(await db.Bodegas.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == companyId));
            var counters = await db.Set<FacturaSecuencialEntity>().IgnoreQueryFilters().Where(x => x.EmpresaId == companyId).ToArrayAsync();
            Assert.Equal(4, counters.Length); Assert.All(counters, x => Assert.Equal(0, x.UltimoSecuencial));
            Assert.True(await db.CuentasContables.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == companyId && x.Nombre == "Caja General"));
            await new SaaSQuotaValidationService(db).EnsureActiveSubscriptionAsync(companyId);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
            Assert.Contains(jwt.Claims, x => x.Type == "default_empresa_id" && x.Value == companyId.ToString());
            var loginUser = await new EfSecurityUserRepository(db, new TenantContextAccessor()).FindByUserNameAsync(dto.Email);
            Assert.NotNull(loginUser); Assert.Contains(SecurityRoleNames.AdministradorEmpresa, loginUser.Roles);
            var tenant = new TenantContextAccessor { EmpresaId = companyId, UserId = user.Id };
            var repository = new EfSecurityUserRepository(db, tenant);
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdatePerfilAsync(user.Id, [SecurityRoleNames.Administrador], null));
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdatePerfilAsync(user.Id, [SecurityRoleNames.Gerente], null));
        }
        finally { await Cleanup(dto); }
    }

    [SqlQuotaFact]
    public async Task PaidRegistrationRemainsPendingAndCannotOperate()
    {
        await using var plan = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var dto = Request(plan.PlanId); dto.EsDemo = false;
        try
        {
            await using var db = Open();
            var response = await Service(db).RegistrarPublicoAsync(dto);
            var subscription = await db.SaasSuscripciones.IgnoreQueryFilters().SingleAsync(x => x.EmpresaId == response.ActiveEmpresaId);
            Assert.Equal(EstadoSuscripcionSaas.PendientePago, subscription.EstadoSuscripcion);
            Assert.Equal(TimeSpan.FromDays(3), subscription.FechaVencimiento - subscription.FechaInicio);
            await Assert.ThrowsAsync<SubscriptionRequiredException>(() => new SaaSQuotaValidationService(db).EnsureActiveSubscriptionAsync(response.ActiveEmpresaId!.Value));
        }
        finally { await Cleanup(dto); }
    }

    [SqlQuotaFact]
    public async Task TokenFailureRollsBackCompanyUserSubscriptionAndSeeds()
    {
        await using var plan = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var dto = Request(plan.PlanId);
        var failure = new FailingTokenGenerator();
        try
        {
            await using (var db = Open())
                await Assert.ThrowsAsync<InvalidOperationException>(() => Service(db, failure).RegistrarPublicoAsync(dto));
            await using var verify = Open();
            Assert.False(await verify.EmpresasEmisoras.IgnoreQueryFilters().AnyAsync(x => x.Ruc == dto.Ruc));
            Assert.False(await verify.SecurityUsers.IgnoreQueryFilters().AnyAsync(x => x.NormalizedEmail == dto.Email.ToUpperInvariant()));
            Assert.False(await verify.Personas.IgnoreQueryFilters().AnyAsync(x => x.CorreoElectronicoPrincipal == dto.Email));
            Assert.NotNull(failure.CompanyId);
            Assert.False(await verify.SaasSuscripciones.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == failure.CompanyId));
            Assert.False(await verify.EmpresaPuntosEmision.IgnoreQueryFilters().AnyAsync(x => x.EmpresaEmisoraId == failure.CompanyId));
            Assert.False(await verify.Set<FacturaSecuencialEntity>().IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == failure.CompanyId));
            Assert.False(await verify.CuentasContables.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == failure.CompanyId));
            Assert.False(await verify.SecurityAuditLogs.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == failure.CompanyId));
        }
        finally { await Cleanup(dto); }
    }

    [SqlQuotaFact]
    public async Task ConcurrentDuplicateRegistrationCreatesOnlyOneCompany()
    {
        await using var plan = await SaasQuotaSqlConcurrencyTests.Fixture.CreateAsync();
        var dto = Request(plan.PlanId);
        try
        {
            var outcomes = await Task.WhenAll(Enumerable.Range(0, 4).Select(async _ =>
            {
                await using var db = Open();
                try { await Service(db).RegistrarPublicoAsync(dto); return true; }
                catch (PublicTenantRegistrationException) { return false; }
            }));
            Assert.Equal(1, outcomes.Count(x => x));
            await using var verify = Open();
            Assert.Equal(1, await verify.EmpresasEmisoras.IgnoreQueryFilters().CountAsync(x => x.Ruc == dto.Ruc));
            Assert.Equal(1, await verify.SecurityUsers.IgnoreQueryFilters().CountAsync(x => x.NormalizedEmail == dto.Email.ToUpperInvariant()));
            var changedRuc = Request(plan.PlanId); changedRuc.Email = dto.Email.ToUpperInvariant();
            await Assert.ThrowsAsync<PublicTenantRegistrationException>(() => Service(verify).RegistrarPublicoAsync(changedRuc));
            Assert.False(await verify.EmpresasEmisoras.IgnoreQueryFilters().AnyAsync(x => x.Ruc == changedRuc.Ruc));
            var changedEmail = Request(plan.PlanId); changedEmail.Ruc = dto.Ruc;
            await Assert.ThrowsAsync<PublicTenantRegistrationException>(() => Service(verify).RegistrarPublicoAsync(changedEmail));
            Assert.False(await verify.SecurityUsers.IgnoreQueryFilters().AnyAsync(x => x.NormalizedEmail == changedEmail.Email.ToUpperInvariant()));
        }
        finally { await Cleanup(dto); }
    }

    private static TenantOnboardingService Service(TestDeIaDbContext db, ISecurityTokenGenerator? tokens = null) =>
        new(db, new SaaSQuotaValidationService(db), Configuration, new Sha256PasswordHashService(), tokens ?? new JwtSecurityTokenGenerator(Configuration));
    private static TestDeIaDbContext Open() => new(new DbContextOptionsBuilder<TestDeIaDbContext>()
        .UseSqlServer(Environment.GetEnvironmentVariable("EASYMARKET_TEST_SQL")!).Options,
        new TenantContextAccessor { IsSystemContext = true, EmpresaId = Guid.NewGuid(), UserId = Guid.NewGuid() });
    private static PublicTenantRegisterDto Request(int planId) => new()
    {
        NombreCompleto = "Propietario de prueba", Email = Guid.NewGuid().ToString("N") + "@example.invalid", Password = "Registro-Seguro123!",
        Telefono = "+593991234567", Ruc = Random.Shared.NextInt64(1000000000000, 9999999999999).ToString(),
        RazonSocial = "TEST-REGISTRO-PUBLICO", Direccion = "Dirección de prueba", PlanId = planId, EsDemo = true
    };
    private sealed class FailingTokenGenerator : ISecurityTokenGenerator
    {
        public Guid? CompanyId { get; private set; }
        public GeneratedToken Generate(AuthenticatedUser user)
        {
            CompanyId = user.DefaultEmpresaId;
            throw new InvalidOperationException("Fallo simulado después del guardado y antes del commit.");
        }
    }
    private static async Task Cleanup(PublicTenantRegisterDto dto)
    {
        await using var db = Open();
        var ids = await db.EmpresasEmisoras.IgnoreQueryFilters().Where(x => x.Ruc == dto.Ruc && x.RazonSocial == "TEST-REGISTRO-PUBLICO").Select(x => x.Id).ToArrayAsync();
        foreach (var id in ids)
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            await db.Set<SecurityUserPuntoEmisionEntity>().IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            await db.EmpresaPuntosEmision.IgnoreQueryFilters().Where(x => x.EmpresaEmisoraId == id).ExecuteDeleteAsync();
            await db.Set<FacturaSecuencialEntity>().IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            await db.CuentasContables.IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            await db.SecurityAuditLogs.IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            await db.SecurityUserEmpresas.IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            var users = db.SecurityUsers.IgnoreQueryFilters().Where(x => x.EmpresaId == id).Select(x => x.Id);
            await db.SecurityUserRoles.IgnoreQueryFilters().Where(x => users.Contains(x.UserId)).ExecuteDeleteAsync();
            await db.SecurityUsers.IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            await db.Personas.IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            await db.SaasSuscripciones.IgnoreQueryFilters().Where(x => x.EmpresaId == id).ExecuteDeleteAsync();
            await db.EmpresasEmisoras.IgnoreQueryFilters().Where(x => x.Id == id).ExecuteDeleteAsync();
            await tx.CommitAsync();
        }
    }
}
