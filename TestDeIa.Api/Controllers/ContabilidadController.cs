using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Contabilidad.Exceptions;
using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ContabilidadController(IContabilidadUseCase contabilidadUseCase) : ControllerBase
{
    [HttpGet("plan-cuentas")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetPlanCuentas(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetPlanCuentasAsync(cancellationToken));
    }

    [HttpGet("cuentas-aceptables")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetCuentasAceptables(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetCuentasAceptablesAsync(cancellationToken));
    }

    [HttpGet("libro-diario")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetLibroDiario(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetLibroDiarioAsync(cancellationToken));
    }

    [HttpGet("libros/diario")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetLibroDiarioOficial(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetLibroDiarioAsync(desde, hasta, cancellationToken));
    }

    [HttpGet("libros/mayor")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetLibroMayor(
        [FromQuery] Guid cuentaContableId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetLibroMayorAsync(cuentaContableId, desde, hasta, cancellationToken));
    }

    [HttpGet("estados/balance-general")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetBalanceGeneral(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetBalanceGeneralAsync(cancellationToken));
    }

    [HttpGet("estados/estado-resultados")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetEstadoResultados(CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetEstadoResultadosAsync(cancellationToken));
    }

    [HttpGet("periodos")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> GetPeriodos([FromQuery] int anio, CancellationToken cancellationToken = default)
    {
        return Ok(await contabilidadUseCase.GetPeriodosAsync(anio, cancellationToken));
    }

    [HttpPost("periodos/cerrar")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> CerrarPeriodo([FromBody] CerrarPeriodoFiscalRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await contabilidadUseCase.CerrarPeriodoFiscalAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
    }

    [HttpPost("inventario/ajuste-contable")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> AjustarInventario([FromQuery] bool generarAsiento = false, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await contabilidadUseCase.AjustarInventarioContableAsync(generarAsiento, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
    }

    [HttpPost("asientos-manuales")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public async Task<IActionResult> CrearAsiento([FromBody] CrearAsientoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var numeroAsiento = await contabilidadUseCase.CrearAsientoAsync(request, cancellationToken);
            return Ok(new { NumeroAsiento = numeroAsiento });
        }
        catch (AsientoDescuadradoException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
    }
}
