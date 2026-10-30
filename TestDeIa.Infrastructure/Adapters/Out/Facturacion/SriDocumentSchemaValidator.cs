using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SriDocumentSchemaValidator
{
    public void Validate(string xml, string rootName, params string[] versions)
    {
        using var safeReader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var doc = XDocument.Load(safeReader);
        var version = (string?)doc.Root?.Attribute("version") ?? "";
        if (doc.Root?.Name != XName.Get(rootName) || !versions.Contains(version))
            throw new InvalidOperationException($"Raíz o versión no soportada para {rootName}.");
        var name = rootName switch
        {
            "comprobanteRetencion" => "SriRetencionV200.xsd",
            "liquidacionCompra" => version == "1.0.0" ? "SriLiquidacionV100.xsd" : "SriLiquidacionV110.xsd",
            _ => throw new InvalidOperationException("No existe un esquema específico para este documento.")
        };
        var schemas = new XmlSchemaSet { XmlResolver = null };
        using var ds = Open("xmldsig-core-schema.xsd");
        using var dsReader = XmlReader.Create(ds, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null });
        schemas.Add("http://www.w3.org/2000/09/xmldsig#", dsReader);
        using var schema = Open(name);
        using var reader = XmlReader.Create(schema, new XmlReaderSettings { XmlResolver = null });
        schemas.Add(null, reader);
        var errors = new List<string>();
        doc.Validate(schemas, (_, args) => errors.Add(args.Message));
        if (errors.Count != 0) throw new InvalidOperationException($"XML {rootName} v{version} inválido: {string.Join(" | ", errors)}");
    }

    private static Stream Open(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream("TestDeIa.Infrastructure." + name)
        ?? throw new InvalidOperationException("No se encontró el esquema " + name);
}
