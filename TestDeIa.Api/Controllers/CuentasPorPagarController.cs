using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Shared.Requests.Compras;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cuentas-por-pagar")]
public sealed class CuentasPorPagarController : ControllerBase
{
    private readonly ICompraUseCase compraUseCase;

    public CuentasPorPagarController(ICompraUseCase compraUseCase)
    {
        this.compraUseCase = compraUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? term,
        [FromQuery] Guid? proveedorId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);
        skip = Math.Max(0, skip);
        return Ok(await compraUseCase.GetCuentasPorPagarAsync(term, proveedorId, skip, take, cancellationToken));
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(CancellationToken cancellationToken)
    {
        return Ok(await compraUseCase.GetCuentasPorPagarResumenAsync(cancellationToken));
    }

    [HttpPost("abonos")]
    public async Task<IActionResult> RegistrarAbono([FromBody] RegistrarAbonoCxPRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await compraUseCase.RegistrarAbonoAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
