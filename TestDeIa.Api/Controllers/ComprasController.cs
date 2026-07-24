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

    public ComprasController(
        ICompraUseCase compraUseCase,
        IEstudioMercadoUseCase estudioMercadoUseCase,
        IReporteComprasConsolidadoService reporteComprasConsolidadoService)
    {
        this.compraUseCase = compraUseCase;
        this.estudioMercadoUseCase = estudioMercadoUseCase;
        this.reporteComprasConsolidadoService = reporteComprasConsolidadoService;
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
