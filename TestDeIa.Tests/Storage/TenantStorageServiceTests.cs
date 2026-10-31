using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Infrastructure.Adapters.Out.Sri;
using TestDeIa.Infrastructure.Options;

namespace TestDeIa.Tests.Storage;

public sealed class TenantStorageServiceTests : IDisposable
{
    private readonly string rootPath;

    public TenantStorageServiceTests()
    {
        rootPath = Path.Combine(Path.GetTempPath(), "easy-market-storage-tests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task SaveAsyncStoresFileInTenantIsolatedHierarchy()
    {
        var empresaId = Guid.NewGuid();
        var entidadId = Guid.NewGuid();
        var service = CreateService();
        var content = "<factura id=\"1\" />"u8.ToArray();

        var result = await service.SaveAsync(new DocumentoStorageRequest
        {
            EmpresaId = empresaId,
            Modulo = "SRI",
            EntidadTipo = "Factura",
            EntidadId = entidadId,
            TipoAdjunto = "XML_GENERADO",
            NombreArchivo = "factura.xml",
            ContentType = "application/xml",
            Content = content,
            Origen = "SRI"
        });

        Assert.False(result.EsCifrado);
        Assert.True(File.Exists(result.RutaStorage));
        Assert.Contains(Path.Combine(empresaId.ToString("D"), "SRI", "FACTURA"), result.RutaStorage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Path.Combine(DateTimeOffset.UtcNow.ToString("yyyy"), DateTimeOffset.UtcNow.ToString("MM"), entidadId.ToString("D")), result.RutaStorage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(content)), result.HashSHA256);
    }

    [Fact]
    public async Task SaveAsyncEncryptsAndDecryptsSensitiveCertificateWithHashValidation()
    {
        var empresaId = Guid.NewGuid();
        var entidadId = Guid.NewGuid();
        var key = RandomNumberGenerator.GetBytes(32);
        var service = CreateService(Convert.ToBase64String(key));
        var content = RandomNumberGenerator.GetBytes(512);

        var result = await service.SaveAsync(new DocumentoStorageRequest
        {
            EmpresaId = empresaId,
            Modulo = "SRI",
            EntidadTipo = "CertificadoDigital",
            EntidadId = entidadId,
            TipoAdjunto = "CERTIFICADO_P12",
            NombreArchivo = "firma.p12",
            ContentType = "application/x-pkcs12",
            Content = content,
            Origen = "Manual",
            RequiereCifrado = true
        });

        var persisted = await File.ReadAllBytesAsync(result.RutaStorage);
        Assert.True(result.EsCifrado);
        Assert.Equal("AES-256-GCM", result.AlgoritmoCifrado);
        Assert.NotEqual(content, persisted);

        var read = await service.ReadAsync(result.RutaStorage, result.EsCifrado, result.HashSHA256);
        Assert.Equal(content, read.Content);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(content)), read.HashSHA256);
    }

    [Fact]
    public async Task SaveAsyncRejectsFilesOutsideWhitelist()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new DocumentoStorageRequest
            {
                EmpresaId = Guid.NewGuid(),
                Modulo = "SRI",
                EntidadTipo = "Factura",
                EntidadId = Guid.NewGuid(),
                TipoAdjunto = "BIN",
                NombreArchivo = "payload.exe",
                ContentType = "application/octet-stream",
                Content = [1, 2, 3]
            }));

        Assert.Contains("no esta permitido", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (Directory.Exists(rootPath))
        {
            Directory.Delete(rootPath, recursive: true);
        }
    }

    private TenantStorageService CreateService(string? encryptionKeyBase64 = null)
    {
        var options = Options.Create(new DocumentoStorageOptions
        {
            RootPath = rootPath,
            EncryptionKeyBase64 = encryptionKeyBase64,
            DefaultMaxBytes = 1024 * 1024,
            CertificateMaxBytes = 1024 * 1024
        });

        var configuration = new ConfigurationBuilder().Build();
        return new TenantStorageService(options, configuration);
    }
}
