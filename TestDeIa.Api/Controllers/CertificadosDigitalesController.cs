using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/certificados-digitales")]
public sealed class CertificadosDigitalesController(ICertificadoDigitalEmpresaUseCase certificadoUseCase) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? term,
        [FromQuery] Guid? empresaId,
        [FromQuery] bool soloAlertas = false,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        return Ok(await certificadoUseCase.GetPagedAsync(term, empresaId, soloAlertas, skip, take, cancellationToken));
    }

    [HttpGet("alertas")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetAlertas(CancellationToken cancellationToken = default)
    {
        return Ok(await certificadoUseCase.GetAlertasAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var certificado = await certificadoUseCase.GetByIdAsync(id, cancellationToken);
        return certificado is null ? NotFound() : Ok(certificado);
    }

    [HttpPost]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> Create([FromBody] CertificadoDigitalEmpresaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var certificado = await certificadoUseCase.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = certificado.Id }, certificado);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/activar")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> Activar(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await certificadoUseCase.ActivarAsync(id, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await certificadoUseCase.DesactivarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
