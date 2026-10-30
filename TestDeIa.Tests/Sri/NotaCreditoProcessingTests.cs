using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Application.Modules.Sri.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Adapters.Out.Sri;
using TestDeIa.Infrastructure.Options;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Sri;

public sealed class NotaCreditoProcessingTests
{
    [Theory]
    [InlineData("AUTORIZADO", FacturaEstado.AUTORIZADO)]
    [InlineData("", FacturaEstado.PENDIENTE)]
    [InlineData("NO AUTORIZADO", FacturaEstado.RECHAZADO)]
    public async Task RealModeUsesSignedCreditNoteAndMapsActualSriResponse(string response, FacturaEstado expected)
    {
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new TenantContextAccessor { IsSystemContext = true });
        await db.Database.EnsureCreatedAsync();
        var company = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), Ruc = "0991111111001", IsActive = true, ModoDesarrollo = false };
        using var rsa = RSA.Create(2048);
        using var cert = new CertificateRequest("CN=Prueba local", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1).CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        company.CertificadosDigitales.Add(new CertificadoDigitalEmpresaEntity { Id = Guid.NewGuid(), EmpresaId = company.Id, Empresa = company, IsActive = true, FechaInicioVigencia = DateTimeOffset.UtcNow.AddDays(-1), FechaFinVigencia = DateTimeOffset.UtcNow.AddDays(1), Contenido = cert.Export(X509ContentType.Pkcs12, "test"), Clave = "test" });
        var note = Note(company.Id);
        var originalXml = note.XmlGenerado;
        db.EmpresasEmisoras.Add(company); db.ComprobanteCabecera.Add(note);
        var item = new ColaProcesamientoSriEntity { Id = Guid.NewGuid(), EmpresaId = company.Id, ComprobanteId = note.Id, TipoDocumentoId = "04", Estado = SriOutboxEstados.EnProceso, ProcessingNode = "test", RowVersion = [1] };
        db.ColaProcesamientoSRI.Add(item); await db.SaveChangesAsync();
        var handler = new OtherDocumentTests.FakeSri(note.ClaveAcceso) { AuthorizationState = response };
        var processor = new SriComprobanteProcessor(db, new MemoryStorage(), new SriStateEngineService(db, NullLogger<SriStateEngineService>.Instance), new(), OtherDocumentTests.Submission(handler), new(), NullLogger<SriComprobanteProcessor>.Instance);
        await processor.ProcessAsync(item.Id, "test"); await db.SaveChangesAsync();
        Assert.Equal(expected, note.Estado);
        Assert.True(SriDocumentSubmissionService.HasSignature(note.XmlFirmado));
        Assert.Equal(originalXml, note.XmlGenerado);
        Assert.Equal(1, handler.Receptions);
        Assert.Equal(expected == FacturaEstado.AUTORIZADO, note.NumeroAutorizacion is not null);
        Assert.Equal(expected == FacturaEstado.AUTORIZADO, await db.DocumentosAdjuntos.AnyAsync(x => x.TipoAdjunto == "XML_AUTORIZADO"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OnlyExplicitDevelopmentModeCanSimulateAuthorization(bool development)
    {
        await using var db = new TestDeIaDbContext(new DbContextOptionsBuilder<TestDeIaDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new TenantContextAccessor { IsSystemContext = true });
        await db.Database.EnsureCreatedAsync();
        var company = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), Ruc = "0991111111001", IsActive = true, ModoDesarrollo = development };
        var note = Note(company.Id);
        db.EmpresasEmisoras.Add(company); db.ComprobanteCabecera.Add(note);
        var item = new ColaProcesamientoSriEntity { Id = Guid.NewGuid(), EmpresaId = company.Id, ComprobanteId = note.Id, TipoDocumentoId = "04", Estado = SriOutboxEstados.EnProceso, ProcessingNode = "test", RowVersion = [1] };
        db.ColaProcesamientoSRI.Add(item);
        await db.SaveChangesAsync();
        var options = Options.Create(new SriSoapOptions());
        var soap = new SriSoapClient(new HttpClient(new NoNetwork()), new(options), new(), NullLogger<SriSoapClient>.Instance);
        var processor = new SriComprobanteProcessor(db, new MemoryStorage(), new SriStateEngineService(db, NullLogger<SriStateEngineService>.Instance), new(), new(soap), new(), NullLogger<SriComprobanteProcessor>.Instance);
        if (development)
        {
            var result = await processor.ProcessAsync(item.Id, "test");
            Assert.True(result.Succeeded); Assert.Equal(FacturaEstado.AUTORIZADO, note.Estado);
            Assert.Contains("simulación", item.Mensaje); Assert.Null(note.XmlFirmado);
        }
        else
        {
            // XML válido pero sin certificado: no puede ser autorizado ni enviado.
            await Assert.ThrowsAsync<InvalidOperationException>(() => processor.ProcessAsync(item.Id, "test"));
            Assert.NotEqual(FacturaEstado.AUTORIZADO, note.Estado); Assert.Null(note.NumeroAutorizacion);
        }
    }

    [Fact]
    public void CreditNoteKeepsItsOwnSchemaAndModifiedDocumentFields()
    {
        var note = Note(Guid.NewGuid());
        new SriNotaCreditoXmlSchemaValidator().Validate(note.XmlGenerado!);
        Assert.Contains("<codDocModificado>01</codDocModificado>", note.XmlGenerado);
        Assert.Contains("<numDocModificado>001-001-000000001</numDocModificado>", note.XmlGenerado);
        Assert.DoesNotContain("<infoFactura>", note.XmlGenerado);
    }

    private static ComprobanteCabeceraEntity Note(Guid empresa)
    {
        var date = new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
        var note = new ComprobanteCabeceraEntity { Id = Guid.NewGuid(), EmpresaId = empresa, TipoDocumentoId = "04", RucEmisor = "0991111111001", RazonSocialEmisor = "Empresa de pruebas", DireccionMatrizEmisor = "Guayaquil", AmbienteSri = "1", TipoEmision = "1", Establecimiento = "001", PuntoEmision = "001", Secuencial = 1, FechaEmision = date, ClienteTipoIdentificacion = "04", ClienteIdentificacion = "0992222222001", ClienteNombre = "Cliente de prueba", CodDocModificado = "01", NumDocModificado = "001-001-000000001", FechaEmisionDocSustento = date, MotivoModificacion = "Devolucion de mercaderia", Subtotal = 100, IvaTotal = 15, Total = 115 };
        note.Detalles.Add(new ComprobanteDetalleEntity { CodigoProducto = "P001", NombreProducto = "Producto", Cantidad = 1, PrecioUnitario = 100, Subtotal = 100, CodigoIva = "4", PorcentajeIva = 15, IvaValor = 15, Total = 115 });
        var generator = new NotaCreditoXmlGenerator(new(), new());
        note.ClaveAcceso = generator.GenerarClaveAcceso(note);
        note.XmlGenerado = generator.BuildUnsignedXml(note);
        return note;
    }
    private sealed class NoNetwork : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => throw new InvalidOperationException("La prueba no permite llamadas de red.");
    }
    private sealed class MemoryStorage : IDocumentoStorageService
    {
        public Task<DocumentoStorageResult> SaveAsync(DocumentoStorageRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new DocumentoStorageResult { RutaStorage = request.NombreArchivo, HashSHA256 = Convert.ToHexString(SHA256.HashData(request.Content)), TamanoBytes = request.Content.Length });
        public Task<DocumentoStorageReadResult> ReadAsync(string rutaStorage, bool esCifrado, string expectedHashSha256, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
