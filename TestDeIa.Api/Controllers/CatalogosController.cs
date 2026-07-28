using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Catalogos.Ports.In;
using TestDeIa.Shared.Requests.Catalogos;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CatalogosController : ControllerBase
{
    private readonly ICatalogoUseCase catalogoUseCase;

    public CatalogosController(ICatalogoUseCase catalogoUseCase)
    {
        this.catalogoUseCase = catalogoUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetCatalogos(CancellationToken cancellationToken)
    {
        return Ok(await catalogoUseCase.GetCatalogosAsync(cancellationToken));
    }

    [HttpGet("{codigo}/items")]
    [AllowAnonymous]
    public async Task<IActionResult> GetItems(string codigo, [FromQuery] bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await catalogoUseCase.GetItemsAsync(codigo, onlyActive, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CatalogoItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await catalogoUseCase.CreateItemAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] CatalogoItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await catalogoUseCase.UpdateItemAsync(id, request, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
