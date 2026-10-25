using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;

namespace TestDeIa.Tests.Sri;

public sealed class XadesSigningTests
{
    [Fact]
    public void SignResolvesSignedPropertiesInsideDataObject()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=Prueba local", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var p12 = certificate.Export(X509ContentType.Pkcs12, "test-password");
        var signed = new SriXadesBesSigner().Sign("<factura id=\"comprobante\" version=\"1.1.0\"><info>Prueba</info></factura>", p12, "test-password");
        var xml = new XmlDocument();
        xml.LoadXml(signed);
        Assert.Single(xml.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").OfType<XmlElement>());
        Assert.Single(xml.GetElementsByTagName("SignedProperties", "http://uri.etsi.org/01903/v1.3.2#").OfType<XmlElement>());
    }
}
