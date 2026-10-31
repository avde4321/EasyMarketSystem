using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Data.SqlClient;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using Xunit.Abstractions;

namespace TestDeIa.Tests.Sri;

public sealed class StoredSriCertificateFactAttribute : FactAttribute
{
    public StoredSriCertificateFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("EASYMARKET_SRI_DIAGNOSTIC_FACTURA_ID")))
            Skip = "Diagnóstico local opt-in: configurar EASYMARKET_SRI_DIAGNOSTIC_FACTURA_ID y EASYMARKET_TEST_SQL.";
    }
}

/// <summary>Solo lectura de BD y firma en memoria; nunca envía al SRI ni persiste XML/contraseñas.</summary>
public sealed class SriStoredCertificateDiagnosticTests(ITestOutputHelper output)
{
    [StoredSriCertificateFact]
    public async Task ActualCompanyP12ProducesConsistentSignerIdentity()
    {
        var facturaId = Guid.Parse(Environment.GetEnvironmentVariable("EASYMARKET_SRI_DIAGNOSTIC_FACTURA_ID")!);
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("EASYMARKET_TEST_SQL"));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP(1) f.EmpresaId, f.XmlGenerado, c.Id, c.Contenido, c.Clave
            FROM Facturas f JOIN CertificadosDigitalesEmpresa c ON c.EmpresaId=f.EmpresaId
            WHERE f.Id=@id AND c.IsActive=1
                AND c.FechaInicioVigencia<=SYSUTCDATETIME() AND c.FechaFinVigencia>=SYSUTCDATETIME()
            ORDER BY c.EsPrincipal DESC, c.FechaFinVigencia DESC
            """;
        command.Parameters.AddWithValue("@id", facturaId);
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync(), "No se encontró factura con certificado vigente de su misma empresa.");
        var companyId = reader.GetGuid(0);
        var xmlBase = reader.GetString(1);
        var certificateId = reader.GetGuid(2);
        var p12 = (byte[])reader.GetValue(3);
        var password = reader.GetString(4);
        try
        {
            using var selected = X509CertificateLoader.LoadPkcs12(p12, password, X509KeyStorageFlags.EphemeralKeySet);
            SriCertificateSelector.Validate(selected);
            var signed = new SriXadesBesSigner().Sign(xmlBase, p12, password);
            var xml = new XmlDocument { PreserveWhitespace = true };
            xml.LoadXml(signed);
            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            var embeddedNode = xml.SelectSingleNode("//ds:KeyInfo/ds:X509Data/ds:X509Certificate", ns)!;
            using var embedded = X509CertificateLoader.LoadCertificate(Convert.FromBase64String(embeddedNode.InnerText));
            Assert.Equal(selected.RawData, embedded.RawData);
            var serial = new BigInteger(selected.GetSerialNumber(), true, false).ToString(CultureInfo.InvariantCulture);
            foreach (XmlNode node in xml.SelectNodes("//ds:X509SerialNumber", ns)!)
                Assert.Equal(serial, node.InnerText);
            var verifier = new SriSignedXml(xml);
            verifier.LoadXml((XmlElement)xml.SelectSingleNode("//ds:Signature", ns)!);
            Assert.True(verifier.CheckSignature(embedded, true));
            output.WriteLine($"Empresa={companyId}; CertificadoRegistro={certificateId}; HuellaSHA1={embedded.Thumbprint}; SerieDecimal={serial}; IdentidadYFirma=OK. Sin envío SRI.");
        }
        finally { CryptographicOperations.ZeroMemory(p12); }
    }
}
