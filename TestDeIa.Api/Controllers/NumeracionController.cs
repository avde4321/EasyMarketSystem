using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
[Route("api/empresa/numeracion")]
[Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
public sealed class NumeracionController(INumeracionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string ambiente = "1", CancellationToken ct = default)
    {
        if (ambiente is not ("1" or "2")) return BadRequest();
        return Ok(await service.ConsultarAsync(ambiente, ct));
    }

    [HttpGet("historial")]
    public async Task<IActionResult> Historial(CancellationToken ct) => Ok(await service.HistorialAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Put(NumeracionRequest request, CancellationToken ct)
    {
        try { await service.ConfigurarAsync(request, ct); return NoContent(); }
        catch (InvalidOperationException ex) { return Problem(statusCode: 409, title: "No se actualizó la numeración", detail: ex.Message); }
    }
}
