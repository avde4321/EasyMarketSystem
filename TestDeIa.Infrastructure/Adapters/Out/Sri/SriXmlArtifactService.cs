using System.Text;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Application.Modules.Sri.Ports.Out;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
namespace TestDeIa.Infrastructure.Adapters.Out.Sri;
public sealed class SriXmlArtifactService(TestDeIaDbContext dbContext, IDocumentoStorageService documentoStorageService)
{
    /// <summary>Lee metadata y contenido de un adjunto solo si coincide con la empresa, tipo e ID de entidad solicitados.</summary>
    public async Task<string?> ReadAsync(Guid empresaId, string entidadTipo, Guid entidadId, string tipoAdjunto, CancellationToken ct)
    {
        var metadata = await dbContext.DocumentosAdjuntos.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.EntidadTipo == entidadTipo && x.EntidadId == entidadId && x.TipoAdjunto == tipoAdjunto && x.EsActivo)
            .OrderByDescending(x => x.FechaCreacion).FirstOrDefaultAsync(ct);
        if (metadata is null) return null;
        var file = await documentoStorageService.ReadAsync(metadata.RutaStorage, metadata.EsCifrado, metadata.HashSHA256, ct);
        return Encoding.UTF8.GetString(file.Content);
    }
    /// <summary>Persiste un XML de proceso SRI bajo el tenant explícito y evita duplicar el mismo tipo de adjunto activo.</summary>
    public async Task SaveAsync(
        Guid empresaId,
        string modulo,
        string entidadTipo,
        Guid entidadId,
        string tipoAdjunto,
        string nombreArchivo,
        string? content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var exists = await dbContext.DocumentosAdjuntos
            .IgnoreQueryFilters()
            .AnyAsync(current =>
                current.EmpresaId == empresaId &&
                current.EntidadTipo == entidadTipo &&
                current.EntidadId == entidadId &&
                current.TipoAdjunto == tipoAdjunto &&
                current.EsActivo,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(content);
        var storage = await documentoStorageService.SaveAsync(new DocumentoStorageRequest
        {
            EmpresaId = empresaId,
            Modulo = modulo,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            TipoAdjunto = tipoAdjunto,
            NombreArchivo = nombreArchivo,
            ContentType = "application/xml",
            Content = bytes,
            Origen = "Worker"
        }, cancellationToken);

        dbContext.DocumentosAdjuntos.Add(new DocumentoAdjuntoEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Modulo = modulo,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            TipoAdjunto = tipoAdjunto,
            NombreArchivo = nombreArchivo,
            ContentType = "application/xml",
            RutaStorage = storage.RutaStorage,
            HashSHA256 = storage.HashSHA256,
            TamanoBytes = storage.TamanoBytes,
            EsCifrado = storage.EsCifrado,
            AlgoritmoCifrado = storage.AlgoritmoCifrado,
            Origen = "Worker",
            EsActivo = true,
            Version = 1,
            FechaCreacion = DateTimeOffset.UtcNow
        });
    }

}

