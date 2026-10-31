using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriFacturaXmlSchemaValidator
{
    private const string FacturaSchemaResourceName = "TestDeIa.Infrastructure.SriFacturaV110.xsd";

    public void Validate(string xmlContent)
    {
        var schemaSet = new XmlSchemaSet();
        using var schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(FacturaSchemaResourceName)
            ?? throw new InvalidOperationException($"No se encontro el recurso XSD '{FacturaSchemaResourceName}'.");
        using var schemaReader = XmlReader.Create(schemaStream);
        schemaSet.Add(string.Empty, schemaReader);

        var errors = new List<string>();
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemaSet,
            DtdProcessing = DtdProcessing.Prohibit
        };

        settings.ValidationEventHandler += (_, args) =>
        {
            errors.Add(args.Message);
        };

        using var xmlReader = XmlReader.Create(new StringReader(xmlContent), settings);
        while (xmlReader.Read())
        {
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"El XML de factura no cumple el esquema XSD configurado: {string.Join(" | ", errors)}");
        }
    }
}
