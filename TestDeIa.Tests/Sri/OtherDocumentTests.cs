using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TestDeIa.Infrastructure.Adapters.Out.Compras;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Adapters.Out.Retenciones;
using TestDeIa.Infrastructure.Options;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Tests.Sri;

public sealed class OtherDocumentTests
{
    private static readonly DateTimeOffset Date = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private static string Key(string type, string ambiente = "1") => new ClaveAccesoService().Generar(Date, type, "0991111111001", ambiente, "001", "001", "000000001", "12345678", "1");

    [Theory]
    [InlineData("factura", "01")]
    [InlineData("notaCredito", "04")]
    [InlineData("liquidacionCompra", "03")]
    [InlineData("comprobanteRetencion", "07")]
    public async Task SharedSignatureAndSoapPreserveEachRoot(string root, string type)
    {
        var xml = $"<{root} id=\"comprobante\"><infoTributaria><ambiente>1</ambiente><claveAcceso>{Key(type)}</claveAcceso></infoTributaria></{root}>";
        var signed = Sign(xml);
        var handler = new FakeSri(Key(type));
        var result = await Submission(handler).SubmitAsync("1", Key(type), signed, false, default);
        Assert.Equal("AUTORIZADO", result.Estado);
        Assert.Equal(root, XDocument.Parse(signed).Root!.Name.LocalName);
        Assert.Equal(1, handler.Receptions);
        Assert.Equal(XNamespace.None, XDocument.Parse(handler.LastReception).Descendants().Single(x => x.Name.LocalName == "xml").Name.Namespace);
        Assert.NotNull(result.XmlAutorizado);
    }

    [Fact]
    public async Task RetryConsultsAuthorizationWithoutResigningOrResending()
    {
        var key = Key("04"); var handler = new FakeSri(key);
        var signed = Sign($"<notaCredito id=\"comprobante\"><infoTributaria><ambiente>1</ambiente><claveAcceso>{key}</claveAcceso></infoTributaria></notaCredito>");
        var result = await Submission(handler).SubmitAsync("1", key, signed, true, default);
        Assert.Equal("AUTORIZADO", result.Estado);
        Assert.Equal(0, handler.Receptions);
    }

    [Theory]
    [InlineData("", "EN_PROCESO")]
    [InlineData("NO AUTORIZADO", "NO_AUTORIZADO")]
    public async Task AuthorizationAbsenceIsNotTreatedAsRejectionOrSuccess(string response, string expected)
    {
        var key = Key("07"); var handler = new FakeSri(key) { AuthorizationState = response };
        var signed = Sign($"<comprobanteRetencion id=\"comprobante\"><infoTributaria><ambiente>1</ambiente><claveAcceso>{key}</claveAcceso></infoTributaria></comprobanteRetencion>");
        var result = await Submission(handler).SubmitAsync("1", key, signed, false, default);
        Assert.Equal(expected, result.Estado); Assert.Null(result.NumeroAutorizacion);
    }

    [Fact]
    public void LiquidationGeneratorValidatesAgainstItsOwnOfficialSchema()
    {
        var (company, provider, purchase) = Fixture();
        purchase.ClaveAccesoGenerada = Key("03");
        var xml = SriLiquidacionCompraXmlBuilder.BuildUnsignedXml(purchase, company, provider);
        new SriLiquidacionCompraXmlValidator().Validate(xml);
        Assert.Equal("liquidacionCompra", XDocument.Parse(xml).Root!.Name.LocalName);
        Assert.Throws<InvalidOperationException>(() => new SriLiquidacionCompraXmlValidator().Validate(xml.Replace("<totalSinImpuestos>100.00</totalSinImpuestos>", "")));
    }

