using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TestDeIa.Infrastructure.Adapters.Out.Empresa;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Shared.Requests.Empresa;

namespace TestDeIa.Tests.Sri;

public sealed class NumeracionTests
{
    [Fact]
    public async Task ConfigurationSeparatesEnvironmentsAndAuditsChanges()
    {
        var (db, tenant, point) = await CreateAsync();
        await using var context = db;
        var service = new NumeracionService(db, tenant);
        await service.ConfigurarAsync(Request(point, "2", 1250), default);
        Assert.Equal(1251, (await service.ConsultarAsync("2", default)).Single(x => x.CodigoDocumento == "01").ProximoSecuencial);
        Assert.Equal(1, (await service.ConsultarAsync("1", default)).Single(x => x.CodigoDocumento == "01").ProximoSecuencial);
        var audit = Assert.Single(await service.HistorialAsync(default));
        Assert.Equal(tenant.UserId, audit.UsuarioId);
        Assert.Equal(1250, audit.Nuevo);
    }

    [Theory]
    [InlineData(50, 100)] // Retroceso.
    [InlineData(120, 0)] // Formulario desactualizado.
    public async Task RejectsBackwardsAndStaleConfiguration(long nuevo, long expected)
    {
        var (db, tenant, point) = await CreateAsync();
        await using var context = db;
        var service = new NumeracionService(db, tenant);
        await service.ConfigurarAsync(Request(point, "1", 100), default);
        var request = Request(point, "1", nuevo); request.ValorEsperado = expected;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConfigurarAsync(request, default));
        Assert.Equal(100, (await db.FacturaSecuenciales.AsNoTracking().SingleAsync()).UltimoSecuencial);
    }

    [Fact]
    public async Task CannotConfigureAnotherCompanyPointOrReadItsHistory()
    {
        var (db, tenant, point) = await CreateAsync();
        await using var context = db;
        var service = new NumeracionService(db, tenant);
        await service.ConfigurarAsync(Request(point, "1", 100), default);
        tenant.EmpresaId = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConfigurarAsync(Request(point, "1", 200), default));
        Assert.Empty(await service.HistorialAsync(default));
        Assert.Empty(await service.ConsultarAsync("1", default));
    }

    [Fact]
    public async Task HistoricalInvoicesPreventReusingNumbersEvenWithoutCounter()
    {
        var (db, tenant, point) = await CreateAsync();
        await using var context = db;
        db.Facturas.Add(new FacturaEntity { Id = Guid.NewGuid(), EmpresaId = tenant.EmpresaId!.Value, AmbienteSri = "1", Establecimiento = "001", PuntoEmision = "001", Secuencial = 500 });
        await db.SaveChangesAsync();
        var service = new NumeracionService(db, tenant);
        Assert.Equal(501, (await service.ConsultarAsync("1", default)).Single(x => x.CodigoDocumento == "01").ProximoSecuencial);
        var request = Request(point, "1", 100); request.ValorEsperado = 500;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConfigurarAsync(request, default));
    }

    private static NumeracionRequest Request(Guid point, string ambiente, long last) => new() { PuntoEmisionId = point, AmbienteSri = ambiente, CodigoDocumento = "01", UltimoSecuencial = last, Motivo = "Migración desde proveedor anterior" };

    private static async Task<(TestDeIaDbContext, TenantContextAccessor, Guid)> CreateAsync()
    {
        var tenant = new TenantContextAccessor { EmpresaId = Guid.NewGuid(), UserId = Guid.NewGuid(), IsSystemContext = true };
        var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options, tenant);
        var company = new EmpresaEmisoraEntity { Id = tenant.EmpresaId.Value, Ruc = "0991111111001" };
        var point = new EmpresaPuntoEmisionEntity { Id = Guid.NewGuid(), EmpresaEmisoraId = company.Id, Empresa = company, Establecimiento = "001", PuntoEmision = "001" };
        db.EmpresasEmisoras.Add(company);
        db.EmpresaPuntosEmision.Add(point);
        db.SecurityUserEmpresas.Add(new SecurityUserEmpresaEntity { SecurityUserId = tenant.UserId.Value, EmpresaId = company.Id, Empresa = company });
        await db.SaveChangesAsync();
        tenant.IsSystemContext = false;
        return (db, tenant, point.Id);
    }
}
