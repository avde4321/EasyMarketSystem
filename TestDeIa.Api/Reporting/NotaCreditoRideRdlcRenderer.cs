using System.Reflection;
using Microsoft.Reporting.NETCore;
using TestDeIa.Shared.Reports.Facturacion;

namespace TestDeIa.Api.Reporting;

public sealed class NotaCreditoRideRdlcRenderer
{
    private const string TemplateResourceName = "TestDeIa.Api.Reporting.Templates.NotaCreditoRide.rdlc";

    public Task<byte[]> RenderAsync(NotaCreditoRideReportDto model, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var report = new LocalReport();
            report.EnableExternalImages = true;
            using var reportDefinition = Assembly.GetExecutingAssembly().GetManifestResourceStream(TemplateResourceName)
                ?? throw new InvalidOperationException("No se encontro la plantilla RDLC de la nota de credito.");

            report.LoadReportDefinition(reportDefinition);
            report.DataSources.Add(new ReportDataSource("DetalleItems", BuildDetalles(model)));
            report.DataSources.Add(new ReportDataSource("TotalesItems", BuildTotales(model)));
            report.SetParameters([
                CreateParameter("BannerImagePath", string.Empty),
                CreateParameter("NumeroComprobante", model.NumeroComprobante),
                CreateParameter("Estado", string.Empty),
                CreateParameter("ClaveAcceso", model.ClaveAcceso),
                CreateParameter("NumeroAutorizacion", string.IsNullOrWhiteSpace(model.NumeroAutorizacion) ? "-" : model.NumeroAutorizacion),
                CreateParameter("FechaEmision", model.FechaEmision.LocalDateTime.ToString("dd/MM/yyyy")),
                CreateParameter("FechaAutorizacion", "-"),
                CreateParameter("EmisorRazonSocial", model.RazonSocialEmisor),
                CreateParameter("EmisorNombreComercial", model.NombreComercialEmisor),
                CreateParameter("EmisorRuc", model.RucEmisor),
                CreateParameter("EmisorDireccionMatriz", model.DireccionMatrizEmisor),
                CreateParameter("EmisorDireccionSucursal", model.DireccionEstablecimientoEmisor),
                CreateParameter("AmbienteSri", ResolveAmbiente(model.ClaveAcceso)),
                CreateParameter("EmisionTipo", "NORMAL"),
                CreateParameter("ObligadoContabilidad", "SI"),
                CreateParameter("ContribuyenteEspecial", string.Empty),
                CreateParameter("ClienteNombre", model.ClienteNombre),
                CreateParameter("ClienteIdentificacion", model.ClienteIdentificacion),
                CreateParameter("ClienteDireccion", model.ClienteDireccion),
                CreateParameter("ClienteEmail", "-"),
                CreateParameter("ClienteTelefono", "-"),
                CreateParameter("DocumentoModificado", $"{ResolveDocumentoModificado(model.CodDocModificado)} {model.NumDocModificado}"),
                CreateParameter("FechaEmisionDocSustento", model.FechaEmisionDocSustento.LocalDateTime.ToString("dd/MM/yyyy")),
                CreateParameter("Subtotal", model.Subtotal.ToString("0.00")),
                CreateParameter("IvaTotal", model.IvaTotal.ToString("0.00")),
                CreateParameter("Total", model.Total.ToString("0.00")),
                CreateParameter("MotivoModificacion", model.MotivoModificacion),
                CreateParameter("WatermarkText", string.Empty),
                CreateParameter("ValorTotalSinSubsidio", model.Total.ToString("0.00")),
                CreateParameter("AhorroSubsidio", "0.00")
            ]);

            return report.Render("PDF");
        }, cancellationToken);
    }

    private static IReadOnlyCollection<FacturaRideDetalleRow> BuildDetalles(NotaCreditoRideReportDto model)
    {
        return model.Detalles
            .Select(detalle => new FacturaRideDetalleRow
            {
                CodigoPrincipal = detalle.CodigoProducto,
                CodigoAuxiliar = string.Empty,
                Cantidad = detalle.Cantidad.ToString("0.####"),
                Descripcion = detalle.NombreProducto,
                DetalleAdicional1 = string.Empty,
                DetalleAdicional2 = string.Empty,
                DetalleAdicional3 = string.Empty,
                PrecioUnitario = detalle.PrecioUnitario.ToString("0.00"),
                Subsidio = "0.00",
                PrecioSinSubsidio = "0.00",
                Descuento = detalle.Descuento.ToString("0.00"),
                PrecioTotal = detalle.Total.ToString("0.00")
            })
            .ToArray();
    }

    private static IReadOnlyCollection<FacturaRideTotalRow> BuildTotales(NotaCreditoRideReportDto model)
    {
        var ivaLabel = model.TotalesImpuesto
            .Where(total => total.PorcentajeIva > 0m)
            .OrderByDescending(total => total.BaseImponible)
            .Select(total => $"IVA {total.PorcentajeIva:0.##}%")
            .FirstOrDefault() ?? "IVA";

        return
        [
            new() { Label = "SUBTOTAL SIN IMPUESTOS", Valor = model.Subtotal.ToString("0.00") },
            new() { Label = "DESCUENTO", Valor = model.TotalDescuento.ToString("0.00") },
            new() { Label = ivaLabel, Valor = model.IvaTotal.ToString("0.00") },
            new() { Label = "VALOR TOTAL", Valor = model.Total.ToString("0.00") }
        ];
    }

    private static string ResolveAmbiente(string claveAcceso)
    {
        return claveAcceso.Length >= 24 && claveAcceso[23] == '2' ? "PRODUCCION" : "PRUEBAS";
    }

    private static string ResolveDocumentoModificado(string codigo)
    {
        return codigo == "01" ? "FACTURA" : codigo;
    }

    private static ReportParameter CreateParameter(string name, string? value)
    {
        return new ReportParameter(name, value ?? string.Empty);
    }
}
