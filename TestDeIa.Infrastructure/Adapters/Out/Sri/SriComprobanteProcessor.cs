using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Application.Modules.Sri.Ports.Out;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Sri;

public sealed class SriComprobanteProcessor(
    TestDeIaDbContext dbContext,
    IDocumentoStorageService documentoStorageService,
    ILogger<SriComprobanteProcessor> logger) : ISriComprobanteProcessor
{
    public async Task<SriOutboxProcessingResult> ProcessAsync(
        Guid colaProcesamientoId,
        string workerId,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ColaProcesamientoSRI
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current =>
                current.Id == colaProcesamientoId &&
                current.ProcessingNode == workerId &&
                current.Estado == SriOutboxEstados.EnProceso,
                cancellationToken)
            ?? throw new InvalidOperationException("No se encontro el registro de outbox SRI tomado por este worker.");

        await PersistExistingXmlArtifactsAsync(item, cancellationToken);

        item.Estado = SriOutboxEstados.Autorizado;
        item.Mensaje = "Registro outbox procesado sin reenviar al SRI. Se conservaron artefactos existentes como DocumentoAdjunto cuando estuvieron disponibles.";
        item.UltimoError = null;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.ProcessingNode = null;
        item.ProcessingStartedAt = null;

        logger.LogInformation(
            "Outbox SRI procesado en modo seguro. Cola={ColaId}; Empresa={EmpresaId}; TipoDocumento={TipoDocumento}; Comprobante={ComprobanteId}.",
            item.Id,
            item.EmpresaId,
            item.TipoDocumentoId,
            item.ComprobanteId);

        return new SriOutboxProcessingResult
        {
            Succeeded = true,
            Message = item.Mensaje
        };
    }

    private async Task PersistExistingXmlArtifactsAsync(ColaProcesamientoSriEntity item, CancellationToken cancellationToken)
    {
        if (item.TipoDocumentoId == "01")
        {
            var factura = await dbContext.Facturas
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(current => current.EmpresaId == item.EmpresaId && current.Id == item.ComprobanteId, cancellationToken);

            if (factura is null)
            {
                return;
            }

            await SaveIfMissingAsync(item.EmpresaId, "SRI", "Factura", factura.Id, "XML_GENERADO", $"FACTURA-{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}-generado.xml", factura.XmlGenerado, cancellationToken);
            await SaveIfMissingAsync(item.EmpresaId, "SRI", "Factura", factura.Id, "XML_FIRMADO", $"FACTURA-{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}-firmado.xml", factura.XmlFirmado, cancellationToken);
            return;
        }

        var comprobante = await dbContext.ComprobanteCabecera
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.EmpresaId == item.EmpresaId && current.Id == item.ComprobanteId, cancellationToken);

        if (comprobante is null)
        {
            return;
        }

        var numero = $"{comprobante.Establecimiento}-{comprobante.PuntoEmision}-{comprobante.Secuencial:000000000}";
        await SaveIfMissingAsync(item.EmpresaId, "SRI", "ComprobanteCabecera", comprobante.Id, "XML_GENERADO", $"{comprobante.TipoDocumentoId}-{numero}-generado.xml", comprobante.XmlGenerado, cancellationToken);
        await SaveIfMissingAsync(item.EmpresaId, "SRI", "ComprobanteCabecera", comprobante.Id, "XML_FIRMADO", $"{comprobante.TipoDocumentoId}-{numero}-firmado.xml", comprobante.XmlFirmado, cancellationToken);
    }

    private async Task SaveIfMissingAsync(
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
            Origen = "Worker",
            EsActivo = true,
            Version = 1,
            FechaCreacion = DateTimeOffset.UtcNow
        });
    }
}
