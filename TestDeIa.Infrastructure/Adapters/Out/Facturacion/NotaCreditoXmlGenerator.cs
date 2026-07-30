using System.Globalization;
using System.Xml.Linq;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class NotaCreditoXmlGenerator
{
    private const string CodigoDocumentoNotaCredito = "04";
    private const string CodigoDocumentoFactura = "01";
    private const string CodigoNumerico = "12345678";
    private const string MonedaDolar = "DOLAR";

    private readonly ClaveAccesoService claveAccesoService;
    private readonly SriNotaCreditoXmlSchemaValidator schemaValidator;

    public NotaCreditoXmlGenerator(
        ClaveAccesoService claveAccesoService,
        SriNotaCreditoXmlSchemaValidator schemaValidator)
    {
        this.claveAccesoService = claveAccesoService;
        this.schemaValidator = schemaValidator;
    }

    public string GenerarClaveAcceso(ComprobanteCabeceraEntity notaCredito)
    {
        return claveAccesoService.Generar(
            notaCredito.FechaEmision,
            CodigoDocumentoNotaCredito,
            notaCredito.RucEmisor,
            notaCredito.AmbienteSri,
            notaCredito.Establecimiento,
            notaCredito.PuntoEmision,
            notaCredito.Secuencial.ToString("000000000", CultureInfo.InvariantCulture),
            CodigoNumerico,
            notaCredito.TipoEmision);
    }

    public string BuildUnsignedXml(ComprobanteCabeceraEntity notaCredito)
    {
        ValidateNotaCredito(notaCredito);

        var ambienteCode = ClaveAccesoService.GetAmbienteCode(notaCredito.AmbienteSri);
        var tipoEmisionCode = ClaveAccesoService.GetTipoEmisionCode(notaCredito.TipoEmision);
        var codDocModificado = string.IsNullOrWhiteSpace(notaCredito.CodDocModificado)
            ? CodigoDocumentoFactura
            : notaCredito.CodDocModificado;
        var numDocModificado = notaCredito.NumDocModificado
            ?? throw new InvalidOperationException("La nota de credito requiere el numero del documento modificado.");
        var fechaSustento = notaCredito.FechaEmisionDocSustento
            ?? throw new InvalidOperationException("La nota de credito requiere fecha de emision del documento sustento.");
        var motivo = string.IsNullOrWhiteSpace(notaCredito.MotivoModificacion)
            ? throw new InvalidOperationException("La nota de credito requiere motivo de modificacion.")
            : notaCredito.MotivoModificacion.Trim();

        var totalConImpuestos = notaCredito.Detalles
            .GroupBy(detalle => detalle.PorcentajeIva)
            .Select(group =>
            {
                var porcentaje = group.Key;
                var codigoPorcentaje = ResolveCodigoPorcentajeIva(group.First().CodigoIva, porcentaje);
                return new XElement("totalImpuesto",
                    new XElement("codigo", "2"),
                    new XElement("codigoPorcentaje", codigoPorcentaje),
                    new XElement("baseImponible", RoundMoney(group.Sum(detalle => detalle.Subtotal)).ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("valor", RoundMoney(group.Sum(detalle => detalle.IvaValor)).ToString("0.00", CultureInfo.InvariantCulture)));
            })
            .ToArray();

        var detalles = notaCredito.Detalles.Select(detalle =>
            new XElement("detalle",
                new XElement("codigoInterno", detalle.CodigoProducto),
                new XElement("descripcion", detalle.NombreProducto),
                new XElement("cantidad", detalle.Cantidad.ToString("0.######", CultureInfo.InvariantCulture)),
                new XElement("precioUnitario", detalle.PrecioUnitario.ToString("0.######", CultureInfo.InvariantCulture)),
                new XElement("descuento", detalle.Descuento.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("precioTotalSinImpuesto", detalle.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("impuestos",
                    new XElement("impuesto",
                        new XElement("codigo", "2"),
                        new XElement("codigoPorcentaje", ResolveCodigoPorcentajeIva(detalle.CodigoIva, detalle.PorcentajeIva)),
                        new XElement("tarifa", detalle.PorcentajeIva.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("baseImponible", detalle.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("valor", detalle.IvaValor.ToString("0.00", CultureInfo.InvariantCulture))))));

        var document = new XDocument(
            new XElement("notaCredito",
                new XAttribute("id", "comprobante"),
                new XAttribute("version", "1.1.0"),
                new XElement("infoTributaria",
                    new XElement("ambiente", ambienteCode),
                    new XElement("tipoEmision", tipoEmisionCode),
                    new XElement("razonSocial", notaCredito.RazonSocialEmisor),
                    BuildOptionalElement("nombreComercial", notaCredito.NombreComercialEmisor ?? notaCredito.RazonSocialEmisor),
                    new XElement("ruc", notaCredito.RucEmisor),
                    new XElement("claveAcceso", notaCredito.ClaveAcceso),
                    new XElement("codDoc", CodigoDocumentoNotaCredito),
                    new XElement("estab", notaCredito.Establecimiento),
                    new XElement("ptoEmi", notaCredito.PuntoEmision),
                    new XElement("secuencial", notaCredito.Secuencial.ToString("000000000", CultureInfo.InvariantCulture)),
                    new XElement("dirMatriz", notaCredito.DireccionMatrizEmisor)),
                new XElement("infoNotaCredito",
                    new XElement("fechaEmision", notaCredito.FechaEmision.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                    BuildOptionalElement("dirEstablecimiento", notaCredito.DireccionEstablecimientoEmisor ?? notaCredito.DireccionMatrizEmisor),
                    new XElement("tipoIdentificacionComprador", notaCredito.ClienteTipoIdentificacion),
                    new XElement("razonSocialComprador", notaCredito.ClienteNombre),
                    new XElement("identificacionComprador", notaCredito.ClienteIdentificacion),
                    new XElement("obligadoContabilidad", notaCredito.ObligadoContabilidad ? "SI" : "NO"),
                    new XElement("codDocModificado", codDocModificado),
                    new XElement("numDocModificado", numDocModificado),
                    new XElement("fechaEmisionDocSustento", fechaSustento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                    new XElement("totalSinImpuestos", notaCredito.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("valorModificacion", notaCredito.Total.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement("moneda", MonedaDolar),
                    new XElement("totalConImpuestos", totalConImpuestos),
                    new XElement("motivo", motivo)),
                new XElement("detalles", detalles),
                BuildInfoAdicional(notaCredito)));

        var xml = document.ToString(SaveOptions.DisableFormatting);
        schemaValidator.Validate(xml);
        return xml;
    }

    private static void ValidateNotaCredito(ComprobanteCabeceraEntity notaCredito)
    {
        if (!string.Equals(notaCredito.TipoDocumentoId, CodigoDocumentoNotaCredito, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("El generador de nota de credito solo admite comprobantes tipo 04.");
        }

        if (string.IsNullOrWhiteSpace(notaCredito.ClaveAcceso) || notaCredito.ClaveAcceso.Length != 49)
        {
            throw new InvalidOperationException("La nota de credito no tiene una clave de acceso valida.");
        }

        if (string.IsNullOrWhiteSpace(notaCredito.RucEmisor) || notaCredito.RucEmisor.Length != 13)
        {
            throw new InvalidOperationException("La nota de credito no tiene un RUC emisor valido.");
        }

        if (string.IsNullOrWhiteSpace(notaCredito.Establecimiento) || notaCredito.Establecimiento.Length != 3)
        {
            throw new InvalidOperationException("La nota de credito no tiene un establecimiento valido.");
        }

        if (string.IsNullOrWhiteSpace(notaCredito.PuntoEmision) || notaCredito.PuntoEmision.Length != 3)
        {
            throw new InvalidOperationException("La nota de credito no tiene un punto de emision valido.");
        }

        if (string.IsNullOrWhiteSpace(notaCredito.ClienteTipoIdentificacion) || notaCredito.ClienteTipoIdentificacion.Length != 2)
        {
            throw new InvalidOperationException("La nota de credito no tiene tipo de identificacion del comprador valido.");
        }

        if (notaCredito.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La nota de credito debe contener al menos un detalle.");
        }
    }

    private static string ResolveCodigoPorcentajeIva(string? codigoIva, decimal porcentaje)
    {
        if (!string.IsNullOrWhiteSpace(codigoIva))
        {
            return SriTaxCatalog.ResolveCodigoPorcentaje(codigoIva, porcentaje);
        }

        return porcentaje switch
        {
            0m => "0",
            5m => "5",
            8m => "8",
            15m => "4",
            _ => throw new InvalidOperationException($"La tarifa IVA {porcentaje:0.##}% no esta soportada para nota de credito.")
        };
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static object? BuildOptionalElement(string name, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new XElement(name, value.Trim());
    }

    private static XElement? BuildInfoAdicional(ComprobanteCabeceraEntity notaCredito)
    {
        var campos = new List<XElement>();

        if (!string.IsNullOrWhiteSpace(notaCredito.ClienteDireccion))
        {
            campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Direccion"), notaCredito.ClienteDireccion));
        }

        return campos.Count == 0 ? null : new XElement("infoAdicional", campos);
    }
}
