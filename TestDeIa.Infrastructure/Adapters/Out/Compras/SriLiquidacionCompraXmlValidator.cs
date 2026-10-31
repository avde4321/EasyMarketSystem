using TestDeIa.Infrastructure.Adapters.Out.Facturacion;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class SriLiquidacionCompraXmlValidator
{
    public void Validate(string xml) => new SriDocumentSchemaValidator().Validate(xml, "liquidacionCompra", "1.0.0", "1.1.0");
}
