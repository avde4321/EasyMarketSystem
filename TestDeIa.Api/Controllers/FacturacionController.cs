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
    private readonly IComisionesUseCase comisionesUseCase;
    private readonly INotaCreditoService notaCreditoService;

    public FacturacionController(
        IFacturacionUseCase facturacionUseCase,
        IComisionesUseCase comisionesUseCase,
        INotaCreditoService notaCreditoService)
    {
        this.facturacionUseCase = facturacionUseCase;
        this.comisionesUseCase = comisionesUseCase;
        this.notaCreditoService = notaCreditoService;
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

    [HttpGet("operadores")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> GetOperadores(CancellationToken cancellationToken = default)
    {
        var operadores = await facturacionUseCase.GetOperadoresAsync(cancellationToken);
        return Ok(operadores);
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
    public async Task<IActionResult> GetMonitor([FromQuery] string? term, [FromQuery] string? tipoDocumentoId = null, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        return Ok(await facturacionUseCase.GetMonitorAsync(term, tipoDocumentoId, skip, take, cancellationToken));
    }

    [HttpGet("comisiones/liquidacion")]
    [Authorize(Roles = SecurityRoleNames.Administrador)]
    public async Task<IActionResult> GetLiquidacionComisiones([FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] Guid? operadorId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await comisionesUseCase.GetLiquidacionAsync(desde, hasta, operadorId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("notas-credito")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    public async Task<IActionResult> CrearNotaCredito([FromBody] NotaCreditoRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await notaCreditoService.CrearNotaCredito(request, cancellationToken);
            return Accepted(response);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("facturas/{facturaId:guid}/nota-credito-origen")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    public async Task<IActionResult> GetNotaCreditoOrigen(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await notaCreditoService.GetFacturaOrigenAsync(facturaId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
