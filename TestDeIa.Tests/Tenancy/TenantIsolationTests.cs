using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TestDeIa.Application.Common;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Tenancy;

[Collection("Tenant isolation API integration")]
public sealed class TenantIsolationTests
{
    [Fact]
    public async Task JwtFromTenantACannotReadTenantBPersonaThroughApi()
    {
        using var testEnvironment = new TemporaryEnvironmentVariables(new Dictionary<string, string?>
        {
            ["Security__Jwt__SecretKey"] = TenantIsolationApiFactory.JwtSecret,
            ["Security__Jwt__Issuer"] = TenantIsolationApiFactory.JwtIssuer,
            ["Security__Jwt__Audience"] = TenantIsolationApiFactory.JwtAudience,
            ["Cors__AllowedOrigins__0"] = "https://client.integration.test",
            ["DeploymentSettings__Mode"] = "SaaS",
            ["DeploymentSettings__EnforceSubscription"] = "false",
            ["DeploymentSettings__EnforceQuotas"] = "false"
        });
        await using var factory = new TenantIsolationApiFactory();
        using var client = factory.CreateClient();

        var empresaA = Guid.NewGuid();
        var empresaB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var personaAId = Guid.NewGuid();
        var personaBId = Guid.NewGuid();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestDeIaDbContext>();
            var tenant = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenant.IsSystemContext = true;

            var companyA = CreateCompany(empresaA, userId, "0991000000001");
            var companyB = CreateCompany(empresaB, Guid.NewGuid(), "0992000000001");
            var ownerPersona = CreatePersona(Guid.NewGuid(), empresaA, "0910000001", "Usuario de prueba");
            var user = new SecurityUserEntity
            {
                Id = userId,
                EmpresaId = empresaA,
                PersonaId = ownerPersona.Id,
                Persona = ownerPersona,
                UserName = "tenant-a-user",
                NormalizedUserName = "TENANT-A-USER",
                DisplayName = "Tenant A",
                Email = "tenant-a@example.test",
                NormalizedEmail = "TENANT-A@EXAMPLE.TEST",
                PasswordHash = "unused-in-test",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var assignment = new SecurityUserEmpresaEntity
            {
                SecurityUserId = userId,
                SecurityUser = user,
                EmpresaId = empresaA,
                Empresa = companyA,
                IsDefault = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.EmpresasAcceso.Add(assignment);
            companyA.UserAssignments.Add(assignment);

            dbContext.EmpresasEmisoras.AddRange(companyA, companyB);
            dbContext.Personas.AddRange(
                ownerPersona,
                CreatePersona(personaAId, empresaA, "0911111111", "Cliente Empresa A"),
                CreatePersona(personaBId, empresaB, "0922222222", "Cliente Empresa B"));
            dbContext.SecurityUsers.Add(user);
            dbContext.SecurityUserEmpresas.Add(assignment);
            await dbContext.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateTenantToken(userId));
        client.DefaultRequestHeaders.Add("X-Empresa-Id", empresaA.ToString());

        var ownResource = await client.GetAsync($"/api/personas/{personaAId}");
        var otherTenantResource = await client.GetAsync($"/api/personas/{personaBId}");

        Assert.Equal(HttpStatusCode.OK, ownResource.StatusCode);
        Assert.Contains(otherTenantResource.StatusCode, new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound });
    }

