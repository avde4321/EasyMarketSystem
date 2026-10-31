using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Application.Modules.Sri.Ports.Out;
using TestDeIa.Infrastructure.Options;

namespace TestDeIa.Infrastructure.Adapters.Out.Sri;

public class TenantStorageService(
    IOptions<DocumentoStorageOptions> options,
    IConfiguration configuration) : ITenantStorageService
{
    private const string EncryptionAlgorithm = "AES-256-GCM";
    private static readonly byte[] Magic = "EMSGCM1"u8.ToArray();

    private static readonly IReadOnlyDictionary<string, string[]> AllowedContentTypesByExtension =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".xml"] = ["application/xml", "text/xml", "application/octet-stream"],
            [".pdf"] = ["application/pdf"],
            [".p12"] = ["application/x-pkcs12", "application/octet-stream"],
            [".pfx"] = ["application/x-pkcs12", "application/octet-stream"],
            [".txt"] = ["text/plain", "application/octet-stream"],
            [".csv"] = ["text/csv", "application/vnd.ms-excel", "text/plain", "application/octet-stream"],
            [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/octet-stream"]
        };

    private readonly DocumentoStorageOptions options = options.Value;

    public async Task<DocumentoStorageResult> SaveAsync(
        DocumentoStorageRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var safeFileName = SanitizeFileName(request.NombreArchivo);
        var now = DateTimeOffset.UtcNow;
        var relativeDirectory = Path.Combine(
            request.EmpresaId.ToString("D"),
            SanitizePathSegment(request.Modulo),
            SanitizePathSegment(request.EntidadTipo),
            now.ToString("yyyy"),
            now.ToString("MM"),
            request.EntidadId.ToString("D"));

        var rootPath = Path.GetFullPath(this.options.RootPath);
        var directory = Path.GetFullPath(Path.Combine(rootPath, relativeDirectory));

        if (!directory.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La ruta de almacenamiento calculada no es segura.");
        }

        Directory.CreateDirectory(directory);
        var fullPath = Path.GetFullPath(Path.Combine(directory, safeFileName));
        if (!fullPath.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El nombre del archivo no es seguro.");
        }

        var originalHash = Convert.ToHexString(SHA256.HashData(request.Content));
        var bytesToPersist = request.RequiereCifrado
            ? Encrypt(request.Content)
            : request.Content;

        await File.WriteAllBytesAsync(fullPath, bytesToPersist, cancellationToken);

        return new DocumentoStorageResult
        {
            RutaStorage = fullPath,
            HashSHA256 = originalHash,
            TamanoBytes = request.Content.LongLength,
            EsCifrado = request.RequiereCifrado,
            AlgoritmoCifrado = request.RequiereCifrado ? EncryptionAlgorithm : null
        };
    }

    public async Task<DocumentoStorageReadResult> ReadAsync(
        string rutaStorage,
        bool esCifrado,
        string expectedHashSha256,
        CancellationToken cancellationToken = default)
    {
        var rootPath = Path.GetFullPath(this.options.RootPath);
        var fullPath = Path.GetFullPath(rutaStorage);
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La ruta solicitada no pertenece al storage configurado.");
        }

        var persisted = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        var content = esCifrado ? Decrypt(persisted) : persisted;
        var hash = Convert.ToHexString(SHA256.HashData(content));

        if (!string.IsNullOrWhiteSpace(expectedHashSha256) &&
            !string.Equals(hash, expectedHashSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La verificacion de integridad SHA256 del documento fallo.");
        }

        return new DocumentoStorageReadResult
        {
            Content = content,
            HashSHA256 = hash,
            TamanoBytes = content.LongLength
        };
    }

    private void ValidateRequest(DocumentoStorageRequest request)
    {
        if (request.EmpresaId == Guid.Empty)
        {
            throw new InvalidOperationException("El documento debe pertenecer a una empresa valida.");
        }

        if (request.EntidadId == Guid.Empty)
        {
            throw new InvalidOperationException("El documento debe pertenecer a una entidad valida.");
        }

        if (request.Content.Length == 0)
        {
            throw new InvalidOperationException("No existe contenido para almacenar como documento adjunto.");
        }

        var extension = Path.GetExtension(request.NombreArchivo);
        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedContentTypesByExtension.TryGetValue(extension, out var allowedContentTypes))
        {
            throw new InvalidOperationException("El tipo de archivo no esta permitido para gestion documental.");
        }

        var contentType = request.ContentType.Trim();
        if (!allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El MIME Content-Type del archivo no coincide con la lista blanca permitida.");
        }

        var maxBytes = extension.Equals(".p12", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".pfx", StringComparison.OrdinalIgnoreCase)
                ? this.options.CertificateMaxBytes
                : this.options.DefaultMaxBytes;

        if (request.Content.LongLength > maxBytes)
        {
            throw new InvalidOperationException("El archivo supera el tamano maximo permitido para su tipo documental.");
        }
    }

    private byte[] Encrypt(byte[] plainContent)
    {
        var key = ResolveEncryptionKey();
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        var cipher = new byte[plainContent.Length];

        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plainContent, cipher, tag);

        using var output = new MemoryStream();
        output.Write(Magic);
        output.WriteByte((byte)nonce.Length);
        output.WriteByte((byte)tag.Length);
        output.Write(nonce);
        output.Write(tag);
        output.Write(cipher);
        return output.ToArray();
    }

    private byte[] Decrypt(byte[] encryptedContent)
    {
        if (encryptedContent.Length < Magic.Length + 2 ||
            !encryptedContent.AsSpan(0, Magic.Length).SequenceEqual(Magic))
        {
            throw new InvalidOperationException("El archivo cifrado no tiene el formato esperado.");
        }

        var nonceLength = encryptedContent[Magic.Length];
        var tagLength = encryptedContent[Magic.Length + 1];
        var offset = Magic.Length + 2;

        var nonce = encryptedContent.AsSpan(offset, nonceLength).ToArray();
        offset += nonceLength;
        var tag = encryptedContent.AsSpan(offset, tagLength).ToArray();
        offset += tagLength;
        var cipher = encryptedContent.AsSpan(offset).ToArray();
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(ResolveEncryptionKey(), tagLength);
        aes.Decrypt(nonce, cipher, tag, plain);
        return plain;
    }

    private byte[] ResolveEncryptionKey()
    {
        var configured = this.options.EncryptionKeyBase64
            ?? configuration["DocumentoStorage:EncryptionKeyBase64"];

        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException("DocumentoStorage:EncryptionKeyBase64 debe configurarse fuera del codigo para cifrar documentos sensibles.");
        }

        var key = Convert.FromBase64String(configured);
        if (key.Length != 32)
        {
            throw new InvalidOperationException("DocumentoStorage:EncryptionKeyBase64 debe contener una clave AES-256 de 32 bytes en Base64.");
        }

        return key;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(fileName.Select(character => invalidChars.Contains(character) ? '_' : character).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? $"{Guid.NewGuid():N}.bin" : cleaned;
    }

    private static string SanitizePathSegment(string value)
    {
        var cleaned = new string(value.Where(character => char.IsLetterOrDigit(character) || character is '-' or '_').ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "GENERAL" : cleaned.ToUpperInvariant();
    }
}
