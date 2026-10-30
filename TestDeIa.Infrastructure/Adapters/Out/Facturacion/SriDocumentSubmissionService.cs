using System.Text;
using System.Xml.Linq;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

// Solo transporte de un XML ya firmado. No construye ni adapta estructuras tributarias.
public sealed class SriDocumentSubmissionService(SriSoapClient soap)
{
    internal async Task<SriDocumentSubmissionResult> SubmitAsync(string ambiente, string clave, string signedXml, bool consultarPrimero, CancellationToken ct)
    {
        SriSoapClient.ValidateEnvironment(ambiente, Encoding.UTF8.GetBytes(signedXml));
        if (!HasSignature(signedXml))
            throw new InvalidOperationException("El comprobante no contiene firma electrónica real.");
        if (XDocument.Parse(signedXml).Root?.Element("infoTributaria")?.Element("claveAcceso")?.Value != clave)
            throw new InvalidOperationException("La clave del XML firmado no coincide con el documento persistido.");
        if (consultarPrimero)
        {
            var previous = await soap.ConsultarAutorizacionAsync(ambiente, clave, ct);
            if (previous.Estado is "AUTORIZADO" or "NO AUTORIZADO" or "NO_AUTORIZADO") return Map(previous, signedXml, clave);
        }
        var reception = await soap.ValidarComprobanteAsync(ambiente, Encoding.UTF8.GetBytes(signedXml), ct);
        var duplicate = reception.Mensajes.Count > 0 && reception.Mensajes.All(x => x.Identificador is "43" or "70");
        if (reception.Estado != "RECIBIDA" && !duplicate)
            return new("DEVUELTA", Message(reception.Mensajes, "El SRI devolvió el comprobante en recepción."), null, null, null);
        return Map(await soap.ConsultarAutorizacionAsync(ambiente, clave, ct), signedXml, clave);
    }

    private static SriDocumentSubmissionResult Map(SriAutorizacionSoapResponse response, string signedXml, string clave)
    {
        if (response.Estado == "AUTORIZADO")
        {
            if (response.ClaveAcceso != clave)
                throw new InvalidOperationException("La respuesta SRI corresponde a una clave de acceso diferente.");
            if (string.IsNullOrWhiteSpace(response.NumeroAutorizacion) || !response.FechaAutorizacion.HasValue)
                throw new InvalidOperationException("Respuesta AUTORIZADO incompleta: falta número o fecha de autorización.");
            var authorized = new XElement("autorizacion", new XElement("estado", "AUTORIZADO"),
                new XElement("numeroAutorizacion", response.NumeroAutorizacion),
                new XElement("fechaAutorizacion", response.FechaAutorizacion.Value.ToString("O")),
                new XElement("comprobante", new XCData(response.ComprobanteXml ?? signedXml)));
            return new("AUTORIZADO", Message(response.Mensajes, "Comprobante autorizado por el SRI."), response.NumeroAutorizacion, response.FechaAutorizacion, authorized.ToString(SaveOptions.DisableFormatting));
        }
        return new(response.Estado is "NO AUTORIZADO" or "NO_AUTORIZADO" ? "NO_AUTORIZADO" : "EN_PROCESO",
            Message(response.Mensajes, "Comprobante pendiente de autorización del SRI."), null, null, null);
    }

    internal static string Message(IReadOnlyCollection<SriSoapMensaje> messages, string fallback) =>
        messages.Count == 0 ? fallback : fallback + " " + string.Join(" | ", messages.Select(x => $"{x.Identificador}: {x.Mensaje} {x.InformacionAdicional}"));

    internal static bool HasSignature(string? xml) => !string.IsNullOrWhiteSpace(xml) && XDocument.Parse(xml)
        .Descendants(XName.Get("Signature", "http://www.w3.org/2000/09/xmldsig#")).Any();
}

internal sealed record SriDocumentSubmissionResult(string Estado, string Mensaje, string? NumeroAutorizacion, DateTimeOffset? FechaAutorizacion, string? XmlAutorizado);
