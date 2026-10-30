using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Application.Modules.Sri.Ports.In;
using TestDeIa.Application.Modules.Sri.Ports.Out;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Sri;

public sealed class SriComprobanteProcessor(
    TestDeIaDbContext dbContext,
    IDocumentoStorageService documentoStorageService,
    ISriStateEngineService sriStateEngineService,
    SriXadesBesSigner signer,
    SriDocumentSubmissionService submission,
    SriNotaCreditoXmlSchemaValidator schemaValidator,
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

        if (item.TipoDocumentoId == "01")
        {
            // Compatibilidad: la factura pertenece a su coordinador propio, aquí solo se archiva.
            var factura = await dbContext.Facturas.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.EmpresaId == item.EmpresaId && x.Id == item.ComprobanteId, cancellationToken)
                ?? throw new InvalidOperationException("No existe la factura de esta empresa.");
            await PersistExistingXmlArtifactsAsync(item, cancellationToken);
            var complete = factura.Estado == FacturaEstado.AUTORIZADO && !string.IsNullOrWhiteSpace(factura.NumeroAutorizacion);
            item.Estado = complete ? SriOutboxEstados.Autorizado : SriOutboxEstados.Pendiente;
            item.Mensaje = complete ? "Artefactos archivados de factura ya autorizada por su procesador." : "La factura continúa en su procesador específico; no se altera su estado.";
            item.NextRetryAt = complete ? null : DateTimeOffset.UtcNow.AddMinutes(1);
            item.ProcessingNode = null;
            item.ProcessingStartedAt = null;
            return new SriOutboxProcessingResult { Succeeded = complete, Message = item.Mensaje };
        }
        if (item.TipoDocumentoId != "04")
            throw new InvalidOperationException("Este procesador admite notas de crédito (04). Los demás documentos tienen su procesador propio.");
        await using var lease = await SriProcessingLease.AcquireAsync(dbContext, item.EmpresaId, item.ComprobanteId, cancellationToken);

        var document = await dbContext.ComprobanteCabecera.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.EmpresaId == item.EmpresaId && x.Id == item.ComprobanteId && x.TipoDocumentoId == "04", cancellationToken)
            ?? throw new InvalidOperationException("No se encontró la nota de crédito de la empresa.");
        var empresa = await dbContext.EmpresasEmisoras.IgnoreQueryFilters().AsNoTracking().Include(x => x.CertificadosDigitales)
            .SingleOrDefaultAsync(x => x.Id == item.EmpresaId && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No existe una empresa activa para procesar la nota de crédito.");

        if (document.Estado == FacturaEstado.AUTORIZADO && !string.IsNullOrWhiteSpace(document.NumeroAutorizacion))
        {
            item.Estado = SriOutboxEstados.Autorizado;
            item.ProcessingNode = null;
            item.ProcessingStartedAt = null;
            return new SriOutboxProcessingResult { Succeeded = true, Message = "Documento ya autorizado; no se reenvió." };
        }
        if (string.IsNullOrWhiteSpace(document.XmlGenerado))
            throw new InvalidOperationException("La nota de crédito no tiene XML base.");

        string message;
        string finalState;
        if (empresa.ModoDesarrollo)
        {
            document.NumeroAutorizacion = document.ClaveAcceso;
            document.FechaAutorizacion = DateTimeOffset.UtcNow;
            message = "Nota de crédito autorizada por simulación interna de desarrollo. No enviada al SRI.";
            finalState = SriEstadosComprobante.Autorizado;
        }
        else
        {
            schemaValidator.Validate(document.XmlGenerado);
            var retry = SriDocumentSubmissionService.HasSignature(document.XmlFirmado);
            if (!retry)
            {
                var certificate = SriCertificateSelector.Resolve(empresa);
                document.XmlFirmado = signer.Sign(document.XmlGenerado, certificate.Contenido, certificate.Clave);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            await PersistExistingXmlArtifactsAsync(item, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var result = await submission.SubmitAsync(ClaveAccesoService.GetAmbienteCode(document.AmbienteSri),
                document.ClaveAcceso, document.XmlFirmado!, retry, cancellationToken);
            message = result.Mensaje;
            finalState = result.Estado;
            if (result.Estado == SriEstadosComprobante.Autorizado)
            {
                document.NumeroAutorizacion = result.NumeroAutorizacion;
                document.FechaAutorizacion = result.FechaAutorizacion;
                await SaveIfMissingAsync(item.EmpresaId, "SRI", "ComprobanteCabecera", document.Id,
                    "XML_AUTORIZADO", $"04-{document.Establecimiento}-{document.PuntoEmision}-{document.Secuencial:D9}-autorizado.xml",
                    result.XmlAutorizado, cancellationToken);
            }
        }
        if (finalState != SriEstadosComprobante.EnProceso)
            await sriStateEngineService.ApplyTransitionAsync(new SriTransitionRequest
            {
                EmpresaId = item.EmpresaId, ComprobanteId = item.ComprobanteId, TipoDocumentoId = item.TipoDocumentoId,
                EstadoNuevoCodigo = finalState, MensajeRespuesta = message, WorkerNode = workerId
            }, cancellationToken);
        item.Estado = finalState;
        item.Mensaje = message;
        item.UltimoError = finalState is "DEVUELTA" or "NO_AUTORIZADO" ? message : null;
        item.NextRetryAt = finalState == SriEstadosComprobante.EnProceso ? DateTimeOffset.UtcNow.AddSeconds(30) : null;
        item.ProcessingNode = null;
        item.ProcessingStartedAt = null;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        if (finalState == SriEstadosComprobante.EnProceso) document.Estado = FacturaEstado.PENDIENTE;
        logger.LogInformation("Nota de crédito procesada. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}, DocumentoId: {DocumentoId}, Estado: {Estado}",
            item.EmpresaId, null, document.Id, finalState);
        return new SriOutboxProcessingResult { Succeeded = finalState == SriEstadosComprobante.Autorizado,
            Devuelto = finalState is "DEVUELTA" or "NO_AUTORIZADO", Message = message };
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
            EsCifrado = storage.EsCifrado,
            AlgoritmoCifrado = storage.AlgoritmoCifrado,
            Origen = "Worker",
            EsActivo = true,
            Version = 1,
            FechaCreacion = DateTimeOffset.UtcNow
        });
    }
}
