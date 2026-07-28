using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Caja.Ports.In;
using TestDeIa.Shared.Requests.Caja;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CajaController(ICajaSesionUseCase cajaSesionUseCase) : ControllerBase
{
    [HttpGet("activa")]
    [Authorize(Policy = SecurityPolicyNames.CajaOperar)]
    public async Task<IActionResult> GetActiva(CancellationToken cancellationToken)
    {
        var caja = await cajaSesionUseCase.GetActivaAsync(cancellationToken);
        return caja is null ? NoContent() : Ok(caja);
    }

    [HttpPost("abrir")]
    [Authorize(Policy = SecurityPolicyNames.CajaOperar)]
    public async Task<IActionResult> Abrir([FromBody] AbrirCajaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await cajaSesionUseCase.AbrirAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("cerrar")]
    [Authorize(Policy = SecurityPolicyNames.CajaOperar)]
    public async Task<IActionResult> Cerrar([FromBody] CerrarCajaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await cajaSesionUseCase.CerrarAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
