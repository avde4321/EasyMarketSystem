using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Retenciones.Ports.In;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Retenciones;

namespace TestDeIa.Infrastructure.Adapters.Out.Retenciones;

public sealed class RetencionSriTestService(
    TestDeIaDbContext dbContext,
    SriXadesBesSigner signer,
    SriSoapClient soapClient,
    ILogger<RetencionSriTestService> logger) : IRetencionSriTestService
{
    public async Task<RetencionSriTestResultResponse> ValidarXmlRetencionAsync(
        Guid retencionId,
        CancellationToken cancellationToken = default)
    {
        var retencion = await dbContext.ComprobantesRetencion
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == retencionId, cancellationToken);

        if (retencion is null)
        {
            return Failed("XML Retencion SRI", "La retencion no existe o no pertenece a la empresa activa.");
        }

        var xml = BuildDiagnosticXml(retencion);
        var document = XDocument.Parse(xml);
        var errores = new List<string>();

        Require(document, "comprobanteRetencion", errores);
        Require(document, "infoTributaria", errores);
        Require(document, "codDoc", errores, "07");
        Require(document, "ambiente", errores);
        Require(document, "claveAcceso", errores, expectedLength: 49);
        Require(document, "docsSustento", errores);
        Require(document, "retencion", errores);

        var ok = errores.Count == 0;
        logger.LogInformation(
            "Prueba XML retencion {RetencionId}: {Estado}. Errores={Errores}",
            retencionId,
            ok ? "OK" : "FALLIDA",
            string.Join(" | ", errores));

        return new RetencionSriTestResultResponse
        {
            NombrePrueba = "XML Retencion SRI",
            Succeeded = ok,
            Estado = ok ? "VALIDO" : "INVALIDO",
            ClaveAcceso = retencion.ClaveAcceso,
            Mensaje = ok ? "XML diagnostico compatible con estructura base SRI v2.0.0." : "XML diagnostico con observaciones.",
            PayloadResumen = Trim(xml, 900),
            Errores = errores
        };
    }

    public Task<RetencionSriTestResultResponse> ValidarFirmaXadesBesAsync(
        string xmlGenerado,
        byte[] certificadoP12,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var signed = signer.Sign(xmlGenerado, certificadoP12, password);
            var containsSignature = signed.Contains("<Signature", StringComparison.OrdinalIgnoreCase) ||
                signed.Contains(":Signature", StringComparison.OrdinalIgnoreCase);

            return Task.FromResult(new RetencionSriTestResultResponse
            {
                NombrePrueba = "Firma XAdES-BES",
                Succeeded = containsSignature,
                Estado = containsSignature ? "FIRMADO" : "SIN_FIRMA",
                Mensaje = containsSignature
                    ? "La firma XAdES-BES fue generada y validada criptograficamente de forma local."
                    : "El XML firmado no contiene el nodo Signature.",
                PayloadResumen = Trim(signed, 900),
                Errores = containsSignature ? Array.Empty<string>() : ["No se encontro el nodo Signature."]
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Prueba de firma XAdES-BES fallida.");
            return Task.FromResult(Failed("Firma XAdES-BES", exception.Message));
        }
    }

    public async Task<RetencionSriTestResultResponse> ProbarRecepcionSriPruebasAsync(
        byte[] xmlFirmado,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await soapClient.ValidarComprobanteAsync("1", xmlFirmado, cancellationToken);
            var accepted = string.Equals(response.Estado, "RECIBIDA", StringComparison.OrdinalIgnoreCase);

            return new RetencionSriTestResultResponse
            {
                NombrePrueba = "Recepcion SRI Pruebas",
                Succeeded = accepted,
                Estado = response.Estado,
                ClaveAcceso = response.ClaveAcceso,
                Mensaje = accepted ? "El SRI recibio el comprobante." : "El SRI devolvio observaciones en recepcion.",
                PayloadResumen = Trim(response.RawSoapXml, 900),
                Errores = response.Mensajes.Select(current => current.ToString()).Where(current => !string.IsNullOrWhiteSpace(current)).ToArray()
            };
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Prueba de recepcion SRI en ambiente pruebas fallida.");
            return Failed("Recepcion SRI Pruebas", exception.Message);
        }
    }

    public async Task<RetencionSriTestResultResponse> ProbarAutorizacionSriPruebasAsync(
        string claveAcceso,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(claveAcceso) || claveAcceso.Trim().Length != 49)
        {
            return Failed("Autorizacion SRI Pruebas", "La clave de acceso debe tener 49 digitos.");
        }

        try
        {
            var response = await soapClient.ConsultarAutorizacionAsync("1", claveAcceso.Trim(), cancellationToken);
            var authorized = string.Equals(response.Estado, "AUTORIZADO", StringComparison.OrdinalIgnoreCase);

            return new RetencionSriTestResultResponse
            {
                NombrePrueba = "Autorizacion SRI Pruebas",
                Succeeded = authorized,
                Estado = response.Estado,
                ClaveAcceso = response.ClaveAcceso,
                Mensaje = authorized ? "El SRI autorizo el comprobante." : "El SRI aun no autoriza o devolvio observaciones.",
                PayloadResumen = Trim(response.RawSoapXml, 900),
                Errores = response.Mensajes.Select(current => current.ToString()).Where(current => !string.IsNullOrWhiteSpace(current)).ToArray()
            };
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Prueba de autorizacion SRI en ambiente pruebas fallida.");
            return Failed("Autorizacion SRI Pruebas", exception.Message);
        }
    }

    private static void Require(XDocument document, string localName, List<string> errors, string? expectedValue = null, int? expectedLength = null)
    {
        var value = document.Descendants().FirstOrDefault(current => current.Name.LocalName == localName)?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Nodo obligatorio ausente: {localName}.");
            return;
        }

        if (expectedValue is not null && !string.Equals(value, expectedValue, StringComparison.Ordinal))
        {
            errors.Add($"Nodo {localName} debe ser {expectedValue} y llego {value}.");
        }

        if (expectedLength.HasValue && value.Length != expectedLength.Value)
        {
            errors.Add($"Nodo {localName} debe tener {expectedLength.Value} caracteres y tiene {value.Length}.");
        }
    }

    private static string BuildDiagnosticXml(Persistence.Entities.ComprobanteRetencionEntity retencion)
    {
        var clave = string.IsNullOrWhiteSpace(retencion.ClaveAcceso)
            ? new string('0', 49)
            : retencion.ClaveAcceso;
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        builder.AppendLine("""<comprobanteRetencion id="comprobante" version="2.0.0">""");
        builder.AppendLine("  <infoTributaria>");
        builder.AppendLine($"    <ambiente>{(byte)retencion.AmbienteSRI}</ambiente>");
        builder.AppendLine("    <tipoEmision>1</tipoEmision>");
        builder.AppendLine("    <codDoc>07</codDoc>");
        builder.AppendLine($"    <estab>{retencion.Establecimiento}</estab>");
        builder.AppendLine($"    <ptoEmi>{retencion.PuntoEmision}</ptoEmi>");
        builder.AppendLine($"    <secuencial>{retencion.Secuencial}</secuencial>");
        builder.AppendLine($"    <claveAcceso>{clave}</claveAcceso>");
        builder.AppendLine("  </infoTributaria>");
        builder.AppendLine("  <docsSustento>");
        foreach (var detail in retencion.Detalles)
        {
            builder.AppendLine("    <docSustento>");
            builder.AppendLine($"      <codDocSustento>{detail.CodDocSustento}</codDocSustento>");
            builder.AppendLine($"      <numDocSustento>{detail.NumDocSustento.Replace("-", string.Empty, StringComparison.Ordinal)}</numDocSustento>");
            builder.AppendLine("      <retenciones>");
            builder.AppendLine("        <retencion>");
            builder.AppendLine($"          <codigo>{detail.CodigoImpuesto}</codigo>");
            builder.AppendLine($"          <codigoRetencion>{detail.CodigoRetencionSRI}</codigoRetencion>");
            builder.AppendLine($"          <baseImponible>{detail.BaseImponible:0.00}</baseImponible>");
            builder.AppendLine($"          <porcentajeRetener>{detail.PorcentajeRetencion:0.00}</porcentajeRetener>");
            builder.AppendLine($"          <valorRetenido>{detail.ValorRetenido:0.00}</valorRetenido>");
            builder.AppendLine("        </retencion>");
            builder.AppendLine("      </retenciones>");
            builder.AppendLine("    </docSustento>");
        }

        builder.AppendLine("  </docsSustento>");
        builder.AppendLine("</comprobanteRetencion>");
        return builder.ToString();
    }

    private static RetencionSriTestResultResponse Failed(string testName, string message)
    {
        return new RetencionSriTestResultResponse
        {
            NombrePrueba = testName,
            Succeeded = false,
            Estado = "FALLIDA",
            Mensaje = message,
            Errores = [message]
        };
    }

    private static string Trim(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
