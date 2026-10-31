using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Compras.Models;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class SriCompraProcessor : ISriCompraProcessor
{
    private readonly TestDeIaDbContext dbContext;
    private readonly SriLiquidacionCompraXmlValidator validator;
    private readonly SriXadesBesSigner signer;
    private readonly SriDocumentSubmissionService submission;
    private readonly TestDeIa.Infrastructure.Adapters.Out.Sri.SriXmlArtifactService artifacts;

    public SriCompraProcessor(
        TestDeIaDbContext dbContext,
        SriLiquidacionCompraXmlValidator validator,
        SriXadesBesSigner signer,
        SriDocumentSubmissionService submission,
        TestDeIa.Infrastructure.Adapters.Out.Sri.SriXmlArtifactService artifacts)
    {
        this.dbContext = dbContext;
        this.validator = validator;
        this.signer = signer;
        this.submission = submission;
        this.artifacts = artifacts;
    }

    public async Task<SriCompraProcessingResult> ProcessAsync(Compra compra, CancellationToken cancellationToken = default)
    {
        await using var lease = await SriProcessingLease.AcquireAsync(dbContext, compra.EmpresaId, compra.Id, cancellationToken);
        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(current => current.CertificadosDigitales)
            .FirstOrDefaultAsync(current => current.Id == compra.EmpresaId && current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la empresa emisora activa para firmar la liquidacion.");

        if (empresa.ModoDesarrollo)
        {
            return new SriCompraProcessingResult
            {
                EstadoFinal = FacturaEstado.AUTORIZADO,
                ClaveAcceso = compra.ClaveAccesoGenerada ?? string.Empty,
                NumeroAutorizacion = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}{compra.Secuencial}",
                XmlGenerado = compra.XmlGenerado,
                XmlFirmado = compra.XmlGenerado,
                Mensaje = "Liquidacion autorizada por la simulacion interna de desarrollo.",
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }

        if (string.IsNullOrWhiteSpace(compra.ClaveAccesoGenerada))
        {
            throw new InvalidOperationException("La liquidacion no tiene clave de acceso generada.");
        }

        if (string.IsNullOrWhiteSpace(compra.XmlGenerado))
        {
            throw new InvalidOperationException("La liquidacion no tiene XML generado.");
        }

        try
        {
            validator.Validate(compra.XmlGenerado);
        }
        catch (InvalidOperationException exception)
        {
            return BuildRejected(compra, exception.Message);
        }

        var xmlFirmado = compra.XmlFirmado;
        try
        {
            var retry = SriDocumentSubmissionService.HasSignature(xmlFirmado);
            if (!retry)
            {
                var certificado = SriCertificateSelector.Resolve(empresa);
                xmlFirmado = signer.Sign(compra.XmlGenerado, certificado.Contenido, certificado.Clave);
                var persisted = await dbContext.Compras.SingleAsync(x => x.EmpresaId == compra.EmpresaId && x.Id == compra.Id, cancellationToken);
                persisted.XmlFirmado = xmlFirmado;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            await artifacts.SaveAsync(compra.EmpresaId, "COMPRAS", "Compra", compra.Id, "XML_GENERADO", $"03-{compra.Id}-generado.xml", compra.XmlGenerado, cancellationToken);
            await artifacts.SaveAsync(compra.EmpresaId, "COMPRAS", "Compra", compra.Id, "XML_FIRMADO", $"03-{compra.Id}-firmado.xml", xmlFirmado, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var ambiente = compra.ClaveAccesoGenerada.Substring(23, 1);
            var result = await submission.SubmitAsync(ambiente, compra.ClaveAccesoGenerada, xmlFirmado!, retry, cancellationToken);
            await artifacts.SaveAsync(compra.EmpresaId, "COMPRAS", "Compra", compra.Id, "XML_AUTORIZADO", $"03-{compra.Id}-autorizado.xml", result.XmlAutorizado, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new SriCompraProcessingResult
            {
                EstadoFinal = result.Estado == "AUTORIZADO" ? FacturaEstado.AUTORIZADO : result.Estado is "DEVUELTA" or "NO_AUTORIZADO" ? FacturaEstado.RECHAZADO : FacturaEstado.PENDIENTE,
                NumeroAutorizacion = result.NumeroAutorizacion,
                ClaveAcceso = compra.ClaveAccesoGenerada,
                XmlGenerado = compra.XmlGenerado,
                XmlFirmado = xmlFirmado,
                Mensaje = result.Mensaje,
                FechaRespuesta = result.FechaAutorizacion ?? DateTimeOffset.UtcNow
            };
        }
        catch (Exception exception) when ((exception is HttpRequestException or TaskCanceledException) && !cancellationToken.IsCancellationRequested)
        {
            return new SriCompraProcessingResult { EstadoFinal = FacturaEstado.PENDIENTE, ClaveAcceso = compra.ClaveAccesoGenerada, XmlGenerado = compra.XmlGenerado, XmlFirmado = xmlFirmado, Mensaje = "Comunicación SRI pendiente: " + exception.Message };
        }
        catch (CryptographicException exception)
        {
            return BuildUnsigned(compra, $"No se pudo firmar la liquidacion con el certificado .p12. Detalle: {exception.Message}");
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or FormatException)
        {
            return BuildUnsigned(compra, $"La firma XAdES-BES de la liquidacion no pudo generarse. Detalle: {exception.Message}");
        }
    }

    private static SriCompraProcessingResult BuildUnsigned(Compra compra, string mensaje)
    {
        return new SriCompraProcessingResult
        {
            EstadoFinal = FacturaEstado.NO_FIRMADO,
            ClaveAcceso = compra.ClaveAccesoGenerada ?? string.Empty,
            XmlGenerado = compra.XmlGenerado,
            Mensaje = mensaje,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }

    private static SriCompraProcessingResult BuildRejected(Compra compra, string mensaje)
    {
        return new SriCompraProcessingResult
        {
            EstadoFinal = FacturaEstado.RECHAZADO,
            ClaveAcceso = compra.ClaveAccesoGenerada ?? string.Empty,
            XmlGenerado = compra.XmlGenerado,
            Mensaje = mensaje,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }
}
