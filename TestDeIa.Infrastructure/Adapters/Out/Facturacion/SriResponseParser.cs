using System.Text.Json;
using System.Xml.Linq;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriResponseParser
{
    internal SriRecepcionSoapResponse ParseRecepcion(string soapXml)
    {
        var document = ParseDocument(soapXml);
        var responseNode = document
            .Descendants()
            .FirstOrDefault(current => current.Name.LocalName == "RespuestaRecepcionComprobante")
            ?? throw new InvalidOperationException("El SRI no devolvio una respuesta valida en el servicio de recepcion.");

        var estado = responseNode.Elements().FirstOrDefault(current => current.Name.LocalName == "estado")?.Value?.Trim() ?? string.Empty;
        var mensajes = responseNode
            .Descendants()
            .Where(current => current.Name.LocalName == "mensaje")
            .Select(ParseMensaje)
            .ToArray();

        var claveAcceso = responseNode
            .Descendants()
            .FirstOrDefault(current => current.Name.LocalName == "claveAcceso")
            ?.Value?
            .Trim();

        return new SriRecepcionSoapResponse(estado, claveAcceso, mensajes, soapXml);
    }

    internal SriAutorizacionSoapResponse ParseAutorizacion(string soapXml)
    {
        var document = ParseDocument(soapXml);
        var responseNode = document
            .Descendants()
            .FirstOrDefault(current => current.Name.LocalName == "RespuestaAutorizacionComprobante")
            ?? throw new InvalidOperationException("El SRI no devolvio una respuesta valida en el servicio de autorizacion.");

        var autorizacion = responseNode
            .Descendants()
            .FirstOrDefault(current => current.Name.LocalName == "autorizacion");

        if (autorizacion is null)
        {
            var claveConsultada = responseNode.Elements().FirstOrDefault(current => current.Name.LocalName == "claveAccesoConsultada")?.Value?.Trim() ?? string.Empty;
            return new SriAutorizacionSoapResponse(string.Empty, claveConsultada, null, null, null, [], soapXml);
        }

        var estado = autorizacion.Elements().FirstOrDefault(current => current.Name.LocalName == "estado")?.Value?.Trim() ?? string.Empty;
        var numeroAutorizacion = autorizacion.Elements().FirstOrDefault(current => current.Name.LocalName == "numeroAutorizacion")?.Value?.Trim();
        var comprobante = autorizacion.Elements().FirstOrDefault(current => current.Name.LocalName == "comprobante")?.Value;
        var mensajes = autorizacion
            .Descendants()
            .Where(current => current.Name.LocalName == "mensaje")
            .Select(ParseMensaje)
            .ToArray();

        DateTimeOffset? fechaAutorizacion = null;
        var fechaText = autorizacion.Elements().FirstOrDefault(current => current.Name.LocalName == "fechaAutorizacion")?.Value?.Trim();
        if (DateTimeOffset.TryParse(fechaText, out var parsedDate))
        {
            fechaAutorizacion = parsedDate;
        }

        var claveAcceso = responseNode.Elements().FirstOrDefault(current => current.Name.LocalName == "claveAccesoConsultada")?.Value?.Trim() ?? string.Empty;
        return new SriAutorizacionSoapResponse(estado, claveAcceso, numeroAutorizacion, fechaAutorizacion, comprobante, mensajes, soapXml);
    }

    internal string BuildAuditJson(string servicio, string estado, IReadOnlyCollection<SriSoapMensaje> mensajes)
    {
        var payload = new SriSoapAuditPayload(
            servicio,
            estado,
            mensajes.Select(current => new SriSoapAuditMessage(
                TrimForAudit(current.Identificador, 32),
                TrimForAudit(current.Mensaje, 120),
                TrimForAudit(current.InformacionAdicional, 120),
                TrimForAudit(current.Tipo, 32)))
            .ToArray());

        return JsonSerializer.Serialize(payload);
    }

    private static XDocument ParseDocument(string soapXml)
    {
        if (string.IsNullOrWhiteSpace(soapXml))
        {
            throw new InvalidOperationException("El servicio SOAP del SRI devolvio una carga vacia.");
        }

        return XDocument.Parse(soapXml, LoadOptions.None);
    }

    private static SriSoapMensaje ParseMensaje(XElement mensajeNode)
    {
        return new SriSoapMensaje(
            mensajeNode.Elements().FirstOrDefault(current => current.Name.LocalName == "identificador")?.Value?.Trim(),
            mensajeNode.Elements().FirstOrDefault(current => current.Name.LocalName == "mensaje")?.Value?.Trim(),
            mensajeNode.Elements().FirstOrDefault(current => current.Name.LocalName == "informacionAdicional")?.Value?.Trim(),
            mensajeNode.Elements().FirstOrDefault(current => current.Name.LocalName == "tipo")?.Value?.Trim());
    }

    private static string? TrimForAudit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }
}

internal sealed record SriRecepcionSoapResponse(
    string Estado,
    string? ClaveAcceso,
    IReadOnlyCollection<SriSoapMensaje> Mensajes,
    string RawSoapXml);

internal sealed record SriAutorizacionSoapResponse(
    string Estado,
    string ClaveAcceso,
    string? NumeroAutorizacion,
    DateTimeOffset? FechaAutorizacion,
    string? ComprobanteXml,
    IReadOnlyCollection<SriSoapMensaje> Mensajes,
    string RawSoapXml);

internal sealed record SriSoapMensaje(
    string? Identificador,
    string? Mensaje,
    string? InformacionAdicional,
    string? Tipo)
{
    public override string ToString()
    {
        var parts = new[]
        {
            Identificador,
            Mensaje,
            InformacionAdicional,
            Tipo
        }.Where(current => !string.IsNullOrWhiteSpace(current));

        return string.Join(" | ", parts);
    }
}

internal sealed record SriSoapAuditPayload(
    string Servicio,
    string Estado,
    IReadOnlyCollection<SriSoapAuditMessage> Mensajes);

internal sealed record SriSoapAuditMessage(
    string? Identificador,
    string? Mensaje,
    string? InformacionAdicional,
    string? Tipo);