    [Fact]
    public void RetentionUsesPurchaseTotalsNotSumOfIncomeAndVatRetentionBases()
    {
        var (company, provider, purchase) = Fixture();
        var ret = new ComprobanteRetencionEntity { Id = Guid.NewGuid(), FechaEmision = Date, Establecimiento = "001", PuntoEmision = "001", Secuencial = "000000001", ClaveAcceso = Key("07") };
        ret.Detalles.Add(new() { CodigoImpuesto = "1", CodigoRetencionSRI = "312", BaseImponible = 100, PorcentajeRetencion = 1.75m, ValorRetenido = 1.75m, CodDocSustento = "01", NumDocSustento = "001-001-000000001", FechaEmisionDocSustento = Date });
        ret.Detalles.Add(new() { CodigoImpuesto = "2", CodigoRetencionSRI = "1", BaseImponible = 15, PorcentajeRetencion = 30, ValorRetenido = 4.5m, CodDocSustento = "01", NumDocSustento = "001-001-000000001", FechaEmisionDocSustento = Date });
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
            var xml = new RetencionXmlGenerator(new()).Build(ret, company, provider, purchase);
            var doc = XDocument.Parse(xml);
            Assert.Equal("100.00", doc.Descendants("totalSinImpuestos").Single().Value);
            Assert.Equal("115.00", doc.Descendants("importeTotal").Single().Value);
            Assert.Single(doc.Descendants("impuestoDocSustento"));
            Assert.Single(doc.Descendants("pago"));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    private static (EmpresaEmisoraEntity, ProveedorEntity, CompraEntity) Fixture()
    {
        var company = new EmpresaEmisoraEntity { Id = Guid.NewGuid(), Ruc = "0991111111001", RazonSocial = "Empresa de pruebas", DireccionMatriz = "Guayaquil", AmbienteSri = "1", TipoEmision = "1", ContribuyenteEspecial = "123" };
        var provider = new ProveedorEntity { Persona = new PersonaEntity { TipoIdentificacion = "04", Identificacion = "0992222222001", RazonSocialONombresCompletos = "Proveedor de pruebas", DireccionPrincipal = "Guayaquil" } };
        var purchase = new CompraEntity { EmpresaId = company.Id, TipoDocumentoCodigo = "01", SustentoTributarioSRI = "01", FormaPagoSriCodigo = "01", FechaEmision = Date, Establecimiento = "001", PuntoEmision = "001", Secuencial = "000000001", SubtotalIva15 = 100, TotalImpuestos = 15, ImporteTotal = 115 };
        purchase.Detalles.Add(new() { ProductoCodigo = "P001", ProductoNombre = "Producto de pruebas", CodigoIva = "4", PorcentajeIva = 15, Cantidad = 1, CostoUnitario = 100, CostoTotalSinImpuesto = 100 });
        return (company, provider, purchase);
    }
    private static string Sign(string xml)
    {
        using var rsa = RSA.Create(2048);
        using var certificate = new CertificateRequest("CN=Prueba", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1).CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return new SriXadesBesSigner().Sign(xml, certificate.Export(X509ContentType.Pkcs12, "test"), "test");
    }
    internal static SriDocumentSubmissionService Submission(FakeSri handler)
    {
        var options = new SriSoapOptions { Pruebas = new() { RecepcionUrl = "https://sri.test/recepcion", AutorizacionUrl = "https://sri.test/autorizacion" } };
        return new(new SriSoapClient(new HttpClient(handler), new(Options.Create(options)), new(), NullLogger<SriSoapClient>.Instance));
    }
    internal sealed class FakeSri(string key) : HttpMessageHandler
    {
        public int Receptions { get; private set; }
        public string LastReception { get; private set; } = "";
        public string AuthorizationState { get; set; } = "AUTORIZADO";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body;
            if (request.RequestUri!.AbsolutePath == "/recepcion")
            {
                Receptions++; LastReception = await request.Content!.ReadAsStringAsync(cancellationToken);
                body = "<RespuestaRecepcionComprobante><estado>RECIBIDA</estado></RespuestaRecepcionComprobante>";
            }
            else body = $"<RespuestaAutorizacionComprobante><claveAccesoConsultada>{key}</claveAccesoConsultada><autorizaciones><autorizacion><estado>{AuthorizationState}</estado><numeroAutorizacion>{key}</numeroAutorizacion><fechaAutorizacion>2026-10-28T12:00:00-05:00</fechaAutorizacion></autorizacion></autorizaciones></RespuestaAutorizacionComprobante>";
            return new(HttpStatusCode.OK) { Content = new StringContent(body) };
        }
    }
}
