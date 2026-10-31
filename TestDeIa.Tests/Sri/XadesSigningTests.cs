using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Numerics;
using System.Globalization;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;

namespace TestDeIa.Tests.Sri;

public sealed class XadesSigningTests
{
    [Theory]
    [InlineData("00C2035650", "3254998608")]
    [InlineData("0100", "256")]
    [InlineData("7FFFFFFFFFFFFFFF", "9223372036854775807")]
    public void CertificateSerialMatchesKeyInfoAndSignedProperties(string serialHex, string expectedDecimal)
    {
        using var issuerKey = RSA.Create(2048);
        var issuerRequest = new CertificateRequest("CN=Emisor de prueba,O=EasyMarket,C=EC", issuerKey,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        issuerRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        using var issuer = issuerRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(2));
        using var leafKey = RSA.Create(2048);
        var leafRequest = new CertificateRequest("CN=Firmante", leafKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var publicLeaf = leafRequest.Create(issuer, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), Convert.FromHexString(serialHex));
        using var leaf = publicLeaf.CopyWithPrivateKey(leafKey);
        var signed = new SriXadesBesSigner().Sign("<factura id=\"comprobante\"><info>Prueba</info></factura>",
            leaf.Export(X509ContentType.Pkcs12, "test-password"), "test-password");
        var xml = new XmlDocument { PreserveWhitespace = true };
        xml.LoadXml(signed);
        var ns = new XmlNamespaceManager(xml.NameTable);
        ns.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
        ns.AddNamespace("etsi", "http://uri.etsi.org/01903/v1.3.2#");
        var keySerial = xml.SelectSingleNode("//ds:KeyInfo/ds:X509Data/ds:X509IssuerSerial/ds:X509SerialNumber", ns)!;
        var signedSerial = xml.SelectSingleNode("//etsi:SigningCertificate/etsi:Cert/etsi:IssuerSerial/ds:X509SerialNumber", ns)!;
        Assert.Equal(expectedDecimal, new BigInteger(leaf.GetSerialNumber(), isUnsigned: true, isBigEndian: false).ToString(CultureInfo.InvariantCulture));
        Assert.Equal(expectedDecimal, keySerial.InnerText);
        Assert.Equal(expectedDecimal, signedSerial.InnerText);
    }

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
        var ns = new XmlNamespaceManager(xml.NameTable);
        ns.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
        var keyInfo = (XmlElement)xml.SelectSingleNode("//ds:KeyInfo", ns)!;
        Assert.False(string.IsNullOrEmpty(keyInfo.GetAttribute("Id")));
        Assert.NotNull(xml.SelectSingleNode($"//ds:SignedInfo/ds:Reference[@URI='#{keyInfo.GetAttribute("Id")}']", ns));
        Assert.Equal(3, xml.SelectNodes("//ds:SignedInfo/ds:Reference", ns)!.Count);
        var embedded = xml.SelectSingleNode("//ds:KeyInfo/ds:X509Data/ds:X509Certificate", ns)!;
        using var recovered = X509CertificateLoader.LoadCertificate(Convert.FromBase64String(embedded.InnerText));
        Assert.Equal(certificate.RawData, recovered.RawData);
        var verifier = new SriSignedXml(xml);
        verifier.LoadXml((XmlElement)xml.SelectSingleNode("//ds:Signature", ns)!);
        Assert.True(verifier.CheckSignature(recovered, true));
        keyInfo.SetAttribute("Id", "certificado-sustituido");
        var tamperedVerifier = new SriSignedXml(xml);
        tamperedVerifier.LoadXml((XmlElement)xml.SelectSingleNode("//ds:Signature", ns)!);
        var accepted = false;
        try { accepted = tamperedVerifier.CheckSignature(recovered, true); }
        catch (CryptographicException) { /* Referencia inexistente: rechazo esperado. */ }
        Assert.False(accepted);
    }
}
