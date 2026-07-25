using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriSoapClient
{
    private static readonly XNamespace SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace RecepcionNamespace = "http://ec.gob.sri.ws.recepcion";
    private static readonly XNamespace AutorizacionNamespace = "http://ec.gob.sri.ws.autorizacion";

    private readonly HttpClient httpClient;
    private readonly SriUrlResolverService urlResolverService;
    private readonly SriResponseParser responseParser;

    public SriSoapClient(HttpClient httpClient, SriUrlResolverService urlResolverService, SriResponseParser responseParser)
    {
        this.httpClient = httpClient;
        this.urlResolverService = urlResolverService;
        this.responseParser = responseParser;
    }

    internal async Task<SriRecepcionSoapResponse> ValidarComprobanteAsync(
        string ambienteSri,
        byte[] xmlBytes,
        CancellationToken cancellationToken = default)
    {
        var endpoint = urlResolverService.Resolve(ambienteSri).RecepcionUrl;

        var body = new XElement(
            RecepcionNamespace + "validarComprobante",
            new XElement(RecepcionNamespace + "xml", Convert.ToBase64String(xmlBytes)));

        var responseContent = await SendSoapAsync(endpoint, body, cancellationToken);
        return responseParser.ParseRecepcion(responseContent);
    }

    internal async Task<SriAutorizacionSoapResponse> ConsultarAutorizacionAsync(
        string ambienteSri,
        string claveAcceso,
        CancellationToken cancellationToken = default)
    {
        var endpoint = urlResolverService.Resolve(ambienteSri).AutorizacionUrl;

        var body = new XElement(
            AutorizacionNamespace + "autorizacionComprobante",
            new XElement(AutorizacionNamespace + "claveAccesoComprobante", claveAcceso));

        var responseContent = await SendSoapAsync(endpoint, body, cancellationToken);
        return responseParser.ParseAutorizacion(responseContent);
    }

    private async Task<string> SendSoapAsync(string endpoint, XElement bodyContent, CancellationToken cancellationToken)
    {
        var envelope = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(
                SoapEnvelopeNamespace + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", SoapEnvelopeNamespace),
                new XElement(SoapEnvelopeNamespace + "Header"),
                new XElement(SoapEnvelopeNamespace + "Body", bodyContent)));

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(envelope.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml")
        };

        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
