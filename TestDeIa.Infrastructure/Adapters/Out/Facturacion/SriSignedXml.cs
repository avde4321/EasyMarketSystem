using System.Security.Cryptography.Xml;
using System.Xml;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

internal sealed class SriSignedXml : SignedXml
{
    public SriSignedXml(XmlDocument document)
        : base(document)
    {
    }

    public override XmlElement? GetIdElement(XmlDocument? document, string idValue)
    {
        var defaultElement = base.GetIdElement(document, idValue);
        if (defaultElement is not null || document?.DocumentElement is null)
        {
            return defaultElement;
        }

        return document.SelectSingleNode($"//*[@id='{idValue}' or @Id='{idValue}']") as XmlElement;
    }
}
