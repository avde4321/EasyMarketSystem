using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Inventario.Ports.In;
using TestDeIa.Shared.Requests.Inventario;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class InventarioController : ControllerBase
{
    private readonly IInventarioUseCase inventarioUseCase;

    public InventarioController(IInventarioUseCase inventarioUseCase)
    {
        this.inventarioUseCase = inventarioUseCase;
    }

    [HttpGet("bodegas")]
    public async Task<IActionResult> GetBodegas(CancellationToken cancellationToken = default)
    {
        return Ok(await inventarioUseCase.GetBodegasAsync(cancellationToken));
    }

    [HttpGet("bodegas/{id:guid}")]
    public async Task<IActionResult> GetBodegaById(Guid id, CancellationToken cancellationToken = default)
    {
        var bodega = await inventarioUseCase.GetBodegaByIdAsync(id, cancellationToken);
        return bodega is null ? NotFound() : Ok(bodega);
    }

    [HttpPost("bodegas")]
    public async Task<IActionResult> CreateBodega([FromBody] BodegaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var bodega = await inventarioUseCase.CreateBodegaAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetBodegaById), new { id = bodega.Id }, bodega);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("bodegas/{id:guid}")]
    public async Task<IActionResult> UpdateBodega(Guid id, [FromBody] BodegaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var bodega = await inventarioUseCase.UpdateBodegaAsync(id, request, cancellationToken);
            return bodega is null ? NotFound() : Ok(bodega);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("productos")]
    public async Task<IActionResult> GetProductos([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);
        skip = Math.Max(0, skip);
        var productos = await inventarioUseCase.GetCatalogoPagedAsync(term, skip, take, bodegaId, cancellationToken);
        return Ok(productos);
    }

    [HttpGet("productos/{id:guid}")]
    public async Task<IActionResult> GetProductoById(Guid id, CancellationToken cancellationToken)
    {
        var producto = await inventarioUseCase.GetProductoByIdAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost("productos")]
    public async Task<IActionResult> CreateProducto(
        [FromBody] ProductoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var producto = await inventarioUseCase.CreateProductoAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetProductoById), new { id = producto.Id }, producto);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("productos/{id:guid}")]
    public async Task<IActionResult> UpdateProducto(
        Guid id,
        [FromBody] ProductoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var producto = await inventarioUseCase.UpdateProductoAsync(id, request, cancellationToken);
            return producto is null ? NotFound() : Ok(producto);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("productos/{id:guid}/kardex")]
    public async Task<IActionResult> GetKardex(Guid id, [FromQuery] Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        var movimientos = await inventarioUseCase.GetKardexAsync(id, bodegaId, cancellationToken);
        return Ok(movimientos);
    }

    [HttpPost("productos/{id:guid}/ajustes")]
    public async Task<IActionResult> AjustarStock(
        Guid id,
        [FromBody] AjusteStockRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var producto = await inventarioUseCase.AjustarStockAsync(id, request, cancellationToken);
            return producto is null ? NotFound() : Ok(producto);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("movimientos/compra")]
    public async Task<IActionResult> RegistrarCompra([FromBody] IngresoCompraRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var producto = await inventarioUseCase.RegistrarCompraAsync(request, cancellationToken);
            return producto is null ? NotFound() : Ok(producto);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("movimientos/merma")]
    public async Task<IActionResult> RegistrarMerma([FromBody] EgresoMermaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var producto = await inventarioUseCase.RegistrarMermaAsync(request, cancellationToken);
            return producto is null ? NotFound() : Ok(producto);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("movimientos/transferencia")]
    public async Task<IActionResult> TransferirStock([FromBody] TransferenciaInventarioRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var producto = await inventarioUseCase.TransferirStockAsync(request, cancellationToken);
            return producto is null ? NotFound() : Ok(producto);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("facturas/descontar-stock")]
    public async Task<IActionResult> DescontarStockPorFactura(
        [FromBody] DescontarStockFacturaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await inventarioUseCase.DescontarStockPorFacturaAsync(request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
