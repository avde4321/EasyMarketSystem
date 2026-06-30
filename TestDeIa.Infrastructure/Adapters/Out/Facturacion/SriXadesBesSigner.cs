using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriXadesBesSigner
{
    private const string XadesNamespace = "http://uri.etsi.org/01903/v1.3.2#";

    public string Sign(string xmlGenerado, byte[] certificadoContenido, string certificadoClave)
    {
        var xmlDocument = new XmlDocument { PreserveWhitespace = true };
        xmlDocument.LoadXml(xmlGenerado);

        using var certificate = X509CertificateLoader.LoadPkcs12(
            certificadoContenido,
            certificadoClave,
            X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

        using var privateKey = certificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("El certificado .p12 no contiene una clave privada RSA valida.");

        var signatureId = $"Signature-{Guid.NewGuid():N}";
        var signedPropertiesId = $"{signatureId}-SignedProperties";
        const string documentReferenceId = "Reference-comprobante";

        var signedXml = new SriSignedXml(xmlDocument)
        {
            SigningKey = privateKey
        };

        var signedInfo = signedXml.SignedInfo
            ?? throw new InvalidOperationException("No fue posible inicializar la seccion SignedInfo para la firma XAdES-BES.");
        signedInfo.CanonicalizationMethod = SignedXml.XmlDsigCanonicalizationUrl;
        signedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

        var documentReference = new Reference
        {
            Uri = "#comprobante",
            DigestMethod = SignedXml.XmlDsigSHA1Url,
            Id = documentReferenceId
        };
        documentReference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        documentReference.AddTransform(new XmlDsigC14NTransform());
        signedXml.AddReference(documentReference);

        var qualifyingPropertiesDocument = BuildQualifyingPropertiesDocument(certificate, signatureId, signedPropertiesId, documentReferenceId);
        signedXml.AddObject(new DataObject
        {
            Data = qualifyingPropertiesDocument.ChildNodes
        });

        var signedPropertiesReference = new Reference
        {
            Uri = $"#{signedPropertiesId}",
            Type = "http://uri.etsi.org/01903#SignedProperties",
            DigestMethod = SignedXml.XmlDsigSHA1Url
        };
        signedPropertiesReference.AddTransform(new XmlDsigC14NTransform());
        signedXml.AddReference(signedPropertiesReference);

        var keyInfo = new KeyInfo();
        var x509Data = new KeyInfoX509Data(certificate);
        x509Data.AddIssuerSerial(certificate.Issuer, GetDecimalSerialNumber(certificate));
        keyInfo.AddClause(new RSAKeyValue(privateKey));
        keyInfo.AddClause(x509Data);
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();

        var signatureElement = signedXml.GetXml();
        signatureElement.SetAttribute("Id", signatureId);
        var rootElement = xmlDocument.DocumentElement
            ?? throw new InvalidOperationException("El XML generado no tiene un nodo raiz valido para adjuntar la firma.");
        rootElement.AppendChild(xmlDocument.ImportNode(signatureElement, true));

        ValidateGeneratedSignature(xmlDocument, certificate);
        return xmlDocument.OuterXml;
    }

    private static XmlDocument BuildQualifyingPropertiesDocument(
        X509Certificate2 certificate,
        string signatureId,
        string signedPropertiesId,
        string documentReferenceId)
    {
        var digestValue = Convert.ToBase64String(SHA1.HashData(certificate.RawData));
        var signingTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
        var serialNumber = GetDecimalSerialNumber(certificate);

        XNamespace ds = SignedXml.XmlDsigNamespaceUrl;
        XNamespace xades = XadesNamespace;

        var document = new XDocument(
            new XElement(xades + "QualifyingProperties",
                new XAttribute(XNamespace.Xmlns + "etsi", xades),
                new XAttribute(XNamespace.Xmlns + "ds", ds),
                new XAttribute("Target", $"#{signatureId}"),
                new XElement(xades + "SignedProperties",
                    new XAttribute("Id", signedPropertiesId),
                    new XElement(xades + "SignedSignatureProperties",
                        new XElement(xades + "SigningTime", signingTime),
                        new XElement(xades + "SigningCertificate",
                            new XElement(xades + "Cert",
                                new XElement(xades + "CertDigest",
                                    new XElement(ds + "DigestMethod", new XAttribute("Algorithm", SignedXml.XmlDsigSHA1Url)),
                                    new XElement(ds + "DigestValue", digestValue)),
                                new XElement(xades + "IssuerSerial",
                                    new XElement(ds + "X509IssuerName", certificate.Issuer),
                                    new XElement(ds + "X509SerialNumber", serialNumber))))),
                    new XElement(xades + "SignedDataObjectProperties",
                        new XElement(xades + "DataObjectFormat",
                            new XAttribute("ObjectReference", $"#{documentReferenceId}"),
                            new XElement(xades + "Description", "Comprobante electronico"),
                            new XElement(xades + "MimeType", "text/xml"),
                            new XElement(xades + "Encoding", "UTF-8"))))));

        var xmlDocument = new XmlDocument { PreserveWhitespace = true };
        xmlDocument.LoadXml(document.ToString(SaveOptions.DisableFormatting));
        return xmlDocument;
    }

    private static string GetDecimalSerialNumber(X509Certificate2 certificate)
    {
        var serialLittleEndian = certificate.GetSerialNumber();
        var decimalValue = new BigInteger(serialLittleEndian.Concat(new byte[] { 0 }).ToArray());
        return decimalValue.ToString(CultureInfo.InvariantCulture);
    }

    private static void ValidateGeneratedSignature(XmlDocument xmlDocument, X509Certificate2 certificate)
    {
        var signatureElement = xmlDocument.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)
            .OfType<XmlElement>()
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No fue posible recuperar la firma XML generada.");

        var validator = new SriSignedXml(xmlDocument);
        validator.LoadXml(signatureElement);

        if (!validator.CheckSignature(certificate, true))
        {
            throw new InvalidOperationException("La firma XAdES-BES generada no paso la validacion criptografica local.");
        }
    }
}
