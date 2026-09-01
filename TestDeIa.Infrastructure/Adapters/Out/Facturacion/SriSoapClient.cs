using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriSoapClient
{
    private static readonly XNamespace SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace RecepcionNamespace = "http://ec.gob.sri.ws.recepcion";
    private static readonly XNamespace AutorizacionNamespace = "http://ec.gob.sri.ws.autorizacion";

    private readonly HttpClient httpClient;
    private readonly SriUrlResolverService urlResolverService;
    private readonly SriResponseParser responseParser;
    private readonly ILogger<SriSoapClient> logger;

    public SriSoapClient(
        HttpClient httpClient,
        SriUrlResolverService urlResolverService,
        SriResponseParser responseParser,
        ILogger<SriSoapClient> logger)
    {
        this.httpClient = httpClient;
        this.urlResolverService = urlResolverService;
        this.responseParser = responseParser;
        this.logger = logger;
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

        return await SendWithRetryAsync(endpoint, request, cancellationToken);
    }

    private async Task<string> SendWithRetryAsync(string endpoint, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation("Intento {Attempt}/{MaxAttempts} hacia Web Service SRI {Endpoint}.", attempt, maxAttempts, endpoint);
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Respuesta HTTP {StatusCode} recibida desde SRI en intento {Attempt}.", (int)response.StatusCode, attempt);
                    return content;
                }

                lastException = new HttpRequestException($"SRI devolvio HTTP {(int)response.StatusCode}: {response.ReasonPhrase}. {content}");
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                lastException = exception;
                logger.LogWarning(exception, "Fallo temporal al consumir SRI en intento {Attempt}/{MaxAttempts}.", attempt, maxAttempts);
            }

            if (attempt < maxAttempts)
            {
                var delay = TimeSpan.FromMilliseconds(350 * attempt * attempt);
                await Task.Delay(delay, cancellationToken);
                request = CloneRequest(request);
            }
        }

        logger.LogError(lastException, "Se agotaron los reintentos contra el Web Service SRI {Endpoint}.", endpoint);
        throw lastException ?? new HttpRequestException("No fue posible consumir el Web Service del SRI.");
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage source)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri);
        foreach (var header in source.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (source.Content is not null)
        {
            var content = source.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            clone.Content = new StringContent(content, Encoding.UTF8, "text/xml");
        }

        return clone;
    }
}
