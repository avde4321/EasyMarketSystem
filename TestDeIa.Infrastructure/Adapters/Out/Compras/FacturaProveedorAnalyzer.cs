using System.Globalization;
using System.Xml.Linq;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class FacturaProveedorAnalyzer : IFacturaProveedorAnalyzer
{
    public async Task<FacturaProveedorAnalisisResponse> AnalyzeAsync(
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension != ".xml" && !contentType.Contains("xml", StringComparison.OrdinalIgnoreCase))
        {
            return new FacturaProveedorAnalisisResponse
            {
                Succeeded = false,
                FileName = fileName,
                Message = "Por ahora el análisis automático confiable está habilitado para XML de factura electrónica. PDF/imagen queda preparado para OCR/IA en una segunda fase.",
                Confidence = 0
            };
        }

        using var reader = new StreamReader(content, leaveOpen: false);
        var xml = await reader.ReadToEndAsync(cancellationToken);
        return AnalyzeXml(fileName, xml);
    }

    private static FacturaProveedorAnalisisResponse AnalyzeXml(string fileName, string xml)
    {
        try
        {
            var document = XDocument.Parse(xml);
            var root = document.Root;
            if (root is null)
            {
                return Failed(fileName, "El XML no contiene información legible.");
            }

            var facturaNode = ResolveFacturaNode(root);
            if (facturaNode is null)
            {
                return Failed(fileName, "El XML no corresponde a una factura electrónica SRI reconocible.");
            }

            var infoTributaria = facturaNode.Element("infoTributaria");
            var infoFactura = facturaNode.Element("infoFactura");
            if (infoTributaria is null || infoFactura is null)
            {
                return Failed(fileName, "El XML no contiene infoTributaria/infoFactura completas.");
            }

            var establecimiento = Value(infoTributaria, "estab");
            var puntoEmision = Value(infoTributaria, "ptoEmi");
            var secuencial = Value(infoTributaria, "secuencial");
            var response = new FacturaProveedorAnalisisResponse
            {
                Succeeded = true,
                FileName = fileName,
                Message = "Factura XML analizada. Revisa y aprueba antes de aplicar los datos.",
                DocumentoTipo = Value(infoTributaria, "codDoc") ?? "01",
                ProveedorRuc = Value(infoTributaria, "ruc"),
                ProveedorNombre = Value(infoTributaria, "razonSocial"),
                NumeroComprobante = BuildNumeroComprobante(establecimiento, puntoEmision, secuencial),
                ClaveAcceso = Value(infoTributaria, "claveAcceso"),
                NumeroAutorizacion = Value(infoTributaria, "claveAcceso"),
                FechaEmision = ParseSriDate(Value(infoFactura, "fechaEmision")),
                Total = ParseDecimal(Value(infoFactura, "importeTotal")),
                Confidence = 0.92m
            };

            foreach (var totalImpuesto in infoFactura.Element("totalConImpuestos")?.Elements("totalImpuesto") ?? [])
            {
                var tarifa = ParseDecimal(Value(totalImpuesto, "tarifa"));
                var baseImponible = ParseDecimal(Value(totalImpuesto, "baseImponible"));
                var valor = ParseDecimal(Value(totalImpuesto, "valor"));

                if (tarifa == 0)
                {
                    response.BaseIva0 += baseImponible;
                }
                else
                {
                    response.BaseIva15 += baseImponible;
                    response.Iva += valor;
                }
            }

            foreach (var detalle in facturaNode.Element("detalles")?.Elements("detalle") ?? [])
            {
                var tarifaIva = detalle.Element("impuestos")?
                    .Elements("impuesto")
                    .Select(current => ParseDecimal(Value(current, "tarifa")))
                    .FirstOrDefault() ?? 0m;

                response.Detalles.Add(new FacturaProveedorAnalisisDetalleResponse
                {
                    CodigoPrincipal = Value(detalle, "codigoPrincipal") ?? string.Empty,
                    Descripcion = Value(detalle, "descripcion") ?? "Item de factura proveedor",
                    Cantidad = ParseDecimal(Value(detalle, "cantidad")),
                    PrecioUnitario = ParseDecimal(Value(detalle, "precioUnitario")),
                    Descuento = ParseDecimal(Value(detalle, "descuento")),
                    TarifaIva = tarifaIva,
                    Subtotal = ParseDecimal(Value(detalle, "precioTotalSinImpuesto"))
                });
            }

            if (response.BaseIva0 == 0 && response.BaseIva15 == 0)
            {
                response.BaseIva15 = Math.Max(0, response.Total - response.Iva);
            }

            return response;
        }
        catch (Exception exception) when (exception is System.Xml.XmlException or InvalidOperationException)
        {
            return Failed(fileName, "No se pudo analizar el XML de la factura. Verifica que el archivo corresponda al comprobante autorizado del proveedor.");
        }
    }

    private static XElement? ResolveFacturaNode(XElement root)
    {
        if (root.Name.LocalName == "factura")
        {
            return root;
        }

        var comprobante = root.Descendants().FirstOrDefault(element => element.Name.LocalName == "comprobante");
        if (comprobante is null)
        {
            return null;
        }

        var innerXml = comprobante.Value;
        if (string.IsNullOrWhiteSpace(innerXml))
        {
            return null;
        }

        return XDocument.Parse(innerXml).Root;
    }

    private static FacturaProveedorAnalisisResponse Failed(string fileName, string message) =>
        new()
        {
            Succeeded = false,
            FileName = fileName,
            Message = message
        };

    private static string? Value(XElement element, string name) =>
        element.Element(name)?.Value?.Trim();

    private static DateTime? ParseSriDate(string? value)
    {
        return DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static decimal ParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
    }

    private static string? BuildNumeroComprobante(string? establecimiento, string? puntoEmision, string? secuencial)
    {
        if (string.IsNullOrWhiteSpace(establecimiento) ||
            string.IsNullOrWhiteSpace(puntoEmision) ||
            string.IsNullOrWhiteSpace(secuencial))
        {
            return null;
        }

        return $"{establecimiento}-{puntoEmision}-{secuencial}";
    }
}
