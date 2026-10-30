using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TestDeIa.Infrastructure.Adapters.Out.Security;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Shared.Security;

namespace TestDeIa.Tests.Saas;

public sealed class SuperAdminBootstrapTests
{
    private static TestDeIaDbContext Open() => new(new DbContextOptionsBuilder<TestDeIaDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options,
        new TenantContextAccessor { IsSystemContext = true, UserId = Guid.NewGuid(), EmpresaId = Guid.NewGuid() });
    private static IConfiguration Config(Guid company, string password = "Root-TEST-only-Password123!") => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Saas:SuperAdminBootstrap:Enabled"] = "true", ["Saas:SuperAdminBootstrap:UserName"] = "sudo-test",
        ["Saas:SuperAdminBootstrap:Email"] = "root@example.invalid", ["Saas:SuperAdminBootstrap:Password"] = password,
        ["Saas:SuperAdminBootstrap:EmpresaId"] = company.ToString()
    }).Build();
    private static async Task<Guid> SeedAsync(TestDeIaDbContext db)
    {
        var company = Guid.NewGuid();
        db.EmpresasEmisoras.Add(new EmpresaEmisoraEntity { Id = company, IsActive = true });
        db.SecurityRoles.Add(new SecurityRoleEntity { Id = Guid.NewGuid(), Name = SecurityRoleNames.SuperAdministrador,
            NormalizedName = SecurityRoleNames.SuperAdministrador.ToUpperInvariant(), IsActive = true });
        await db.SaveChangesAsync(); return company;
    }
    private static SaasSuperAdminBootstrapper Service(TestDeIaDbContext db, IConfiguration config) => new(db, config, new Sha256PasswordHashService(), NullLogger<SaasSuperAdminBootstrapper>.Instance);

    [Fact]
    public async Task CreatesExactlyOneProtectedOwnerWithHashedPasswordAndInstallationMarker()
    {
        await using var db = Open(); var company = await SeedAsync(db);
        var config = Config(company);
        Assert.True(await Service(db, config).EnsureAsync());
        var user = await db.SecurityUsers.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("sudo-test", user.UserName); Assert.NotEqual(config["Saas:SuperAdminBootstrap:Password"], user.PasswordHash);
        Assert.True(new Sha256PasswordHashService().Verify(config["Saas:SuperAdminBootstrap:Password"]!, user.PasswordHash).Succeeded);
        Assert.Equal(user.Id, (await db.Set<SaasInicializacionEntity>().SingleAsync()).UsuarioId);
        Assert.Equal(company, (await db.SecurityUserEmpresas.IgnoreQueryFilters().SingleAsync()).EmpresaId);
        var hash = user.PasswordHash;
        user.IsActive = false; user.BloqueadoManualmente = true; await db.SaveChangesAsync();
        Assert.False(await Service(db, Config(company, "Another-TEST-password123!")).EnsureAsync());
        Assert.Equal(hash, user.PasswordHash); Assert.False(user.IsActive); Assert.True(user.BloqueadoManualmente);
        Assert.Equal(1, await db.SecurityUsers.IgnoreQueryFilters().CountAsync());
        Assert.DoesNotContain(SecurityRoleNames.SuperAdministrador, SecurityRoleNames.All);
        var repository = new EfSecurityUserRepository(db, new TenantContextAccessor { EmpresaId = company, UserId = user.Id, IsSystemContext = true });
        Assert.DoesNotContain(await repository.GetRolesAsync(), x => x.Name == SecurityRoleNames.SuperAdministrador);
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdatePerfilAsync(user.Id, [SecurityRoleNames.SuperAdministrador], null));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ResetPasswordAsync(user.Id, "not-allowed", null));
    }

    [Fact]
    public async Task DisabledBootstrapNeverCreatesAnAccount()
    {
        await using var db = Open(); await SeedAsync(db);
        Assert.False(await Service(db, new ConfigurationBuilder().Build()).EnsureAsync());
        Assert.Empty(await db.SecurityUsers.IgnoreQueryFilters().ToArrayAsync());
    }

    [Fact]
    public async Task ExistingEmailIsNeverPromoted()
    {
        await using var db = Open(); var company = await SeedAsync(db);
        db.SecurityUsers.Add(new SecurityUserEntity { Id = Guid.NewGuid(), EmpresaId = company, NormalizedEmail = "ROOT@EXAMPLE.INVALID", UserName = "old", NormalizedUserName = "OLD" });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(db, Config(company)).EnsureAsync());
        Assert.Empty(await db.SecurityUserRoles.IgnoreQueryFilters().ToArrayAsync());
        Assert.Empty(await db.Set<SaasInicializacionEntity>().ToArrayAsync());
    }

    [Theory]
    [InlineData("weak")]
    [InlineData("abcdefghijklmnop")]
    [InlineData("ALL-UPPERCASE-1234")]
    [InlineData("Spaces are not accepted123!")]
    public void WeakOrInvalidPasswordsAreRejected(string password) => Assert.Throws<InvalidOperationException>(() => SaasSuperAdminBootstrapper.ValidatePassword(password));

    [Fact]
    public async Task ExplicitCashierRetirementReleasesEmailWithoutDeletingUserHistory()
    {
        await using var db = Open(); var company = await SeedAsync(db);
        var id = Guid.NewGuid(); var cashierRole = Guid.NewGuid(); var persona = new PersonaEntity { Id = Guid.NewGuid(), EmpresaId = company };
        db.SecurityRoles.Add(new SecurityRoleEntity { Id = cashierRole, Name = SecurityRoleNames.Cajero, NormalizedName = "CAJERO", IsActive = true });
        db.SecurityUsers.Add(new SecurityUserEntity { Id = id, EmpresaId = company, PersonaId = persona.Id, Persona = persona,
            UserName = "OldCashier", NormalizedUserName = "OLDCASHIER", Email = "root@example.invalid", NormalizedEmail = "ROOT@EXAMPLE.INVALID", IsActive = true,
            UserRoles = [new SecurityUserRoleEntity { UserId = id, RoleId = cashierRole }],
            EmpresasAcceso = [new SecurityUserEmpresaEntity { EmpresaId = company, SecurityUserId = id }] });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(db, Config(company)).EnsureAsync(retirarCajeroId: id));
        Assert.True(await Service(db, Config(company)).EnsureAsync(force: true, retirarCajeroId: id));
        var old = await db.SecurityUsers.IgnoreQueryFilters().SingleAsync(x => x.Id == id);
        Assert.False(old.IsActive); Assert.True(old.BloqueadoManualmente); Assert.NotNull(old.TokensInvalidosDesde);
        Assert.Empty(await db.SecurityUserRoles.IgnoreQueryFilters().Where(x => x.UserId == id).ToArrayAsync());
        Assert.Equal(1, await db.SecurityUsers.IgnoreQueryFilters().CountAsync(x => x.NormalizedEmail == "ROOT@EXAMPLE.INVALID" && x.IsActive));
        Assert.Equal(2, await db.SecurityUsers.IgnoreQueryFilters().CountAsync());
    }
}
