using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Reporteria.Ports.In;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reporteria/gerencial")]
public sealed class ReporteriaGerencialController(
    IDashboardService dashboardService,
    IConsolidacionContableService consolidacionContableService,
    IReporteService reporteService,
    ITenantContextAccessor tenantContextAccessor) : ControllerBase
{
    [HttpGet("ventas-diarias")]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetVentasDiarias([FromQuery] DateTime? fecha, CancellationToken cancellationToken)
    {
        var empresaId = GetEmpresaId();
        return Ok(await dashboardService.ObtenerResumenVentasDiariasAsync(empresaId, (fecha ?? DateTime.Today).Date, cancellationToken));
    }

    [HttpGet("top-productos")]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetTopProductos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        var empresaId = GetEmpresaId();
        var fechaFin = (hasta ?? DateTime.Today).Date;
        var fechaInicio = (desde ?? fechaFin.AddDays(-30)).Date;
        return Ok(await dashboardService.ObtenerTopProductosMasVendidosAsync(empresaId, top, fechaInicio, fechaFin, cancellationToken));
    }

    [HttpGet("ventas-bodega-pago")]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetVentasPorBodegaMetodoPago(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken = default)
    {
        var empresaId = GetEmpresaId();
        var fechaFin = (hasta ?? DateTime.Today).Date;
        var fechaInicio = (desde ?? fechaFin.AddDays(-30)).Date;
        return Ok(await dashboardService.ObtenerVentasPorBodegaYMetodoPagoAsync(empresaId, fechaInicio, fechaFin, cancellationToken));
    }

    [HttpGet("rentabilidad")]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetRentabilidad(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken = default)
    {
        var empresaId = GetEmpresaId();
        var fechaFin = (hasta ?? DateTime.Today).Date;
        var fechaInicio = (desde ?? fechaFin.AddDays(-30)).Date;
        return Ok(await dashboardService.ObtenerKpisRentabilidadAsync(empresaId, fechaInicio, fechaFin, cancellationToken));
    }

    [HttpGet("consolidacion-diaria")]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetConsolidacionDiaria([FromQuery] DateTime? fecha, CancellationToken cancellationToken)
    {
        var empresaId = GetEmpresaId();
        return Ok(await consolidacionContableService.ObtenerResumenDiarioAsync(empresaId, (fecha ?? DateTime.Today).Date, cancellationToken));
    }

    [HttpGet("kardex-valorizado")]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetKardexValorizado(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] Guid? bodegaId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var fechaFin = (hasta ?? DateTime.Today).Date;
        var fechaInicio = (desde ?? fechaFin.AddDays(-30)).Date;
        return Ok(await reporteService.ObtenerKardexValorizadoAsync(fechaInicio, fechaFin, bodegaId, skip, take, cancellationToken));
    }

    [HttpGet("cierres-caja")]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetCierresCaja(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var fechaFin = (hasta ?? DateTime.Today).Date;
        var fechaInicio = (desde ?? fechaFin.AddDays(-30)).Date;
        return Ok(await reporteService.ObtenerCierresCajaAsync(fechaInicio, fechaFin, skip, take, cancellationToken));
    }

    private Guid GetEmpresaId()
    {
        return tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa para consultar reportería gerencial.");
    }
}
