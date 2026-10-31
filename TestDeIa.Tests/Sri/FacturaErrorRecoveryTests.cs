using Microsoft.EntityFrameworkCore;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Adapters.Out.Saas;

namespace TestDeIa.Tests.Sri;

public sealed class FacturaErrorRecoveryTests
{
    [Theory]
    [InlineData(null, FacturaEstado.NO_FIRMADO)]
    [InlineData("<firmado/>", FacturaEstado.PENDIENTE)]
    public async Task ErrorRecoveryInsertsEventAndPreservesRetryStage(string? signedXml, FacturaEstado expected)
    {
        var tenant = new TenantContextAccessor { IsSystemContext = true };
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, tenant);
        var factura = new FacturaEntity { Id = Guid.NewGuid(), EmpresaId = Guid.NewGuid(), XmlFirmado = signedXml, Estado = FacturaEstado.NO_FIRMADO };
        db.Facturas.Add(factura);
        await db.SaveChangesAsync();
        var repo = new EfFacturacionRepository(db, tenant, null!, new SaaSQuotaValidationService(db));
        var diagnostic = new string('X', 1500);
        await repo.MarkFacturaAsErrorAsync(factura.Id, diagnostic, null);
        Assert.Equal(expected, (await db.Facturas.SingleAsync()).Estado);
        Assert.Single(await db.FacturaSriEventos.ToListAsync());
        Assert.Equal(diagnostic, (await db.FacturaSriEventos.SingleAsync()).Mensaje);
        Assert.Null(db.Model.FindEntityType(typeof(FacturaSriEventoEntity))!.FindProperty("Mensaje")!.GetMaxLength());
    }
}
