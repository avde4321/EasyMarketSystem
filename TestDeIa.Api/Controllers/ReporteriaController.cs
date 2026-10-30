using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Api.Reporting;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ReporteriaController : ControllerBase
{
    private readonly FacturaDocumentQueryService facturaDocumentQueryService;
    private readonly FacturaRideRdlcRenderer facturaRideRdlcRenderer;
    private readonly NotaCreditoRideRdlcRenderer notaCreditoRideRdlcRenderer;
    private readonly SimpleRidePdfRenderer simpleRidePdfRenderer;
    private readonly TestDeIaDbContext dbContext;

    public ReporteriaController(
        FacturaDocumentQueryService facturaDocumentQueryService,
        FacturaRideRdlcRenderer facturaRideRdlcRenderer,
        NotaCreditoRideRdlcRenderer notaCreditoRideRdlcRenderer,
        SimpleRidePdfRenderer simpleRidePdfRenderer,
        TestDeIaDbContext dbContext)
    {
        this.facturaDocumentQueryService = facturaDocumentQueryService;
        this.facturaRideRdlcRenderer = facturaRideRdlcRenderer;
        this.notaCreditoRideRdlcRenderer = notaCreditoRideRdlcRenderer;
        this.simpleRidePdfRenderer = simpleRidePdfRenderer;
        this.dbContext = dbContext;
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("facturas/{facturaId:guid}/ride")]
    public async Task<IActionResult> GetRide(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await facturaDocumentQueryService.GetFacturaRideAsync(facturaId, cancellationToken);

        if (factura is null)
        {
            return NotFound();
        }

        var pdfBytes = await facturaRideRdlcRenderer.RenderAsync(factura, cancellationToken);
        var fileName = $"RIDE-{factura.NumeroComprobante}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("comprobantes/{comprobanteId:guid}/ride")]
    public async Task<IActionResult> GetComprobanteRide(Guid comprobanteId, CancellationToken cancellationToken)
    {
        var notaCredito = await facturaDocumentQueryService.GetNotaCreditoRideAsync(comprobanteId, cancellationToken);
        if (notaCredito is not null)
        {
            var notaPdfBytes = await notaCreditoRideRdlcRenderer.RenderAsync(notaCredito, cancellationToken);
            return File(notaPdfBytes, "application/pdf", $"RIDE-NC-{notaCredito.NumeroComprobante}.pdf");
        }

        var comprobante = await facturaDocumentQueryService.GetComprobanteRideAsync(comprobanteId, cancellationToken);

        if (comprobante is null)
        {
            return NotFound();
        }

        var pdfBytes = await facturaRideRdlcRenderer.RenderAsync(comprobante, cancellationToken);
        return File(pdfBytes, "application/pdf", $"RIDE-{comprobante.NumeroComprobante}.pdf");
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("comprobantes/{comprobanteId:guid}/xml-generado")]
    public async Task<IActionResult> GetComprobanteXmlGenerado(Guid comprobanteId, CancellationToken cancellationToken)
    {
        var comprobante = await facturaDocumentQueryService.GetComprobanteRideAsync(comprobanteId, cancellationToken);

        if (comprobante is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(comprobante.XmlGenerado))
        {
            return Conflict(new { message = "El comprobante aun no tiene XML generado." });
        }

        return File(
            Encoding.UTF8.GetBytes(comprobante.XmlGenerado),
            "application/xml",
            $"COMPROBANTE-{comprobante.NumeroComprobante}-xml-generado.xml");
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("comprobantes/{comprobanteId:guid}/xml-firmado")]
    public async Task<IActionResult> GetComprobanteXmlFirmado(Guid comprobanteId, CancellationToken cancellationToken)
    {
        var comprobante = await facturaDocumentQueryService.GetComprobanteRideAsync(comprobanteId, cancellationToken);

        if (comprobante is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(comprobante.XmlFirmado))
        {
            return Conflict(new { message = "El comprobante aun no tiene XML firmado disponible." });
        }

        return File(
            Encoding.UTF8.GetBytes(comprobante.XmlFirmado),
            "application/xml",
            $"COMPROBANTE-{comprobante.NumeroComprobante}-xml-firmado.xml");
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("facturas/{facturaId:guid}/xml-generado")]
    public async Task<IActionResult> GetXmlGenerado(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await facturaDocumentQueryService.GetFacturaRideAsync(facturaId, cancellationToken);

        if (factura is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(factura.XmlGenerado))
        {
            return Conflict(new { message = "La factura aun no tiene XML generado." });
        }

        return File(
            Encoding.UTF8.GetBytes(factura.XmlGenerado),
            "application/xml",
            $"FACTURA-{factura.NumeroComprobante}-xml-generado.xml");
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("facturas/{facturaId:guid}/xml-firmado")]
    public async Task<IActionResult> GetXmlFirmado(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await facturaDocumentQueryService.GetFacturaRideAsync(facturaId, cancellationToken);

        if (factura is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(factura.XmlFirmado))
        {
            return Conflict(new { message = "La factura aun no tiene XML firmado disponible." });
        }

        return File(
            Encoding.UTF8.GetBytes(factura.XmlFirmado),
            "application/xml",
            $"FACTURA-{factura.NumeroComprobante}-xml-firmado.xml");
    }

    [HttpGet("proformas/{proformaId:guid}/ride")]
    public async Task<IActionResult> GetProformaRide(Guid proformaId, CancellationToken cancellationToken)
    {
        var proforma = await dbContext.Proformas
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == proformaId, cancellationToken);

        if (proforma is null)
        {
            return NotFound();
        }

        var lines = new List<string>
        {
            "EASYMARKET SYSTEMS - PROFORMA / COTIZACION",
            $"Secuencial: {proforma.Secuencial}",
            $"Fecha emision: {proforma.FechaEmision:dd/MM/yyyy}",
            $"Fecha vencimiento: {proforma.FechaVencimiento:dd/MM/yyyy}",
            $"ClienteId: {proforma.ClienteId}",
            $"BodegaId: {proforma.BodegaId}",
            $"Estado: {proforma.Estado}",
            $"Observacion: {proforma.Observacion}",
            "DETALLE"
        };

        lines.AddRange(proforma.Detalles.Select(detalle =>
            $"Producto {detalle.ProductoId} | Cant: {detalle.Cantidad:N2} | Precio: {detalle.PrecioUnitario:N2} | IVA: {detalle.ValorIVA:N2} | Subtotal: {detalle.Subtotal:N2}"));
        lines.Add($"Subtotal: {proforma.SubtotalSinImpuestos:N2}");
        lines.Add($"Descuento: {proforma.DescuentoTotal:N2}");
        lines.Add($"IVA: {proforma.SubtotalIVA:N2}");
        lines.Add($"TOTAL: {proforma.Total:N2}");

        var pdfBytes = simpleRidePdfRenderer.Render($"PROFORMA {proforma.Secuencial}", lines);
        return File(pdfBytes, "application/pdf", $"PROFORMA-{proforma.Secuencial}.pdf");
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("retenciones/{retencionId:guid}/ride")]
    public async Task<IActionResult> GetRetencionRide(Guid retencionId, CancellationToken cancellationToken)
    {
        var retencion = await dbContext.ComprobantesRetencion
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == retencionId, cancellationToken);

        if (retencion is null)
        {
            return NotFound();
        }

        var numero = $"{retencion.Establecimiento}-{retencion.PuntoEmision}-{retencion.Secuencial}";
        var lines = new List<string>
        {
            "EASYMARKET SYSTEMS - COMPROBANTE DE RETENCION SRI",
            $"Numero: {numero}",
            $"Fecha emision: {retencion.FechaEmision:dd/MM/yyyy}",
            $"ProveedorId: {retencion.ProveedorId}",
            $"Ambiente: {retencion.AmbienteSRI}",
            $"Estado SRI: {retencion.EstadoSRI}",
            $"Clave acceso: {retencion.ClaveAcceso}",
            $"Numero autorizacion: {retencion.NumeroAutorizacion}",
            $"Fecha autorizacion: {retencion.FechaAutorizacion:dd/MM/yyyy HH:mm}",
            "DETALLE RETENCIONES"
        };

        lines.AddRange(retencion.Detalles.Select(detalle =>
            $"Imp {detalle.CodigoImpuesto} | Cod {detalle.CodigoRetencionSRI} | Base {detalle.BaseImponible:N2} | % {detalle.PorcentajeRetencion:N2} | Retenido {detalle.ValorRetenido:N2} | Doc {detalle.CodDocSustento}-{detalle.NumDocSustento}"));
        lines.Add($"TOTAL RETENIDO: {retencion.TotalRetenido:N2}");
        lines.Add($"Mensaje SRI: {retencion.MensajeErrorSRI}");

        var pdfBytes = simpleRidePdfRenderer.Render($"RETENCION {numero}", lines);
        return File(pdfBytes, "application/pdf", $"RETENCION-{numero}.pdf");
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("retenciones/{retencionId:guid}/xml-generado")]
    public async Task<IActionResult> GetRetencionXmlGenerado(Guid retencionId, [FromServices] TestDeIa.Infrastructure.Adapters.Out.Sri.SriXmlArtifactService artifacts, CancellationToken cancellationToken)
    {
        var retencion = await dbContext.ComprobantesRetencion
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == retencionId, cancellationToken);

        if (retencion is null)
        {
            return NotFound();
        }

        var xml = await artifacts.ReadAsync(retencion.EmpresaId, "ComprobanteRetencion", retencion.Id, "XML_AUTORIZADO", cancellationToken)
            ?? await artifacts.ReadAsync(retencion.EmpresaId, "ComprobanteRetencion", retencion.Id, "XML_FIRMADO", cancellationToken)
            ?? await artifacts.ReadAsync(retencion.EmpresaId, "ComprobanteRetencion", retencion.Id, "XML_GENERADO", cancellationToken);
        if (xml is null) return Problem(statusCode: 409, detail: "La retención todavía no tiene un XML procesado. Ejecute el procesamiento SRI antes de descargarlo.");
        var numero = $"{retencion.Establecimiento}-{retencion.PuntoEmision}-{retencion.Secuencial}";
        return File(Encoding.UTF8.GetBytes(xml), "application/xml", $"RETENCION-{numero}-xml-generado.xml");
    }

}
