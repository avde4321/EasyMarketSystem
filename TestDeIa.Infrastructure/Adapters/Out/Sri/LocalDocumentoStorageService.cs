using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Application.Modules.Sri.Ports.Out;
using TestDeIa.Infrastructure.Options;

namespace TestDeIa.Infrastructure.Adapters.Out.Sri;

public sealed class LocalDocumentoStorageService(IOptions<DocumentoStorageOptions> options) : IDocumentoStorageService
{
    private readonly DocumentoStorageOptions options = options.Value;

    public async Task<DocumentoStorageResult> SaveAsync(
        DocumentoStorageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Content.Length == 0)
        {
            throw new InvalidOperationException("No existe contenido para almacenar como documento adjunto.");
        }

        var safeFileName = SanitizeFileName(request.NombreArchivo);
        var relativeDirectory = Path.Combine(
            request.EmpresaId.ToString("N"),
            SanitizePathSegment(request.Modulo),
            SanitizePathSegment(request.EntidadTipo),
            DateTimeOffset.UtcNow.ToString("yyyy"),
            DateTimeOffset.UtcNow.ToString("MM"),
            request.EntidadId.ToString("N"));

        var rootPath = Path.GetFullPath(options.RootPath);
        var directory = Path.GetFullPath(Path.Combine(rootPath, relativeDirectory));

        if (!directory.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La ruta de almacenamiento calculada no es segura.");
        }

        Directory.CreateDirectory(directory);
        var fullPath = Path.Combine(directory, safeFileName);
        await File.WriteAllBytesAsync(fullPath, request.Content, cancellationToken);

        return new DocumentoStorageResult
        {
            RutaStorage = fullPath,
            HashSHA256 = Convert.ToHexString(SHA256.HashData(request.Content)),
            TamanoBytes = request.Content.LongLength
        };
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
