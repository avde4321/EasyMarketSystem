using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Contabilidad.Exceptions;
using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ContabilidadController(IContabilidadUseCase contabilidadUseCase) : ControllerBase
{
    [HttpGet("plan-cuentas")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetPlanCuentas(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetPlanCuentasAsync(cancellationToken));
    }

    [HttpGet("cuentas-aceptables")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetCuentasAceptables(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetCuentasAceptablesAsync(cancellationToken));
    }

    [HttpGet("libro-diario")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetLibroDiario(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetLibroDiarioAsync(cancellationToken));
    }

    [HttpPost("asientos-manuales")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> CrearAsiento([FromBody] CrearAsientoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var numeroAsiento = await contabilidadUseCase.CrearAsientoAsync(request, cancellationToken);
            return Ok(new { NumeroAsiento = numeroAsiento });
        }
        catch (AsientoDescuadradoException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
    }
}
