using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Adapters.Out.Sri;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Sri;

public sealed class SriStateEngineServiceTests
{
    [Fact]
    public async Task ApplyTransitionRejectsInvalidTransitionFromAuthorizedToGenerated()
    {
        var empresaId = Guid.NewGuid();
        var comprobanteId = Guid.NewGuid();
        await using var dbContext = CreateContext();

        dbContext.ColaProcesamientoSRI.Add(new ColaProcesamientoSriEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ComprobanteId = comprobanteId,
            TipoDocumentoId = "01",
            Estado = SriEstadosComprobante.Autorizado,
            CreatedAt = DateTimeOffset.UtcNow,
            RowVersion = [1]
        });
        await dbContext.SaveChangesAsync();

        var service = new SriStateEngineService(dbContext, NullLogger<SriStateEngineService>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApplyTransitionAsync(new SriTransitionRequest
            {
                EmpresaId = empresaId,
                ComprobanteId = comprobanteId,
                TipoDocumentoId = "01",
                EstadoNuevoCodigo = SriEstadosComprobante.Generado,
                MensajeRespuesta = "Intento no permitido"
            }));

        Assert.Contains("Transición SRI no permitida", exception.Message);
        Assert.Empty(await dbContext.SriHistorialEstadosComprobante.ToArrayAsync());
    }

    [Fact]
    public async Task ApplyTransitionFromSignedToInProcessAuditsHistory()
    {
        var empresaId = Guid.NewGuid();
        var comprobanteId = Guid.NewGuid();
        await using var dbContext = CreateContext();

        dbContext.ColaProcesamientoSRI.Add(new ColaProcesamientoSriEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ComprobanteId = comprobanteId,
            TipoDocumentoId = "01",
            Estado = SriEstadosComprobante.Firmado,
            CreatedAt = DateTimeOffset.UtcNow,
            RowVersion = [1]
        });
        await dbContext.SaveChangesAsync();

        var service = new SriStateEngineService(dbContext, NullLogger<SriStateEngineService>.Instance);

        var result = await service.ApplyTransitionAsync(new SriTransitionRequest
        {
            EmpresaId = empresaId,
            ComprobanteId = comprobanteId,
            TipoDocumentoId = "01",
            EstadoNuevoCodigo = SriEstadosComprobante.EnProceso,
            MensajeRespuesta = "RECIBIDA",
            WorkerNode = "test-worker"
        });
        await dbContext.SaveChangesAsync();

        var outbox = await dbContext.ColaProcesamientoSRI.SingleAsync();
        var historial = await dbContext.SriHistorialEstadosComprobante.SingleAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(SriEstadosComprobante.EnProceso, outbox.Estado);
        Assert.Equal(SriEstadosComprobante.Firmado, result.EstadoAnterior);
        Assert.Equal(SriEstadosComprobante.EnProceso, result.EstadoNuevo);
        Assert.Equal(comprobanteId, historial.ComprobanteId);
        Assert.Equal("RECIBIDA", historial.MensajeRespuesta);
        Assert.Equal("test-worker", historial.WorkerNode);
    }

    [Fact]
    public async Task GetErrorByCodigoReturnsCatalogMapping()
    {
        await using var dbContext = CreateContext();
        var service = new SriStateEngineService(dbContext, NullLogger<SriStateEngineService>.Instance);

        var error = await service.GetErrorByCodigoAsync("70");

        Assert.NotNull(error);
        Assert.Equal("70", error.CodigoSri);
        Assert.Equal(SriTipoError.ErrorFirma.ToString(), error.TipoError);
        Assert.Contains("certificado", error.SolucionSugerida, StringComparison.OrdinalIgnoreCase);
    }

    private static TestDeIaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDeIaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var dbContext = new TestDeIaDbContext(options, new TenantContextAccessor { IsSystemContext = true });
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
