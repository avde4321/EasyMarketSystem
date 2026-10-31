using System.Globalization;
using System.Xml.Linq;
using TestDeIa.Shared.Sri;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

internal static class SriFacturaXmlBuilder
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];
    private const string CodigoDocumentoFactura = "01";
    private const string CodigoNumerico = "12345678";
    private const string MonedaDolar = "DOLAR";

    public static string GenerateClaveAcceso(Factura factura)
    {
        return new ClaveAccesoService().Generar(
            factura.FechaEmision,
            CodigoDocumentoFactura,
            factura.RucEmisor,
            factura.AmbienteSri,
            factura.Establecimiento,
            factura.PuntoEmision,
            factura.Secuencial.ToString("000000000", CultureInfo.InvariantCulture),
            CodigoNumerico,
            factura.TipoEmision);
    }

    public static string BuildUnsignedXml(Factura factura, string claveAcceso)
    {
        ValidateSriConfiguration(factura);

        var ambienteCode = GetAmbienteCode(factura.AmbienteSri);
        var tipoEmisionCode = GetTipoEmisionCode(factura.TipoEmision);
        var totalesConImpuestos = SriTaxCatalog.BuildHeaderTotals(factura)
            .Select(total => new XElement("totalImpuesto",
                new XElement("codigo", total.CodigoImpuesto),
                new XElement("codigoPorcentaje", total.CodigoPorcentaje),
                new XElement("baseImponible", total.BaseImponible.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("valor", total.Valor.ToString("0.00", CultureInfo.InvariantCulture))))
            .ToArray();

        var detalles = factura.Detalles.Select(detalle =>
        {
            var tax = SriTaxCatalog.ResolveDetalleTax(detalle);

            return new XElement("detalle",
                new XElement("codigoPrincipal", detalle.CodigoProducto),
                new XElement("descripcion", detalle.NombreProducto),
                new XElement("cantidad", detalle.Cantidad.ToString("0.######", CultureInfo.InvariantCulture)),
                new XElement("precioUnitario", detalle.PrecioUnitario.ToString("0.######", CultureInfo.InvariantCulture)),
                new XElement("descuento", detalle.Descuento.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("precioTotalSinImpuesto", detalle.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("impuestos",
                    new XElement("impuesto",
                        new XElement("codigo", tax.CodigoImpuesto),
                        new XElement("codigoPorcentaje", tax.CodigoPorcentaje),
                        new XElement("tarifa", detalle.PorcentajeIva.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("baseImponible", detalle.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("valor", detalle.IvaValor.ToString("0.00", CultureInfo.InvariantCulture)))));
        });

        var pagos = new XElement("pagos",
            new XElement("pago",
                new XElement("formaPago", factura.FormaPagoSriCodigo),
                new XElement("total", factura.Total.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("plazo", "0"),
                new XElement("unidadTiempo", "dias")));

        var infoAdicional = BuildInfoAdicional(factura);

        var document = new XDocument(
            new XElement("factura",
                new XAttribute("id", "comprobante"),
                new XAttribute("version", "1.1.0"),
                new XElement("infoTributaria",
                    new XElement("ambiente", ambienteCode),
                    new XElement("tipoEmision", tipoEmisionCode),
                    new XElement("razonSocial", factura.RazonSocialEmisor),
                    new XElement("nombreComercial", factura.NombreComercialEmisor ?? factura.RazonSocialEmisor),
                    new XElement("ruc", factura.RucEmisor),
                    new XElement("claveAcceso", claveAcceso),
                    new XElement("codDoc", CodigoDocumentoFactura),
                    new XElement("estab", factura.Establecimiento),
                    new XElement("ptoEmi", factura.PuntoEmision),
                    new XElement("secuencial", factura.Secuencial.ToString("000000000", CultureInfo.InvariantCulture)),
                    new XElement("dirMatriz", factura.DireccionMatrizEmisor)),
                new XElement("infoFactura",
                    new XElement("fechaEmision", factura.FechaEmision.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                    new XElement("dirEstablecimiento", factura.DireccionEstablecimientoEmisor ?? factura.DireccionMatrizEmisor),
                    new XElement("obligadoContabilidad", factura.ObligadoContabilidad ? "SI" : "NO"),
                    new XElement("tipoIdentificacionComprador", factura.ClienteTipoIdentificacion),
                    new XElement("razonSocialComprador", factura.ClienteNombre),
                    new XElement("identificacionComprador", factura.ClienteIdentificacion),
                    BuildOptionalElement("direccionComprador", factura.ClienteDireccion),
                    new XElement("totalSinImpuestos", factura.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("totalDescuento", factura.TotalDescuento.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("totalConImpuestos", totalesConImpuestos),
                    new XElement("propina", "0.00"),
                    new XElement("importeTotal", factura.Total.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("moneda", MonedaDolar),
                    pagos,
                    BuildOptionalElement("contribuyenteEspecial", factura.ContribuyenteEspecial),
                    BuildOptionalElement("contribuyenteRimpe", factura.RegimenRimpe),
                    BuildOptionalElement("agenteRetencion", factura.AgenteRetencionResolucion)),
                new XElement("detalles", detalles),
                infoAdicional));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    public static void ValidateSriConfiguration(Factura factura)
    {
        _ = GetAmbienteCode(factura.AmbienteSri);
        _ = GetTipoEmisionCode(factura.TipoEmision);

        if (string.IsNullOrWhiteSpace(factura.RucEmisor) || factura.RucEmisor.Length != 13)
        {
            throw new InvalidOperationException("La factura no tiene un RUC emisor valido para generar el XML.");
        }

        if (string.IsNullOrWhiteSpace(factura.Establecimiento) || factura.Establecimiento.Length != 3)
        {
            throw new InvalidOperationException("La factura no tiene un establecimiento valido.");
        }

        if (string.IsNullOrWhiteSpace(factura.PuntoEmision) || factura.PuntoEmision.Length != 3)
        {
            throw new InvalidOperationException("La factura no tiene un punto de emision valido.");
        }

        if (string.IsNullOrWhiteSpace(factura.ClienteTipoIdentificacion) || factura.ClienteTipoIdentificacion.Length != 2)
        {
            throw new InvalidOperationException("La factura no tiene un tipo de identificacion del comprador valido.");
        }

        if (string.IsNullOrWhiteSpace(factura.FormaPagoSriCodigo) || factura.FormaPagoSriCodigo.Length != 2)
        {
            throw new InvalidOperationException("La factura no tiene una forma de pago SRI valida.");
        }

        if (factura.Detalles.Any(detalle => !SupportedIvaRates.Contains(detalle.PorcentajeIva)))
        {
            throw new InvalidOperationException("El comprobante contiene una tarifa de IVA no soportada por el motor SRI.");
        }

        foreach (var detalle in factura.Detalles)
        {
            _ = SriTaxCatalog.ResolveDetalleTax(detalle);
        }
    }

    public static string GetAmbienteCode(string ambienteSri)
    {
        return ClaveAccesoService.GetAmbienteCode(ambienteSri);
    }

    public static string GetTipoEmisionCode(string tipoEmision)
    {
        return ClaveAccesoService.GetTipoEmisionCode(tipoEmision);
    }

    private static object? BuildOptionalElement(string name, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new XElement(name, value);
    }

    private static XElement? BuildInfoAdicional(Factura factura)
    {
        var campos = new List<XElement>();

        if (!string.IsNullOrWhiteSpace(factura.ClienteEmail))
        {
            campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Email"), factura.ClienteEmail));
        }

        if (!string.IsNullOrWhiteSpace(factura.ClienteTelefono))
        {
            campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Telefono"), factura.ClienteTelefono));
        }

        if (!string.IsNullOrWhiteSpace(factura.ClienteDireccion))
        {
            campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Direccion"), factura.ClienteDireccion));
        }

        return campos.Count == 0 ? null : new XElement("infoAdicional", campos);
    }
}
