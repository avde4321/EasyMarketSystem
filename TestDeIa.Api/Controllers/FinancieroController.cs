using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Financiero.Ports.In;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/financiero/reportes")]
public sealed class FinancieroController : ControllerBase
{
    private readonly IFinancieroReportesUseCase financieroReportesUseCase;

    public FinancieroController(IFinancieroReportesUseCase financieroReportesUseCase)
    {
        this.financieroReportesUseCase = financieroReportesUseCase;
    }

    [HttpGet("iva-mensual")]
    [Authorize(Policy = SecurityPolicyNames.FinancieroIva)]
    public async Task<IActionResult> GetIvaMensual(
        [FromQuery] int mes,
        [FromQuery] int anio,
        [FromQuery] string? puntoEmision,
        [FromQuery] string? cajero,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await financieroReportesUseCase.ObtenerConsolidadoIvaAsync(mes, anio, puntoEmision, cajero, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
