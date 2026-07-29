using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Api.Reporting;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ReporteriaController : ControllerBase
{
    private readonly FacturaDocumentQueryService facturaDocumentQueryService;
    private readonly FacturaRideRdlcRenderer facturaRideRdlcRenderer;

    public ReporteriaController(
        FacturaDocumentQueryService facturaDocumentQueryService,
        FacturaRideRdlcRenderer facturaRideRdlcRenderer)
    {
        this.facturaDocumentQueryService = facturaDocumentQueryService;
        this.facturaRideRdlcRenderer = facturaRideRdlcRenderer;
    }

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

    [HttpGet("comprobantes/{comprobanteId:guid}/ride")]
    public async Task<IActionResult> GetComprobanteRide(Guid comprobanteId, CancellationToken cancellationToken)
    {
        var comprobante = await facturaDocumentQueryService.GetComprobanteRideAsync(comprobanteId, cancellationToken);

        if (comprobante is null)
        {
            return NotFound();
        }

        var pdfBytes = await facturaRideRdlcRenderer.RenderAsync(comprobante, cancellationToken);
        return File(pdfBytes, "application/pdf", $"RIDE-{comprobante.NumeroComprobante}.pdf");
    }

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
}
