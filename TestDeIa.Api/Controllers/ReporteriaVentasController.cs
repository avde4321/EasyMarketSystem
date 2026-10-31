using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Reporteria.Ports.In;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reporteria/ventas")]
public sealed class ReporteriaVentasController(IReporteVentasUseCase reporteVentasUseCase) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = SecurityPolicyNames.ReporteriaVentas)]
    public async Task<IActionResult> GetVentas(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken cancellationToken = default)
    {
        var hasta = (fechaFin ?? DateTime.Today).Date;
        var desde = (fechaInicio ?? hasta.AddMonths(-3).AddDays(1)).Date;

        try
        {
            return Ok(await reporteVentasUseCase.GetVentasAsync(desde, hasta, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
