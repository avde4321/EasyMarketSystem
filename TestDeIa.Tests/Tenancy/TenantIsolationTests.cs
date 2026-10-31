using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Tenancy;

public sealed class TenantIsolationTests
{
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
}
