using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Facturacion.Models;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriFacturaProcessor : ISriFacturaProcessor
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];
    private static readonly TimeSpan PendingAuthorizationRetryDelay = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan NetworkRetryDelay = TimeSpan.FromSeconds(30);

    private readonly TestDeIaDbContext dbContext;
    private readonly SriFacturaXmlSchemaValidator xmlSchemaValidator;
    private readonly SriXadesBesSigner signer;
    private readonly SriSoapClient soapClient;
    private readonly SriResponseParser responseParser;

    public SriFacturaProcessor(
        TestDeIaDbContext dbContext,
        SriFacturaXmlSchemaValidator xmlSchemaValidator,
        SriXadesBesSigner signer,
        SriSoapClient soapClient,
        SriResponseParser responseParser)
    {
        this.dbContext = dbContext;
        this.xmlSchemaValidator = xmlSchemaValidator;
        this.signer = signer;
        this.soapClient = soapClient;
        this.responseParser = responseParser;
    }

    public async Task<SriFacturaProcessingResult> ProcessAsync(Factura factura, CancellationToken cancellationToken = default)
    {
        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == factura.EmpresaId && current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la empresa emisora activa para firmar la factura.");

        if (empresa.ModoDesarrollo)
        {
            return new SriFacturaProcessingResult
            {
                EstadoFinal = FacturaEstado.AUTORIZADO,
                ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
                NumeroAutorizacion = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}{factura.Secuencial:000000000}",
                XmlGenerado = factura.XmlGenerado,
                XmlFirmado = factura.XmlGenerado,
                Mensaje = "Comprobante autorizado por la simulacion interna de desarrollo.",
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }

        if (string.IsNullOrWhiteSpace(factura.ClaveAcceso))
        {
            throw new InvalidOperationException("La factura no tiene una clave de acceso generada.");
        }

        if (string.IsNullOrWhiteSpace(factura.XmlGenerado))
        {
            throw new InvalidOperationException("La factura no tiene un XML base generado.");
        }

        if (factura.Estado == FacturaEstado.PENDIENTE &&
            !string.IsNullOrWhiteSpace(factura.XmlFirmado))
        {
            return await ContinuePendingAuthorizationAsync(factura, cancellationToken);
        }

        if (factura.Detalles.Any(detalle => !SupportedIvaRates.Contains(detalle.PorcentajeIva)))
        {
            return BuildRejectedResult(
                factura,
                "El comprobante contiene una tarifa de IVA no soportada por el motor tributario actual.",
                null);
        }

        try
        {
            xmlSchemaValidator.Validate(factura.XmlGenerado);
        }
        catch (InvalidOperationException exception)
        {
            return BuildRejectedResult(factura, exception.Message, null);
        }

        if (empresa.CertificadoContenido is null || empresa.CertificadoContenido.Length == 0)
        {
            return BuildUnsignedResult(factura, "La empresa activa no tiene un certificado .p12 cargado para firmar el comprobante.");
        }

        if (string.IsNullOrWhiteSpace(empresa.CertificadoClave))
        {
            return BuildUnsignedResult(factura, "La empresa activa no tiene configurada la clave del certificado .p12.");
        }

        try
        {
            var xmlFirmado = signer.Sign(factura.XmlGenerado, empresa.CertificadoContenido, empresa.CertificadoClave);
            var xmlFirmadoBytes = Encoding.UTF8.GetBytes(xmlFirmado);
            var recepcionResponse = await soapClient.ValidarComprobanteAsync(factura.AmbienteSri, xmlFirmadoBytes, cancellationToken);

            if (string.Equals(recepcionResponse.Estado, "RECIBIDA", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var autorizacionResponse = await soapClient.ConsultarAutorizacionAsync(factura.AmbienteSri, factura.ClaveAcceso, cancellationToken);
                    var mensajesAutorizacion = JoinMensajes(autorizacionResponse.Mensajes);

                    if (string.Equals(autorizacionResponse.Estado, "AUTORIZADO", StringComparison.OrdinalIgnoreCase))
                    {
                        return new SriFacturaProcessingResult
                        {
                            EstadoFinal = FacturaEstado.AUTORIZADO,
                            ClaveAcceso = factura.ClaveAcceso,
                            NumeroAutorizacion = autorizacionResponse.NumeroAutorizacion,
                            XmlGenerado = factura.XmlGenerado,
                            XmlFirmado = BuildAuthorizedXmlPackage(
                                autorizacionResponse,
                                factura.AmbienteSri,
                                xmlFirmado),
                            Mensaje = string.IsNullOrWhiteSpace(mensajesAutorizacion)
                                ? "Comprobante autorizado por el SRI."
                                : $"Comprobante autorizado por el SRI. {mensajesAutorizacion}",
                            FechaRespuesta = autorizacionResponse.FechaAutorizacion ?? DateTimeOffset.UtcNow
                        };
                    }

                    if (string.Equals(autorizacionResponse.Estado, "NO AUTORIZADO", StringComparison.OrdinalIgnoreCase))
                    {
                        return BuildRejectedResult(
                            factura,
                            string.IsNullOrWhiteSpace(mensajesAutorizacion)
                                ? "El SRI devolvio el comprobante como NO AUTORIZADO."
                                : $"El SRI devolvio el comprobante como NO AUTORIZADO. {mensajesAutorizacion}",
                            responseParser.BuildAuditJson("Autorizacion", autorizacionResponse.Estado, autorizacionResponse.Mensajes),
                            xmlFirmado);
                    }

                    return new SriFacturaProcessingResult
                    {
                        EstadoFinal = FacturaEstado.PENDIENTE,
                        ClaveAcceso = factura.ClaveAcceso,
                        XmlGenerado = factura.XmlGenerado,
                        XmlFirmado = xmlFirmado,
                        Mensaje = string.IsNullOrWhiteSpace(mensajesAutorizacion)
                            ? $"Comprobante recibido por el SRI y pendiente de autorizacion. Estado actual: {autorizacionResponse.Estado}."
                            : $"Comprobante recibido por el SRI y pendiente de autorizacion. Estado actual: {autorizacionResponse.Estado}. {mensajesAutorizacion}",
                        AuditoriaJson = responseParser.BuildAuditJson("Autorizacion", autorizacionResponse.Estado, autorizacionResponse.Mensajes),
                        RetryDelay = PendingAuthorizationRetryDelay,
                        FechaRespuesta = DateTimeOffset.UtcNow
                    };
                }
                catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
                {
                    return BuildPendingNetworkResult(
                        factura,
                        xmlFirmado,
                        $"Timeout consultando autorizacion SRI: {exception.Message}");
                }
                catch (HttpRequestException exception)
                {
                    return BuildPendingNetworkResult(
                        factura,
                        xmlFirmado,
                        $"Error de red consultando autorizacion SRI: {exception.Message}");
                }
            }

            var mensajesRecepcion = JoinMensajes(recepcionResponse.Mensajes);

            if (string.Equals(recepcionResponse.Estado, "DEVUELTA", StringComparison.OrdinalIgnoreCase))
            {
                return BuildRejectedResult(
                    factura,
                    string.IsNullOrWhiteSpace(mensajesRecepcion)
                        ? "El SRI devolvio el comprobante en recepcion."
                        : $"El SRI devolvio el comprobante en recepcion. {mensajesRecepcion}",
                    responseParser.BuildAuditJson("Recepcion", recepcionResponse.Estado, recepcionResponse.Mensajes),
                    xmlFirmado);
            }

            return new SriFacturaProcessingResult
            {
                EstadoFinal = FacturaEstado.PENDIENTE,
                ClaveAcceso = factura.ClaveAcceso,
                XmlGenerado = factura.XmlGenerado,
                XmlFirmado = xmlFirmado,
                Mensaje = string.IsNullOrWhiteSpace(mensajesRecepcion)
                    ? $"XML firmado correctamente. Estado de recepcion SRI: {recepcionResponse.Estado}. Se mantiene pendiente para reintento."
                    : $"XML firmado correctamente. Estado de recepcion SRI: {recepcionResponse.Estado}. {mensajesRecepcion}",
                AuditoriaJson = responseParser.BuildAuditJson("Recepcion", recepcionResponse.Estado, recepcionResponse.Mensajes),
                RetryDelay = PendingAuthorizationRetryDelay,
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return BuildPendingNetworkResult(
                factura,
                factura.XmlFirmado ?? factura.XmlGenerado ?? string.Empty,
                $"Timeout comunicandose con el SRI: {exception.Message}");
        }
        catch (HttpRequestException exception)
        {
            return BuildPendingNetworkResult(
                factura,
                factura.XmlFirmado ?? factura.XmlGenerado ?? string.Empty,
                $"Error de red comunicandose con el SRI: {exception.Message}");
        }
        catch (CryptographicException exception)
        {
            return BuildUnsignedResult(factura, $"No se pudo firmar el comprobante con el certificado .p12. Detalle: {exception.Message}");
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or FormatException)
        {
            return BuildUnsignedResult(factura, $"La firma XAdES-BES no pudo generarse. Detalle: {exception.Message}");
        }
    }

    private static SriFacturaProcessingResult BuildUnsignedResult(Factura factura, string mensaje)
    {
        return new SriFacturaProcessingResult
        {
            EstadoFinal = FacturaEstado.NO_FIRMADO,
            ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
            XmlGenerado = factura.XmlGenerado,
            Mensaje = mensaje,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }

    private static SriFacturaProcessingResult BuildRejectedResult(Factura factura, string mensaje, string? auditoriaJson, string? xmlFirmado = null)
    {
        return new SriFacturaProcessingResult
        {
            EstadoFinal = FacturaEstado.RECHAZADO,
            ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
            XmlGenerado = factura.XmlGenerado,
            XmlFirmado = xmlFirmado,
            Mensaje = mensaje,
            AuditoriaJson = auditoriaJson,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }

    private static SriFacturaProcessingResult BuildPendingNetworkResult(Factura factura, string xmlFirmado, string mensaje)
    {
        return new SriFacturaProcessingResult
        {
            EstadoFinal = FacturaEstado.PENDIENTE,
            ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
            XmlGenerado = factura.XmlGenerado,
            XmlFirmado = string.IsNullOrWhiteSpace(xmlFirmado) ? factura.XmlFirmado : xmlFirmado,
            Mensaje = mensaje,
            AuditoriaJson = $$"""{"Servicio":"Red","Estado":"PENDIENTE","Mensajes":[{"Tipo":"ERROR","Mensaje":"{{EscapeJson(mensaje)}}"}]}""",
            RetryDelay = NetworkRetryDelay,
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }

    private async Task<SriFacturaProcessingResult> ContinuePendingAuthorizationAsync(Factura factura, CancellationToken cancellationToken)
    {
        try
        {
            var autorizacionResponse = await soapClient.ConsultarAutorizacionAsync(factura.AmbienteSri, factura.ClaveAcceso ?? string.Empty, cancellationToken);
            var mensajesAutorizacion = JoinMensajes(autorizacionResponse.Mensajes);

            if (string.Equals(autorizacionResponse.Estado, "AUTORIZADO", StringComparison.OrdinalIgnoreCase))
            {
                return new SriFacturaProcessingResult
                {
                    EstadoFinal = FacturaEstado.AUTORIZADO,
                    ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
                    NumeroAutorizacion = autorizacionResponse.NumeroAutorizacion,
                    XmlGenerado = factura.XmlGenerado,
                    XmlFirmado = BuildAuthorizedXmlPackage(
                        autorizacionResponse,
                        factura.AmbienteSri,
                        factura.XmlFirmado ?? factura.XmlGenerado ?? string.Empty),
                    Mensaje = string.IsNullOrWhiteSpace(mensajesAutorizacion)
                        ? "Comprobante autorizado por el SRI."
                        : $"Comprobante autorizado por el SRI. {mensajesAutorizacion}",
                    FechaRespuesta = autorizacionResponse.FechaAutorizacion ?? DateTimeOffset.UtcNow
                };
            }

            if (string.Equals(autorizacionResponse.Estado, "NO AUTORIZADO", StringComparison.OrdinalIgnoreCase))
            {
                return BuildRejectedResult(
                    factura,
                    string.IsNullOrWhiteSpace(mensajesAutorizacion)
                        ? "El SRI devolvio el comprobante como NO AUTORIZADO."
                        : $"El SRI devolvio el comprobante como NO AUTORIZADO. {mensajesAutorizacion}",
                    responseParser.BuildAuditJson("Autorizacion", autorizacionResponse.Estado, autorizacionResponse.Mensajes),
                    factura.XmlFirmado);
            }

            return new SriFacturaProcessingResult
            {
                EstadoFinal = FacturaEstado.PENDIENTE,
                ClaveAcceso = factura.ClaveAcceso ?? string.Empty,
                XmlGenerado = factura.XmlGenerado,
                XmlFirmado = factura.XmlFirmado,
                Mensaje = string.IsNullOrWhiteSpace(mensajesAutorizacion)
                    ? "El comprobante sigue pendiente de autorizacion en el SRI."
                    : $"El comprobante sigue pendiente de autorizacion en el SRI. {mensajesAutorizacion}",
                AuditoriaJson = responseParser.BuildAuditJson("Autorizacion", string.IsNullOrWhiteSpace(autorizacionResponse.Estado) ? "PENDIENTE" : autorizacionResponse.Estado, autorizacionResponse.Mensajes),
                RetryDelay = PendingAuthorizationRetryDelay,
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return BuildPendingNetworkResult(
                factura,
                factura.XmlFirmado ?? factura.XmlGenerado ?? string.Empty,
                $"Timeout consultando autorizacion SRI: {exception.Message}");
        }
        catch (HttpRequestException exception)
        {
            return BuildPendingNetworkResult(
                factura,
                factura.XmlFirmado ?? factura.XmlGenerado ?? string.Empty,
                $"Error de red consultando autorizacion SRI: {exception.Message}");
        }
    }

    private static string JoinMensajes(IReadOnlyCollection<SriSoapMensaje> mensajes)
    {
        return string.Join(" || ", mensajes
            .Select(current => current.ToString())
            .Where(current => !string.IsNullOrWhiteSpace(current)));
    }

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static string BuildAuthorizedXmlPackage(
        SriAutorizacionSoapResponse autorizacionResponse,
        string ambienteSri,
        string xmlFirmadoFallback)
    {
        var comprobanteXml = string.IsNullOrWhiteSpace(autorizacionResponse.ComprobanteXml)
            ? xmlFirmadoFallback
            : autorizacionResponse.ComprobanteXml;

        var ambienteCodigo = SriFacturaXmlBuilder.GetAmbienteCode(ambienteSri);
        var autorizacion = new XElement("autorizacion",
            new XElement("estado", string.IsNullOrWhiteSpace(autorizacionResponse.Estado) ? "AUTORIZADO" : autorizacionResponse.Estado),
            new XElement("numeroAutorizacion", autorizacionResponse.NumeroAutorizacion ?? string.Empty),
            new XElement("fechaAutorizacion", autorizacionResponse.FechaAutorizacion?.ToString("yyyy-MM-ddTHH:mm:ssK") ?? string.Empty),
            new XElement("ambiente", ambienteCodigo),
            new XElement("comprobante", new XCData(comprobanteXml)));

        if (autorizacionResponse.Mensajes.Count > 0)
        {
            autorizacion.Add(new XElement("mensajes",
                autorizacionResponse.Mensajes.Select(current => new XElement("mensaje",
                    new XElement("identificador", current.Identificador ?? string.Empty),
                    new XElement("mensaje", current.Mensaje ?? string.Empty),
                    new XElement("informacionAdicional", current.InformacionAdicional ?? string.Empty),
                    new XElement("tipo", current.Tipo ?? string.Empty)))));
        }

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("autorizacionComprobante", autorizacion)).ToString(SaveOptions.DisableFormatting);
    }
}
