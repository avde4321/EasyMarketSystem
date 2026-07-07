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
        return empresa is null ? NoContent() : Ok(empresa);
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await empresaUseCase.GetMineAsync(cancellationToken));
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        return Ok(await empresaUseCase.GetPagedAsync(term, skip, take, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var empresa = await empresaUseCase.GetByIdAsync(id, cancellationToken);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpresaRequest request, CancellationToken cancellationToken)
    {
        return await SaveInternalAsync(null, request, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmpresaRequest request, CancellationToken cancellationToken)
    {
        return await SaveInternalAsync(id, request, cancellationToken);
    }

    private async Task<IActionResult> SaveInternalAsync(Guid? id, EmpresaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empresa = await empresaUseCase.SaveAsync(id, request, cancellationToken);
            return Ok(empresa);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
