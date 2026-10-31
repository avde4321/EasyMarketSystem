using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
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
        if (defaultElement is not null)
        {
            return defaultElement;
        }

        // SignedProperties está dentro de un ds:Object aún no adjunto al XML durante ComputeSignature.
        var matches = new List<XmlElement>();
        if (document is not null)
            matches.AddRange(document.GetElementsByTagName("*").OfType<XmlElement>()
                .Where(element => element.GetAttribute("id") == idValue || element.GetAttribute("Id") == idValue));
        if (matches.Count == 0)
        {
            // KeyInfo aún está separado del documento al calcular su referencia.
            var keyInfo = KeyInfo?.GetXml();
            if (keyInfo?.GetAttribute("Id") == idValue) matches.Add(keyInfo);
            foreach (DataObject dataObject in Signature.ObjectList)
            {
                matches.AddRange(dataObject.GetXml().GetElementsByTagName("*").OfType<XmlElement>()
                    .Where(element => element.GetAttribute("Id") == idValue));
            }
        }
        if (matches.Count > 1)
            throw new CryptographicException("La referencia de firma XML contiene identificadores duplicados.");
        return matches.SingleOrDefault();
    }
}
