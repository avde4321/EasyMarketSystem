using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Shared.Requests.Facturacion;

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
    public async Task<IActionResult> SearchClientes([FromQuery] string term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        var clientes = await facturacionUseCase.SearchClientesAsync(term, skip, take, cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("productos")]
    public async Task<IActionResult> SearchProductos([FromQuery] string term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        var productos = await facturacionUseCase.SearchProductosAsync(term, skip, take, cancellationToken);
        return Ok(productos);
    }

    [HttpGet("puntos-emision")]
    public async Task<IActionResult> GetPuntosEmision(CancellationToken cancellationToken = default)
    {
        var puntos = await facturacionUseCase.GetPuntosEmisionAsync(cancellationToken);
        return Ok(puntos);
    }

    [HttpPost("facturas")]
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
    public async Task<IActionResult> GetMonitor(CancellationToken cancellationToken)
    {
        var facturas = await facturacionUseCase.GetMonitorAsync(cancellationToken);
        return Ok(facturas);
    }
}
