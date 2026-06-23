using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Shared.Requests.Empresa;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class EmpresaController : ControllerBase
{
    private readonly IEmpresaUseCase empresaUseCase;

    public EmpresaController(IEmpresaUseCase empresaUseCase)
    {
        this.empresaUseCase = empresaUseCase;
    }

    [HttpGet("actual")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var empresa = await empresaUseCase.GetCurrentAsync(cancellationToken);
        return Ok(empresa);
    }

    [HttpPut("actual")]
    public async Task<IActionResult> Upsert([FromBody] EmpresaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empresa = await empresaUseCase.UpsertAsync(request, cancellationToken);
            return Ok(empresa);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
