using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Inventario;
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
        (skip, take) = QueryDefaults.NormalizePaging(skip, take);
        return Ok(await empresaUseCase.GetPagedAsync(term, skip, take, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var empresa = await empresaUseCase.GetByIdAsync(id, cancellationToken);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    [HttpGet("{id:guid}/bodegas")]
    public async Task<IActionResult> GetBodegas(Guid id, [FromServices] TestDeIaDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await empresaUseCase.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        // La empresa fue autorizada arriba; no depender de la empresa activa del selector.
        var bodegas = await dbContext.Bodegas.IgnoreQueryFilters().AsNoTracking()
            .Where(b => b.EmpresaId == id && b.IsActive)
            .OrderBy(b => b.Nombre)
            .Select(b => new BodegaResponse
            {
                Id = b.Id, Codigo = b.Codigo, Nombre = b.Nombre, Direccion = b.Direccion,
                EsPrincipal = b.EsPrincipal, IsActive = b.IsActive, CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt
            }).ToArrayAsync(cancellationToken);
        return Ok(bodegas);
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
