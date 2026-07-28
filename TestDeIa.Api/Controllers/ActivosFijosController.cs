using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.ActivosFijos.Ports.In;
using TestDeIa.Shared.ActivosFijos;
using TestDeIa.Shared.Requests.ActivosFijos;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ActivosFijosController(IActivoFijoUseCase activoFijoUseCase) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = SecurityPolicyNames.InventarioView)]
    public async Task<IActionResult> Get(
        [FromQuery] CategoriaSriActivoFijo? categoria,
        [FromQuery] string? custodio,
        [FromQuery] EstadoActivoFijo? estado,
        CancellationToken cancellationToken = default)
    {
        return Ok(await activoFijoUseCase.GetAsync(categoria, custodio, estado, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = SecurityPolicyNames.InventarioView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var activo = await activoFijoUseCase.GetByIdAsync(id, cancellationToken);
        return activo is null ? NotFound() : Ok(activo);
    }

    [HttpPost]
    [Authorize(Policy = SecurityPolicyNames.InventarioManage)]
    public async Task<IActionResult> Create([FromBody] ActivoFijoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var activo = await activoFijoUseCase.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = activo.Id }, activo);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SecurityPolicyNames.InventarioManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActivoFijoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var activo = await activoFijoUseCase.UpdateAsync(id, request, cancellationToken);
            return activo is null ? NotFound() : Ok(activo);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
