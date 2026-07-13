using System.Xml.Linq;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class SriLiquidacionCompraXmlValidator
{
    public void Validate(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new InvalidOperationException("La liquidacion no contiene un XML generado.");
        }

        var document = XDocument.Parse(xml, LoadOptions.None);
        var root = document.Root ?? throw new InvalidOperationException("El XML de la liquidacion no contiene nodo raiz.");
        if (!string.Equals(root.Name.LocalName, "liquidacionCompra", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("El XML de la liquidacion no tiene la estructura raiz esperada.");
        }

        EnsureElement(root, "infoTributaria");
        EnsureElement(root, "infoLiquidacionCompra");
        EnsureElement(root, "detalles");
    }

    private static void EnsureElement(XElement root, string name)
    {
        if (root.Element(name) is null)
        {
            throw new InvalidOperationException($"El XML de la liquidacion no contiene el nodo obligatorio '{name}'.");
        }
    }
}
