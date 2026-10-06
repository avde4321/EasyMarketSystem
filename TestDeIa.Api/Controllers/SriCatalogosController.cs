using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Sri.Ports.In;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize(Policy = SecurityPolicyNames.CatalogosAdministrar)]
[Route("api/sri/catalogos")]
public sealed class SriCatalogosController(ISriStateEngineService sriStateEngineService) : ControllerBase
{
    [HttpGet("errores")]
    public async Task<IActionResult> GetErrores([FromQuery] string? term, CancellationToken cancellationToken)
    {
        return Ok(await sriStateEngineService.GetErroresAsync(term, cancellationToken));
    }

    [HttpGet("errores/{codigoSri}")]
    public async Task<IActionResult> GetErrorByCodigo(string codigoSri, CancellationToken cancellationToken)
    {
        var response = await sriStateEngineService.GetErrorByCodigoAsync(codigoSri, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("estados-comprobante")]
    public async Task<IActionResult> GetEstados(CancellationToken cancellationToken)
    {
        return Ok(await sriStateEngineService.GetEstadosAsync(cancellationToken));
    }
}
