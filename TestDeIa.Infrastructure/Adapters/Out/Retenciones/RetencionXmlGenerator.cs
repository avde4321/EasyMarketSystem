using System.Globalization;
using System.Xml.Linq;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Sri;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;

namespace TestDeIa.Infrastructure.Adapters.Out.Retenciones;

public sealed class RetencionXmlGenerator(SriDocumentSchemaValidator validator)
{
    // Versión ATS 2.0.0. El documento sustento procede de la compra, nunca de sumar bases de retención.
    public string Build(ComprobanteRetencionEntity retencion, EmpresaEmisoraEntity empresa, ProveedorEntity proveedor, CompraEntity compra)
    {
        var numero = $"{compra.Establecimiento}{compra.PuntoEmision}{compra.Secuencial}";
        if (retencion.Detalles.Count == 0 || retencion.Detalles.Any(x => x.NumDocSustento.Replace("-", "") != numero || x.CodDocSustento != compra.TipoDocumentoCodigo || x.FechaEmisionDocSustento.Date != compra.FechaEmision.Date))
            throw new InvalidOperationException("Los rubros de retención no corresponden a la compra vinculada.");
        if (string.IsNullOrWhiteSpace(compra.SustentoTributarioSRI) || string.IsNullOrWhiteSpace(compra.FormaPagoSriCodigo))
            throw new InvalidOperationException("Complete el sustento tributario y la forma de pago SRI de la compra.");
        var taxes = compra.Detalles.GroupBy(x => new { x.CodigoIva, x.PorcentajeIva }).Select(g => new
        {
            Code = SriTaxCatalog.ResolveCodigoPorcentaje(g.Key.CodigoIva, g.Key.PorcentajeIva),
            Rate = g.Key.PorcentajeIva, Base = g.Sum(x => x.CostoTotalSinImpuesto),
            Value = Math.Round(g.Sum(x => x.CostoTotalSinImpuesto * x.PorcentajeIva / 100m), 2, MidpointRounding.AwayFromZero)
        }).ToArray();
        if (Math.Abs(taxes.Sum(x => x.Value) - compra.TotalImpuestos) > .01m)
            throw new InvalidOperationException("Los impuestos de la compra no coinciden con su detalle. No se puede construir la retención ATS.");
        var doc = new XDocument(new XElement("comprobanteRetencion", new XAttribute("id", "comprobante"), new XAttribute("version", "2.0.0"),
            new XElement("infoTributaria",
                new XElement("ambiente", ((byte)retencion.AmbienteSRI).ToString()), new XElement("tipoEmision", ClaveAccesoService.GetTipoEmisionCode(empresa.TipoEmision)),
                new XElement("razonSocial", empresa.RazonSocial), new XElement("nombreComercial", empresa.NombreComercial ?? empresa.RazonSocial),
                new XElement("ruc", empresa.Ruc), new XElement("claveAcceso", retencion.ClaveAcceso), new XElement("codDoc", "07"),
                new XElement("estab", retencion.Establecimiento), new XElement("ptoEmi", retencion.PuntoEmision),
                new XElement("secuencial", retencion.Secuencial), new XElement("dirMatriz", empresa.DireccionMatriz)),
            new XElement("infoCompRetencion", new XElement("fechaEmision", retencion.FechaEmision.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                new XElement("dirEstablecimiento", empresa.DireccionEstablecimiento ?? empresa.DireccionMatriz),
                string.IsNullOrWhiteSpace(empresa.ContribuyenteEspecial) ? null : new XElement("contribuyenteEspecial", empresa.ContribuyenteEspecial),
                new XElement("obligadoContabilidad", empresa.ObligadoContabilidad ? "SI" : "NO"),
                new XElement("tipoIdentificacionSujetoRetenido", SriCatalogCodes.NormalizeTipoIdentificacionCode(proveedor.Persona.TipoIdentificacion)),
                new XElement("parteRel", retencion.ParteRelacionada ? "SI" : "NO"),
                new XElement("razonSocialSujetoRetenido", proveedor.Persona.RazonSocialONombresCompletos),
                new XElement("identificacionSujetoRetenido", proveedor.Persona.Identificacion),
                new XElement("periodoFiscal", retencion.FechaEmision.ToString("MM/yyyy", CultureInfo.InvariantCulture))),
            new XElement("docsSustento", new XElement("docSustento",
                new XElement("codSustento", compra.SustentoTributarioSRI), new XElement("codDocSustento", compra.TipoDocumentoCodigo),
                new XElement("numDocSustento", numero), new XElement("fechaEmisionDocSustento", compra.FechaEmision.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                string.IsNullOrWhiteSpace(compra.ClaveAccesoProveedor) ? null : new XElement("numAutDocSustento", compra.ClaveAccesoProveedor),
                new XElement("pagoLocExt", "01"),
                new XElement("totalSinImpuestos", Money(compra.Detalles.Sum(x => x.CostoTotalSinImpuesto))), new XElement("importeTotal", Money(compra.ImporteTotal)),
                new XElement("impuestosDocSustento", taxes.Select(x => new XElement("impuestoDocSustento",
                    new XElement("codImpuestoDocSustento", "2"), new XElement("codigoPorcentaje", x.Code),
                    new XElement("baseImponible", Money(x.Base)), new XElement("tarifa", Money(x.Rate)), new XElement("valorImpuesto", Money(x.Value))))),
                new XElement("retenciones", retencion.Detalles.Select(x => new XElement("retencion", new XElement("codigo", x.CodigoImpuesto),
                    new XElement("codigoRetencion", x.CodigoRetencionSRI), new XElement("baseImponible", Money(x.BaseImponible)),
                    new XElement("porcentajeRetener", Money(x.PorcentajeRetencion)), new XElement("valorRetenido", Money(x.ValorRetenido))))),
                new XElement("pagos", new XElement("pago", new XElement("formaPago", compra.FormaPagoSriCodigo), new XElement("total", Money(compra.ImporteTotal))))))));
        var xml = doc.ToString(SaveOptions.DisableFormatting);
        validator.Validate(xml, "comprobanteRetencion", "2.0.0");
        return xml;
    }
    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
