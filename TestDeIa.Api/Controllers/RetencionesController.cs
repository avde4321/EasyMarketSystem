using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TestDeIa.Application.Modules.Retenciones.Ports.In;
using TestDeIa.Shared.Requests.Retenciones;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
[Authorize]
[Route("api/retenciones")]
public sealed class RetencionesController(IRetencionService retencionService) : ControllerBase
{
    [HttpGet("compras-pendientes")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> GetComprasPendientesRetencion(
        [FromQuery] string? term,
        [FromQuery] int skip = 0,
        [FromQuery] int take = QueryDefaults.DefaultTake,
        CancellationToken cancellationToken = default)
    {
        (skip, take) = QueryDefaults.NormalizePaging(skip, take);
        return Ok(await retencionService.GetComprasPendientesRetencionAsync(term, skip, take, cancellationToken));
    }

    [HttpPut("{id:guid}/parte-relacionada")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> ActualizarParteRelacionada(Guid id, [FromBody] bool value, CancellationToken cancellationToken)
    {
        try { await retencionService.ActualizarParteRelacionadaAsync(id, value, cancellationToken); return NoContent(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? term,
        [FromQuery] DateTimeOffset? fechaDesde = null,
        [FromQuery] DateTimeOffset? fechaHasta = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = QueryDefaults.DefaultTake,
        CancellationToken cancellationToken = default)
    {
        (skip, take) = QueryDefaults.NormalizePaging(skip, take);
        var rango = QueryDefaults.ResolveCurrentMonthRange(fechaDesde, fechaHasta);
        return Ok(await retencionService.GetPagedAsync(term, rango.Desde, rango.Hasta, skip, take, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var retencion = await retencionService.GetByIdAsync(id, cancellationToken);
        return retencion is null ? NotFound() : Ok(retencion);
    }

    [HttpPost]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> Create([FromBody] ComprobanteRetencionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await retencionService.CreateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("desde-compra/{compraId:guid}")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    public async Task<IActionResult> CrearDesdeCompra(Guid compraId, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await retencionService.CrearRetencionDesdeCompraAsync(compraId, cancellationToken));
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/procesar-sri")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    [EnableRateLimiting(SecurityRateLimitPolicyNames.IntegracionMasiva)]
    public async Task<IActionResult> ProcesarSri(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return Accepted(await retencionService.ProcesarRetencionSRIAsync(id, cancellationToken));
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/reintentar-autorizacion")]
    [Authorize(Policy = SecurityPolicyNames.ComprasRegistrar)]
    [EnableRateLimiting(SecurityRateLimitPolicyNames.IntegracionMasiva)]
    public async Task<IActionResult> ReintentarAutorizacion(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return Accepted(await retencionService.ProcesarRetencionSRIAsync(id, cancellationToken));
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}


