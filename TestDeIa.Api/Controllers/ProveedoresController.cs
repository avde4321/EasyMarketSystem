using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Shared.Requests.Compras;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ProveedoresController : ControllerBase
{
    private readonly IProveedorUseCase proveedorUseCase;

    public ProveedoresController(IProveedorUseCase proveedorUseCase)
    {
        this.proveedorUseCase = proveedorUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);
        skip = Math.Max(0, skip);
        var proveedores = await proveedorUseCase.GetPagedAsync(term, skip, take, cancellationToken);
        return Ok(proveedores);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var proveedor = await proveedorUseCase.GetByIdAsync(id, cancellationToken);
        return proveedor is null ? NotFound() : Ok(proveedor);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProveedorRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var proveedor = await proveedorUseCase.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, proveedor);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProveedorRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var proveedor = await proveedorUseCase.UpdateAsync(id, request, cancellationToken);
            return proveedor is null ? NotFound() : Ok(proveedor);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await proveedorUseCase.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