    [Fact]
    public async Task CompanyListIncludesAllAssignedCompaniesButNotOtherUsersCompanies()
    {
        var tenant = new TenantContextAccessor { IsSystemContext = true };
        await using var context = CreateContext(tenant, Guid.NewGuid().ToString("N"));
        var userId = Guid.NewGuid();
        var companyA = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), Ruc = "0991111111001" };
        var companyB = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), Ruc = "0992222222001" };
        var companyC = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), Ruc = "0993333333001" };
        context.EmpresasEmisoras.AddRange(companyA, companyB, companyC);
        var warehouseB = new BodegaEntity
        {
            Id = Guid.NewGuid(), EmpresaId = companyB.Id, Codigo = "001", Nombre = "Bodega B", IsActive = true
        };
        context.Bodegas.Add(warehouseB);
        context.EmpresaPuntosEmision.Add(new EmpresaPuntoEmisionEntity
        {
            Id = Guid.NewGuid(), EmpresaEmisoraId = companyB.Id, Empresa = companyB,
            BodegaId = warehouseB.Id, Bodega = warehouseB, Establecimiento = "001", PuntoEmision = "001", IsDefault = true
        });
        context.SecurityUserEmpresas.AddRange(
            new SecurityUserEmpresaEntity { SecurityUserId = userId, EmpresaId = companyA.Id, Empresa = companyA },
            new SecurityUserEmpresaEntity { SecurityUserId = userId, EmpresaId = companyB.Id, Empresa = companyB },
            new SecurityUserEmpresaEntity { SecurityUserId = Guid.NewGuid(), EmpresaId = companyC.Id, Empresa = companyC });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        tenant.IsSystemContext = false;
        tenant.UserId = userId;
        tenant.EmpresaId = companyA.Id;

        var visibleIds = await context.EmpresasEmisoras.AsNoTracking().Select(c => c.Id).ToListAsync();
        Assert.Contains(companyA.Id, visibleIds);
        Assert.Contains(companyB.Id, visibleIds);
        Assert.DoesNotContain(companyC.Id, visibleIds);
        var repository = new TestDeIa.Infrastructure.Adapters.Out.Empresa.EfEmpresaRepository(
            context, tenant, new TestDeIa.Infrastructure.Adapters.Out.Contabilidad.CatalogoNiifSeed());
        var company = await repository.GetByIdAsync(companyB.Id);
        Assert.NotNull(company);
        Assert.Equal("Bodega B", Assert.Single(company.PuntosEmision).BodegaNombre);
        Assert.Null(await repository.GetByIdAsync(companyC.Id));
        Assert.Empty(await context.Bodegas.AsNoTracking().ToListAsync());
    }

    [Fact]
    public void EntitiesWithGuidEmpresaIdImplementTenantEntity()
    {
        var entityTypes = typeof(PersonaEntity).Assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.Namespace == typeof(PersonaEntity).Namespace &&
                type.GetProperty(nameof(ITenantEntity.EmpresaId))?.PropertyType == typeof(Guid))
            .OrderBy(type => type.Name)
            .ToArray();

        var offenders = entityTypes
            .Where(type => !typeof(ITenantEntity).IsAssignableFrom(type))
            .Select(type => type.Name)
            .ToArray();

        Assert.True(offenders.Length == 0, $"Entidades con EmpresaId sin ITenantEntity: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TenantEntitiesHaveGlobalQueryFilter()
    {
        var tenantContext = new TenantContextAccessor
        {
            EmpresaId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsSystemContext = false
        };

        using var dbContext = CreateContext(tenantContext, Guid.NewGuid().ToString("N"));

        var offenders = dbContext.Model
            .GetEntityTypes()
            .Where(entityType => typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            .Where(entityType => entityType.GetQueryFilter() is null)
            .Select(entityType => entityType.ClrType.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.True(offenders.Length == 0, $"Entidades tenant sin filtro global: {string.Join(", ", offenders)}");
    }

    [Fact]
    public async Task TenantFilterPreventsCrossCompanyPersonaReads()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var empresaA = Guid.NewGuid();
        var empresaB = Guid.NewGuid();

        await using (var seedContext = CreateContext(new TenantContextAccessor { IsSystemContext = true }, databaseName))
        {
            seedContext.Personas.AddRange(
                CreatePersona(Guid.NewGuid(), empresaA, "0911111111", "Persona Empresa A"),
                CreatePersona(Guid.NewGuid(), empresaB, "0922222222", "Persona Empresa B"));

            await seedContext.SaveChangesAsync();
        }

        await using var empresaAContext = CreateContext(new TenantContextAccessor
        {
            EmpresaId = empresaA,
            UserId = Guid.NewGuid(),
            IsSystemContext = false
        }, databaseName);

        var personasVisibles = await empresaAContext.Personas
            .AsNoTracking()
            .Select(current => new { current.EmpresaId, current.Identificacion })
            .ToArrayAsync();

        Assert.Single(personasVisibles);
        Assert.Equal(empresaA, personasVisibles[0].EmpresaId);
        Assert.Equal("0911111111", personasVisibles[0].Identificacion);

        var crossTenantPersona = await empresaAContext.Personas
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaB);

        Assert.Null(crossTenantPersona);
    }

    [Fact]
    public async Task SystemContextCanReadAllTenantsForControlledWorkers()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var empresaA = Guid.NewGuid();
        var empresaB = Guid.NewGuid();

        await using var dbContext = CreateContext(new TenantContextAccessor { IsSystemContext = true }, databaseName);

        dbContext.Personas.AddRange(
            CreatePersona(Guid.NewGuid(), empresaA, "0933333333", "Persona Sistema A"),
            CreatePersona(Guid.NewGuid(), empresaB, "0944444444", "Persona Sistema B"));

        await dbContext.SaveChangesAsync();

        var total = await dbContext.Personas.CountAsync();

        Assert.Equal(2, total);
    }

    private static TestDeIaDbContext CreateContext(TenantContextAccessor tenantContextAccessor, string databaseName)
    {
        var options = new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestDeIaDbContext(options, tenantContextAccessor);
    }

    private static PersonaEntity CreatePersona(Guid id, Guid empresaId, string identificacion, string nombres)
    {
        return new PersonaEntity
        {
            Id = id,
            EmpresaId = empresaId,
            TipoIdentificacion = "05",
            Identificacion = identificacion,
            RazonSocialONombresCompletos = nombres,
            DireccionPrincipal = "Direccion de prueba",
            CorreoElectronicoPrincipal = "qa@easymarket.test",
            TelefonoCelular = "0999999999",
            EsPersonaJuridica = false,
            EsEmpresa = false,
            IsActive = true,
            IsSystemRecord = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static EmpresaEmisoraEntity CreateCompany(Guid id, Guid ownerUserId, string ruc) => new()
    {
        Id = id,
        OwnerUserId = ownerUserId,
        RazonSocial = $"Empresa {ruc}",
        Ruc = ruc,
        DireccionMatriz = "Dirección de prueba",
        Establecimiento = "001",
        PuntoEmision = "001",
        AmbienteSri = "1",
        TipoEmision = "1",
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static string CreateTenantToken(Guid userId)
    {
        var now = DateTime.UtcNow;
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TenantIsolationApiFactory.JwtSecret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: TenantIsolationApiFactory.JwtIssuer,
            audience: TenantIsolationApiFactory.JwtAudience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            ],
            notBefore: now.AddSeconds(-5),
            expires: now.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class TenantIsolationApiFactory : WebApplicationFactory<Program>
    {
        internal const string JwtSecret = "tenant-isolation-test-secret-key-minimum-32-bytes";
        internal const string JwtIssuer = "TestDeIa.IntegrationTests";
        internal const string JwtAudience = "TestDeIa.IntegrationTests.Client";

        private readonly string databaseName = $"tenant-isolation-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Security:Jwt:SecretKey"] = JwtSecret,
                    ["Security:Jwt:Issuer"] = JwtIssuer,
                    ["Security:Jwt:Audience"] = JwtAudience,
                    ["Cors:AllowedOrigins:0"] = "https://client.integration.test",
                    ["DeploymentSettings:Mode"] = "SaaS",
                    ["DeploymentSettings:EnforceSubscription"] = "false",
                    ["DeploymentSettings:EnforceQuotas"] = "false"
                }));

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IHostedService>();
                services.RemoveAll<IDbContextOptionsConfiguration<TestDeIaDbContext>>();
                services.RemoveAll<DbContextOptions<TestDeIaDbContext>>();
                services.RemoveAll<TestDeIaDbContext>();
                services.AddDbContext<TestDeIaDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
        }
    }

    private sealed class TemporaryEnvironmentVariables : IDisposable
    {
        private readonly Dictionary<string, string?> previousValues;

        public TemporaryEnvironmentVariables(IReadOnlyDictionary<string, string?> values)
        {
            previousValues = values.Keys.ToDictionary(key => key, Environment.GetEnvironmentVariable);
            foreach (var (key, value) in values)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public void Dispose()
        {
            foreach (var (key, value) in previousValues)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}

[CollectionDefinition("Tenant isolation API integration", DisableParallelization = true)]
public sealed class TenantIsolationApiCollection { }
