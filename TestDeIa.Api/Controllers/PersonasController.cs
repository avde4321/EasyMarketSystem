using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Personas.Ports.In;
using TestDeIa.Shared.Requests.Personas;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class PersonasController : ControllerBase
{
    private readonly IPersonaUseCase personaUseCase;

    public PersonasController(IPersonaUseCase personaUseCase)
    {
        this.personaUseCase = personaUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var personas = await personaUseCase.GetAllAsync(cancellationToken);
        return Ok(personas);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var persona = await personaUseCase.GetByIdAsync(id, cancellationToken);
        return persona is null ? NotFound() : Ok(persona);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> FindByIdentificacion([FromQuery] string identificacion, CancellationToken cancellationToken)
    {
        var persona = await personaUseCase.FindByIdentificacionAsync(identificacion, cancellationToken);
        return persona is null ? NotFound() : Ok(persona);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] PersonaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var persona = await personaUseCase.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = persona.Id }, persona);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] PersonaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var persona = await personaUseCase.UpdateAsync(id, request, cancellationToken);
            return persona is null ? NotFound() : Ok(persona);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
