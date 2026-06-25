using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Clientes.Ports.In;
using TestDeIa.Shared.Requests.Clientes;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ClientesController : ControllerBase
{
    private readonly IClienteUseCase clienteUseCase;

    public ClientesController(IClienteUseCase clienteUseCase)
    {
        this.clienteUseCase = clienteUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);
        skip = Math.Max(0, skip);
        var clientes = await clienteUseCase.GetPagedAsync(term, skip, take, cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await clienteUseCase.GetByIdAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ClienteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await clienteUseCase.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] ClienteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await clienteUseCase.UpdateAsync(id, request, cancellationToken);
            return cliente is null ? NotFound() : Ok(cliente);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await clienteUseCase.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
