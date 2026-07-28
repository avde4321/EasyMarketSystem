using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Security;
using TestDeIa.Shared.Compras;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ComprasController : ControllerBase
{
    private readonly ICompraUseCase compraUseCase;
    private readonly IEstudioMercadoUseCase estudioMercadoUseCase;
    private readonly IReporteComprasConsolidadoService reporteComprasConsolidadoService;
    private readonly IFacturaProveedorAnalyzer facturaProveedorAnalyzer;

    public ComprasController(
        ICompraUseCase compraUseCase,
        IEstudioMercadoUseCase estudioMercadoUseCase,
        IReporteComprasConsolidadoService reporteComprasConsolidadoService,
        IFacturaProveedorAnalyzer facturaProveedorAnalyzer)
    {
        this.compraUseCase = compraUseCase;
        this.estudioMercadoUseCase = estudioMercadoUseCase;
        this.reporteComprasConsolidadoService = reporteComprasConsolidadoService;
        this.facturaProveedorAnalyzer = facturaProveedorAnalyzer;
    }

    [HttpPost]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCompraRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var compra = await compraUseCase.RegistrarAsync(request, cancellationToken);
            return Ok(compra);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("estudio-mercado")]
    [Authorize(Policy = SecurityPolicyNames.ComprasEstudioMercado)]
    public async Task<IActionResult> GenerarEstudioMercado([FromQuery] int mes, [FromQuery] int anio, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await estudioMercadoUseCase.GenerarAsync(mes, anio, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("analizar-factura-proveedor")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> AnalizarFacturaProveedor(IFormFile? archivo, CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new { message = "Debes seleccionar un archivo de factura proveedor." });
        }

        await using var stream = archivo.OpenReadStream();
        var result = await facturaProveedorAnalyzer.AnalyzeAsync(
            archivo.FileName,
            archivo.ContentType,
            stream,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("reporte-fisico-financiero")]
    [Authorize(Policy = SecurityPolicyNames.ComprasCuentasPorPagar)]
    public async Task<IActionResult> ConsultarReporteFisicoFinanciero(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] NaturalezaCompra? naturalezaCompra,
        CancellationToken cancellationToken)
    {
        try
        {
            var reporte = await reporteComprasConsolidadoService.ConsultarAsync(
                fechaInicio,
                fechaFin,
                naturalezaCompra,
                cancellationToken);

            return Ok(reporte);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
