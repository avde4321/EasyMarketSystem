using System.Reflection;
using Microsoft.Reporting.NETCore;

namespace TestDeIa.Api.Reporting;

public sealed class FacturaRideRdlcRenderer
{
    private const string TemplateResourceName = "TestDeIa.Api.Reporting.Templates.FacturaRide.rdlc";

    public Task<byte[]> RenderAsync(FacturaRideReportModel model, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var watermarkText = string.Equals(model.Estado, "AUTORIZADO", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : model.WatermarkText;

            string? logoPath = null;
            string? temporaryLogoFilePath = null;
            using var report = new LocalReport();
            report.EnableExternalImages = true;
            using var reportDefinition = Assembly.GetExecutingAssembly().GetManifestResourceStream(TemplateResourceName)
                ?? throw new InvalidOperationException("No se encontro la plantilla RDLC de la factura.");

            report.LoadReportDefinition(reportDefinition);
            report.DataSources.Add(new ReportDataSource("DetalleItems", model.Detalles));
            report.DataSources.Add(new ReportDataSource("TotalesItems", model.Totales));
            (logoPath, temporaryLogoFilePath) = PrepareExternalImagePath(model);
            report.SetParameters([
                CreateParameter("BannerImagePath", logoPath),
                CreateParameter("NumeroComprobante", model.NumeroComprobante),
                CreateParameter("Estado", model.Estado),
                CreateParameter("ClaveAcceso", model.ClaveAcceso),
                CreateParameter("NumeroAutorizacion", model.NumeroAutorizacion),
                CreateParameter("FechaEmision", model.FechaEmision),
                CreateParameter("FechaAutorizacion", model.FechaAutorizacion),
                CreateParameter("EmisorRazonSocial", model.EmisorRazonSocial),
                CreateParameter("EmisorNombreComercial", model.EmisorNombreComercial),
                CreateParameter("EmisorRuc", model.EmisorRuc),
                CreateParameter("EmisorDireccionMatriz", model.EmisorDireccionMatriz),
                CreateParameter("EmisorDireccionSucursal", model.EmisorDireccionSucursal),
                CreateParameter("AmbienteSri", model.AmbienteSri),
                CreateParameter("EmisionTipo", model.EmisionTipo),
                CreateParameter("ObligadoContabilidad", model.ObligadoContabilidad),
                CreateParameter("ContribuyenteEspecial", model.ContribuyenteEspecial),
                CreateParameter("ClienteNombre", model.ClienteNombre),
                CreateParameter("ClienteIdentificacion", model.ClienteIdentificacion),
                CreateParameter("ClienteDireccion", model.ClienteDireccion),
                CreateParameter("ClienteEmail", model.ClienteEmail),
                CreateParameter("ClienteTelefono", model.ClienteTelefono),
                CreateParameter("FormaPago", model.FormaPago),
                CreateParameter("GuiaRemision", model.GuiaRemision),
                CreateParameter("Subtotal", model.Subtotal.ToString("0.00")),
                CreateParameter("IvaTotal", model.IvaTotal.ToString("0.00")),
                CreateParameter("Total", model.Total.ToString("0.00")),
                CreateParameter("Observacion", model.Observacion),
                CreateParameter("WatermarkText", watermarkText),
                CreateParameter("ValorTotalSinSubsidio", model.TotalSinSubsidio.ToString("0.00")),
                CreateParameter("AhorroSubsidio", model.TotalSubsidio.ToString("0.00"))
            ]);

            try
            {
                return report.Render("PDF");
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(temporaryLogoFilePath))
                {
                    TryDeleteTemporaryFile(temporaryLogoFilePath);
                }
            }
        }, cancellationToken);
    }

    private static ReportParameter CreateParameter(string name, string? value)
    {
        return new ReportParameter(name, value ?? string.Empty);
    }

    private static (string ImagePath, string? TemporaryFilePath) PrepareExternalImagePath(FacturaRideReportModel model)
    {
        if (model.BannerImageContent is not { Length: > 0 } || string.IsNullOrWhiteSpace(model.BannerImageMimeType))
        {
            return (model.BannerImagePath, null);
        }

        var extension = ResolveFileExtension(model.BannerImageMimeType);
        var temporaryFilePath = Path.Combine(Path.GetTempPath(), $"ride-logo-{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(temporaryFilePath, model.BannerImageContent);
        return (new Uri(temporaryFilePath).AbsoluteUri, temporaryFilePath);
    }

    private static string ResolveFileExtension(string mimeType)
    {
        return mimeType.Trim().ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/webp" => ".webp",
            _ => ".png"
        };
    }

    private static void TryDeleteTemporaryFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Si el visor mantiene el handle abierto unos milisegundos, no rompemos el flujo de reporte.
        }
    }
}
