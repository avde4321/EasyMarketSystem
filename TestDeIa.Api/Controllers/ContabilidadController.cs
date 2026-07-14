using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Contabilidad.Ports.In;
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
}