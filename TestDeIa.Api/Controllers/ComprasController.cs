using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ComprasController : ControllerBase
{
    private readonly ICompraUseCase compraUseCase;
    private readonly IEstudioMercadoUseCase estudioMercadoUseCase;

    public ComprasController(ICompraUseCase compraUseCase, IEstudioMercadoUseCase estudioMercadoUseCase)
    {
        this.compraUseCase = compraUseCase;
        this.estudioMercadoUseCase = estudioMercadoUseCase;
    }

    [HttpPost]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCompraRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var compra = await compraUseCase.RegistrarAsync(request, cancellationToken);
            return Ok(compra);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("estudio-mercado")]
    [Authorize(Policy = SecurityPolicyNames.ComprasEstudioMercado)]
    public async Task<IActionResult> GenerarEstudioMercado([FromQuery] int mes, [FromQuery] int anio, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await estudioMercadoUseCase.GenerarAsync(mes, anio, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
