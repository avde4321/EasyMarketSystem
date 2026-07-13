using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class FacturacionController : ControllerBase
{
    private readonly IFacturacionUseCase facturacionUseCase;

    public FacturacionController(IFacturacionUseCase facturacionUseCase)
    {
        this.facturacionUseCase = facturacionUseCase;
    }

    [HttpGet("clientes")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> SearchClientes([FromQuery] string term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        var clientes = await facturacionUseCase.SearchClientesAsync(term, skip, take, cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("productos")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> SearchProductos([FromQuery] string term, [FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        var productos = await facturacionUseCase.SearchProductosAsync(term, skip, take, bodegaId, cancellationToken);
        return Ok(productos);
    }

    [HttpGet("puntos-emision")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> GetPuntosEmision(CancellationToken cancellationToken = default)
    {
        var puntos = await facturacionUseCase.GetPuntosEmisionAsync(cancellationToken);
        return Ok(puntos);
    }

    [HttpPost("facturas")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> EmitirFactura([FromBody] EmitirFacturaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await facturacionUseCase.EmitirFacturaAsync(request, cancellationToken);
            return Accepted(response);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("monitor")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    public async Task<IActionResult> GetMonitor([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        return Ok(await facturacionUseCase.GetMonitorAsync(term, skip, take, cancellationToken));
    }
}
