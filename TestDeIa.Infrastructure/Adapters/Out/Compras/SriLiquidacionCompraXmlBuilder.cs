using System.Globalization;
using System.Xml.Linq;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

internal static class SriLiquidacionCompraXmlBuilder
{
    private const string CodigoDocumentoLiquidacionCompra = "03";
    private const string CodigoNumerico = "12345678";
    private const string MonedaDolar = "DOLAR";

    public static string GenerateClaveAcceso(CompraEntity compra, EmpresaEmisoraEntity empresa)
    {
        var ambienteCode = SriFacturaXmlBuilder.GetAmbienteCode(empresa.AmbienteSri);
        var tipoEmisionCode = SriFacturaXmlBuilder.GetTipoEmisionCode(empresa.TipoEmision);

        var claveSinDigito =
            $"{compra.FechaEmision:ddMMyyyy}" +
            CodigoDocumentoLiquidacionCompra +
            empresa.Ruc +
            ambienteCode +
            compra.Establecimiento +
            compra.PuntoEmision +
            compra.Secuencial +
            CodigoNumerico +
            tipoEmisionCode;

        return claveSinDigito + ComputeModulo11Digit(claveSinDigito);
    }

    public static string BuildUnsignedXml(CompraEntity compra, EmpresaEmisoraEntity empresa, ProveedorEntity proveedor)
    {
        var ambienteCode = SriFacturaXmlBuilder.GetAmbienteCode(empresa.AmbienteSri);
        var tipoEmisionCode = SriFacturaXmlBuilder.GetTipoEmisionCode(empresa.TipoEmision);
        var persona = proveedor.Persona;
        var claveAcceso = compra.ClaveAccesoGenerada ?? throw new InvalidOperationException("La liquidacion no tiene clave de acceso generada.");

        var totalesConImpuestos = BuildHeaderTotals(compra)
            .Select(total => new XElement("totalImpuesto",
                new XElement("codigo", "2"),
                new XElement("codigoPorcentaje", total.CodigoPorcentaje),
                new XElement("baseImponible", total.BaseImponible.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("valor", total.Valor.ToString("0.00", CultureInfo.InvariantCulture))))
            .ToArray();

        var detalles = compra.Detalles.Select(detalle =>
        {
            var codigoPorcentaje = SriTaxCatalog.ResolveCodigoPorcentaje(detalle.CodigoIva, detalle.PorcentajeIva);
            return new XElement("detalle",
                new XElement("codigoPrincipal", detalle.ProductoCodigo),
                new XElement("descripcion", detalle.ProductoNombre),
                new XElement("cantidad", detalle.Cantidad.ToString("0.######", CultureInfo.InvariantCulture)),
                new XElement("precioUnitario", detalle.CostoUnitario.ToString("0.######", CultureInfo.InvariantCulture)),
                new XElement("descuento", detalle.Descuento.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("precioTotalSinImpuesto", detalle.CostoTotalSinImpuesto.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("impuestos",
                    new XElement("impuesto",
                        new XElement("codigo", "2"),
                        new XElement("codigoPorcentaje", codigoPorcentaje),
                        new XElement("tarifa", detalle.PorcentajeIva.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("baseImponible", detalle.CostoTotalSinImpuesto.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("valor", detalle.TotalImpuesto().ToString("0.00", CultureInfo.InvariantCulture)))));
        });

        var pagos = new XElement("pagos",
            new XElement("pago",
                new XElement("formaPago", compra.FormaPagoSriCodigo),
                new XElement("total", compra.ImporteTotal.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("plazo", "0"),
                new XElement("unidadTiempo", "dias")));

        var document = new XDocument(
            new XElement("liquidacionCompra",
                new XAttribute("id", "comprobante"),
                new XAttribute("version", "1.0.0"),
                new XElement("infoTributaria",
                    new XElement("ambiente", ambienteCode),
                    new XElement("tipoEmision", tipoEmisionCode),
                    new XElement("razonSocial", empresa.RazonSocial),
                    new XElement("nombreComercial", empresa.NombreComercial ?? empresa.RazonSocial),
                    new XElement("ruc", empresa.Ruc),
                    new XElement("claveAcceso", claveAcceso),
                    new XElement("codDoc", CodigoDocumentoLiquidacionCompra),
                    new XElement("estab", compra.Establecimiento),
                    new XElement("ptoEmi", compra.PuntoEmision),
                    new XElement("secuencial", compra.Secuencial),
                    new XElement("dirMatriz", empresa.DireccionMatriz)),
                new XElement("infoLiquidacionCompra",
                    new XElement("fechaEmision", compra.FechaEmision.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                    new XElement("dirEstablecimiento", ResolveDireccionEstablecimiento(compra, empresa)),
                    new XElement("obligadoContabilidad", empresa.ObligadoContabilidad ? "SI" : "NO"),
                    new XElement("tipoIdentificacionProveedor", MapProveedorTipoIdentificacion(persona.TipoIdentificacion)),
                    new XElement("razonSocialProveedor", persona.RazonSocialONombresCompletos),
                    new XElement("identificacionProveedor", persona.Identificacion),
                    BuildOptionalElement("direccionProveedor", persona.DireccionPrincipal),
                    new XElement("totalSinImpuestos", TotalSinImpuestos(compra).ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("totalDescuento", compra.TotalDescuento.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("totalConImpuestos", totalesConImpuestos),
                    new XElement("importeTotal", compra.ImporteTotal.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("moneda", MonedaDolar),
                    pagos,
                    BuildOptionalElement("contribuyenteEspecial", empresa.ContribuyenteEspecial),
                    BuildOptionalElement("regimenRimpe", empresa.RegimenRimpe)),
                new XElement("detalles", detalles),
                BuildInfoAdicional(compra, persona)));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static decimal TotalSinImpuestos(CompraEntity compra)
        => compra.SubtotalIva0 + compra.SubtotalIva5 + compra.SubtotalIva8 + compra.SubtotalIva15;

    private static string ResolveDireccionEstablecimiento(CompraEntity compra, EmpresaEmisoraEntity empresa)
    {
        var direccionPunto = empresa.PuntosEmision
            .FirstOrDefault(point =>
                point.Establecimiento == compra.Establecimiento &&
                point.PuntoEmision == compra.PuntoEmision)?
            .DireccionEstablecimiento;

        return string.IsNullOrWhiteSpace(direccionPunto)
            ? empresa.DireccionEstablecimiento ?? empresa.DireccionMatriz
            : direccionPunto;
    }

    private static string MapProveedorTipoIdentificacion(string? tipoIdentificacion)
    {
        return SriCatalogCodes.NormalizeTipoIdentificacionCode(tipoIdentificacion)
            ?? throw new InvalidOperationException("El proveedor no tiene un tipo de identificacion compatible con el SRI.");
    }

    private static IEnumerable<(string CodigoPorcentaje, decimal BaseImponible, decimal Valor)> BuildHeaderTotals(CompraEntity compra)
    {
        return compra.Detalles
            .GroupBy(detail => detail.PorcentajeIva)
            .Where(group => group.Sum(item => item.CostoTotalSinImpuesto) > 0)
            .OrderBy(group => group.Key)
            .Select(group => (
                SriTaxCatalog.ResolveCodigoPorcentaje(group.First().CodigoIva, group.Key),
                Math.Round(group.Sum(item => item.CostoTotalSinImpuesto), 2, MidpointRounding.AwayFromZero),
                Math.Round(group.Sum(item => item.TotalImpuesto()), 2, MidpointRounding.AwayFromZero)));
    }

    private static XElement? BuildInfoAdicional(CompraEntity compra, PersonaEntity proveedor)
    {
        var campos = new List<XElement>();
        if (!string.IsNullOrWhiteSpace(proveedor.CorreoElectronicoPrincipal))
        {
            campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Email"), proveedor.CorreoElectronicoPrincipal));
        }

        if (!string.IsNullOrWhiteSpace(proveedor.TelefonoCelular))
        {
            campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Telefono"), proveedor.TelefonoCelular));
        }

        if (!string.IsNullOrWhiteSpace(compra.Observacion))
        {
            campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Observacion"), compra.Observacion));
        }

        return campos.Count == 0 ? null : new XElement("infoAdicional", campos);
    }

    private static XElement? BuildOptionalElement(string name, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new XElement(name, value);
    }

    private static int ComputeModulo11Digit(string key)
    {
        var factor = 2;
        var total = 0;

        for (var index = key.Length - 1; index >= 0; index--)
        {
            total += (key[index] - '0') * factor;
            factor++;
            if (factor > 7)
            {
                factor = 2;
            }
        }

        var modulo = 11 - (total % 11);
        return modulo switch
        {
            11 => 0,
            10 => 1,
            _ => modulo
        };
    }
}

internal static class CompraDetalleEntityExtensions
{
    internal static decimal TotalImpuesto(this CompraDetalleEntity detalle)
    {
        return Math.Round(detalle.CostoTotalSinImpuesto * (detalle.PorcentajeIva / 100m), 2, MidpointRounding.AwayFromZero);
    }
}
