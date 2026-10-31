using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class FacturacionController : ControllerBase
{
    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("facturas/{id:guid}/procesamiento")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    public async Task<IActionResult> GetProcesamiento(Guid id, [FromServices] TestDeIaDbContext db, CancellationToken cancellationToken)
    {
        var factura = await db.Facturas.AsNoTracking().Where(f => f.Id == id)
            .Select(f => new { f.Id, f.Establecimiento, f.PuntoEmision, f.Secuencial, Estado = f.Estado.ToString(),
                f.MensajeEstado, f.RetryCount, f.ProcessingStartedAt, f.NextRetryAt, f.ClaveAcceso, f.NumeroAutorizacion }).SingleOrDefaultAsync(cancellationToken);
        return factura is null ? NotFound() : Ok(factura);
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpPost("facturas/{id:guid}/reprocesar")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    [EnableRateLimiting(SecurityRateLimitPolicyNames.IntegracionMasiva)]
    public async Task<IActionResult> Reprocesar(Guid id, [FromServices] TestDeIaDbContext db,
        [FromServices] IFacturacionBackgroundCoordinator coordinator, CancellationToken cancellationToken)
    {
        var factura = await db.Facturas.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (factura is null) return NotFound();
        if (factura.Estado is not (FacturaEstado.NO_FIRMADO or FacturaEstado.PENDIENTE) || factura.NumeroAutorizacion != null)
            return Conflict(new { message = "Solo se pueden reprocesar facturas sin firmar o pendientes de autorización. No se modifica un documento autorizado o rechazado." });
        if (factura.ProcessingStartedAt > DateTimeOffset.UtcNow.AddMinutes(-2))
            return Conflict(new { message = "El documento ya está siendo procesado. Espera a que termine." });
        await db.Facturas.Where(f => f.Id == id).ExecuteUpdateAsync(s => s.SetProperty(f => f.NextRetryAt, (DateTimeOffset?)null), cancellationToken);
        var tenant = HttpContext.RequestServices.GetRequiredService<TestDeIa.Application.Common.ITenantContextAccessor>();
        var originalUserId = tenant.UserId;
        try { await coordinator.ProcessFacturaAsync(id, $"manual-{Guid.NewGuid():N}", cancellationToken); }
        finally { tenant.EmpresaId = factura.EmpresaId; tenant.UserId = originalUserId; tenant.IsSystemContext = false; }
        return await GetProcesamiento(id, db, cancellationToken);
    }

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
        (skip, take) = QueryDefaults.NormalizePaging(skip, take);
        var clientes = await facturacionUseCase.SearchClientesAsync(term, skip, take, cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("productos")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> SearchProductos([FromQuery] string term, [FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        (skip, take) = QueryDefaults.NormalizePaging(skip, take);
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

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpPost("facturas")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    [EnableRateLimiting(SecurityRateLimitPolicyNames.IntegracionMasiva)]
    public async Task<IActionResult> EmitirFactura([FromBody] EmitirFacturaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await facturacionUseCase.EmitirFacturaAsync(request, cancellationToken);
            return Accepted(response);
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("monitor")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    public async Task<IActionResult> GetMonitor(
        [FromQuery] string? term,
        [FromQuery] string? tipoDocumentoId = null,
        [FromQuery] string? estadoSriId = null,
        [FromQuery] DateTimeOffset? fechaDesde = null,
        [FromQuery] DateTimeOffset? fechaHasta = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = QueryDefaults.DefaultTake,
        CancellationToken cancellationToken = default)
    {
        (skip, take) = QueryDefaults.NormalizePaging(skip, take);
        var rango = QueryDefaults.ResolveCurrentMonthRange(fechaDesde, fechaHasta);
        return Ok(await facturacionUseCase.GetMonitorAsync(term, tipoDocumentoId, estadoSriId, rango.Desde, rango.Hasta, skip, take, cancellationToken));
    }

    [HttpGet("comisiones/liquidacion")]
    [Authorize(Roles = SecurityRoleNames.AdministradoresEmpresa)]
    public async Task<IActionResult> GetLiquidacionComisiones([FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] Guid? operadorId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await comisionesUseCase.GetLiquidacionAsync(desde, hasta, operadorId, cancellationToken));
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpPost("notas-credito")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    [EnableRateLimiting(SecurityRateLimitPolicyNames.IntegracionMasiva)]
    public async Task<IActionResult> CrearNotaCredito([FromBody] NotaCreditoRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await notaCreditoService.CrearNotaCredito(request, cancellationToken);
            return Accepted(response);
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [TestDeIa.Api.Security.RequirePlanModule(TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri)]
    [HttpGet("facturas/{facturaId:guid}/nota-credito-origen")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    public async Task<IActionResult> GetNotaCreditoOrigen(Guid facturaId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await notaCreditoService.GetFacturaOrigenAsync(facturaId, cancellationToken));
        }
        catch (InvalidOperationException exception) when (exception is not TestDeIa.Application.Common.SaaSQuotaExceededException)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}


