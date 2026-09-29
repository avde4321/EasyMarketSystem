using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Tesoreria.Ports.In;
using TestDeIa.Shared.Requests.Tesoreria;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tesoreria")]
public sealed class TesoreriaController(
    ITesoreriaService tesoreriaService,
    IConciliacionService conciliacionService) : ControllerBase
{
    [HttpGet("cuentas-bancarias")]
    public async Task<IActionResult> GetCuentasBancarias(CancellationToken cancellationToken)
    {
        return Ok(await tesoreriaService.GetCuentasBancariasAsync(cancellationToken));
    }

    [HttpPost("cuentas-bancarias")]
    public async Task<IActionResult> SaveCuentaBancaria(
        [FromQuery] Guid? id,
        [FromBody] CuentaBancariaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tesoreriaService.SaveCuentaBancariaAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("importar-extracto")]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> ImportarExtracto(
        [FromForm] Guid cuentaBancariaId,
        [FromForm] IFormFile archivo,
        [FromForm] string? formato,
        CancellationToken cancellationToken)
    {
        if (archivo.Length == 0)
        {
            return BadRequest(new { message = "Debes adjuntar un archivo de extracto bancario." });
        }

        try
        {
            await using var stream = archivo.OpenReadStream();
            return Ok(await tesoreriaService.ImportarExtractoAsync(
                cuentaBancariaId,
                stream,
                archivo.FileName,
                formato,
                cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("movimientos-pendientes/{cuentaId:guid}")]
    public async Task<IActionResult> GetMovimientosPendientes(
        Guid cuentaId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? term,
        CancellationToken cancellationToken)
    {
        return Ok(await tesoreriaService.GetMovimientosPendientesAsync(cuentaId, desde, hasta, term, cancellationToken));
    }

    [HttpGet("extractos-pendientes/{cuentaId:guid}")]
    public async Task<IActionResult> GetExtractosPendientes(
        Guid cuentaId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] byte? tipoMovimiento,
        [FromQuery] string? term,
        CancellationToken cancellationToken)
    {
        return Ok(await tesoreriaService.GetExtractosPendientesAsync(cuentaId, desde, hasta, tipoMovimiento, term, cancellationToken));
    }

    [HttpPost("conciliar-automatico/{cuentaId:guid}")]
    public async Task<IActionResult> ConciliarAutomatico(Guid cuentaId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await conciliacionService.EjecutarConciliacionAutomaticaAsync(cuentaId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("conciliar-manual")]
    public async Task<IActionResult> ConciliarManual([FromBody] ConciliacionManualDto request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await conciliacionService.ConciliarManualAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
