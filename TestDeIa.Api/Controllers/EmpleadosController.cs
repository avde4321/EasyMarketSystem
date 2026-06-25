using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Empleados.Ports.In;
using TestDeIa.Shared.Requests.Empleados;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoUseCase empleadoUseCase;

    public EmpleadosController(IEmpleadoUseCase empleadoUseCase)
    {
        this.empleadoUseCase = empleadoUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);
        skip = Math.Max(0, skip);
        return Ok(await empleadoUseCase.GetPagedAsync(term, skip, take, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var empleado = await empleadoUseCase.GetByIdAsync(id, cancellationToken);
        return empleado is null ? NotFound() : Ok(empleado);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpleadoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empleado = await empleadoUseCase.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = empleado.Id }, empleado);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmpleadoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empleado = await empleadoUseCase.UpdateAsync(id, request, cancellationToken);
            return empleado is null ? NotFound() : Ok(empleado);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await empleadoUseCase.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
