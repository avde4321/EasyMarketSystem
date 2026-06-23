using System.Globalization;
using System.Xml.Linq;
using TestDeIa.Application.Modules.Facturacion.Models;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class SimulatedSriFacturaProcessor : ISriFacturaProcessor
{
    private static readonly HashSet<decimal> SupportedIvaRates = [0m, 5m, 8m, 15m];
    private const string CodigoDocumentoFactura = "01";
    private const string CodigoNumerico = "12345678";
    private const string CodigoImpuestoIva = "2";
    private const string MonedaDolar = "DOLAR";

    public async Task<SriFacturaProcessingResult> ProcessAsync(Factura factura, CancellationToken cancellationToken = default)
    {
        await Task.Delay(900, cancellationToken);

        ValidateSriConfiguration(factura);
        var ambienteCode = GetAmbienteCode(factura.AmbienteSri);
        var tipoEmisionCode = GetTipoEmisionCode(factura.TipoEmision);
        var claveAcceso = GenerateClaveAcceso(factura, ambienteCode, tipoEmisionCode);

        if (factura.Detalles.Any(detalle => !SupportedIvaRates.Contains(detalle.PorcentajeIva)))
        {
            return new SriFacturaProcessingResult
            {
                EstadoFinal = "Rechazado",
                ClaveAcceso = claveAcceso,
                Mensaje = "El comprobante contiene una tarifa de IVA no soportada por el motor SRI.",
                XmlFirmado = BuildFacturaXml(factura),
                FechaRespuesta = DateTimeOffset.UtcNow
            };
        }

        return new SriFacturaProcessingResult
        {
            EstadoFinal = "Autorizado",
            ClaveAcceso = claveAcceso,
            NumeroAutorizacion = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}{factura.Secuencial:000000000}",
            Mensaje = "Comprobante autorizado por el flujo asincrono.",
            XmlFirmado = BuildFacturaXml(factura),
            FechaRespuesta = DateTimeOffset.UtcNow
        };
    }

    private static string BuildFacturaXml(Factura factura)
    {
        var ambienteCode = GetAmbienteCode(factura.AmbienteSri);
        var tipoEmisionCode = GetTipoEmisionCode(factura.TipoEmision);
        var claveAcceso = GenerateClaveAcceso(factura, ambienteCode, tipoEmisionCode);

        var detalles = factura.Detalles.Select(detalle =>
            new XElement("detalle",
                new XElement("codigoPrincipal", detalle.CodigoProducto),
                new XElement("descripcion", detalle.NombreProducto),
                new XElement("cantidad", detalle.Cantidad.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement("precioUnitario", detalle.PrecioUnitario.ToString("0.00####", CultureInfo.InvariantCulture)),
                new XElement("descuento", "0.00"),
                new XElement("precioTotalSinImpuesto", detalle.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("impuestos",
                    new XElement("impuesto",
                        new XElement("codigo", CodigoImpuestoIva),
                        new XElement("codigoPorcentaje", detalle.CodigoIva),
                        new XElement("tarifa", detalle.PorcentajeIva.ToString("0.##", CultureInfo.InvariantCulture)),
                        new XElement("baseImponible", detalle.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("valor", detalle.IvaValor.ToString("0.00", CultureInfo.InvariantCulture))))));

        var totalesConImpuestos = factura.Detalles
            .GroupBy(detalle => new { detalle.CodigoIva, detalle.PorcentajeIva })
            .Select(group => new XElement("totalImpuesto",
                new XElement("codigo", CodigoImpuestoIva),
                new XElement("codigoPorcentaje", group.Key.CodigoIva),
                new XElement("baseImponible", group.Sum(item => item.Subtotal).ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("valor", group.Sum(item => item.IvaValor).ToString("0.00", CultureInfo.InvariantCulture))))
            .ToArray();

        var pagos = new XElement("pagos",
            new XElement("pago",
                new XElement("formaPago", factura.FormaPagoSriCodigo),
                new XElement("total", factura.Total.ToString("0.00", CultureInfo.InvariantCulture))));

        if (RequiresPaymentTerm(factura.FormaPagoSriCodigo))
        {
            pagos.Element("pago")!.Add(
                new XElement("plazo", "30"),
                new XElement("unidadTiempo", "dias"));
        }

        var infoAdicional = BuildInfoAdicional(factura);

        var document = new XDocument(
            new XElement("factura",
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
                    new XElement("totalSinImpuestos", factura.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                    BuildOptionalElement("direccionComprador", factura.ClienteDireccion),
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

    private static string GenerateClaveAcceso(Factura factura, string ambienteCode, string tipoEmisionCode)
    {
        var claveSinDigito =
            $"{factura.FechaEmision:ddMMyyyy}" +
            $"{CodigoDocumentoFactura}" +
            $"{factura.RucEmisor}" +
            $"{ambienteCode}" +
            $"{factura.Establecimiento}" +
            $"{factura.PuntoEmision}" +
            $"{factura.Secuencial:000000000}" +
            $"{CodigoNumerico}" +
            $"{tipoEmisionCode}";

        return claveSinDigito + ComputeModulo11Digit(claveSinDigito);
    }

    private static object? BuildOptionalElement(string name, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new XElement(name, value);
    }

    private static string GetAmbienteCode(string ambienteSri)
    {
        if (string.Equals(ambienteSri, "Pruebas", StringComparison.OrdinalIgnoreCase))
        {
            return "1";
        }

        if (string.Equals(ambienteSri, "Produccion", StringComparison.OrdinalIgnoreCase))
        {
            return "2";
        }

        throw new InvalidOperationException("El ambiente SRI de la factura no es valido.");
    }

    private static string GetTipoEmisionCode(string tipoEmision)
    {
        if (string.Equals(tipoEmision, "Normal", StringComparison.OrdinalIgnoreCase))
        {
            return "1";
        }

        throw new InvalidOperationException("El tipo de emision de la factura no es valido.");
    }

    private static void ValidateSriConfiguration(Factura factura)
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

    private static bool RequiresPaymentTerm(string formaPagoSriCodigo)
    {
        return formaPagoSriCodigo is not "01";
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
