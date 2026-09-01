using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.OfflinePos.Ports.In;
using TestDeIa.Shared.Requests.OfflinePos;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pos-offline")]
public sealed class PosOfflineController(IOfflineSyncService offlineSyncService) : ControllerBase
{
    [HttpGet("catalogo-cache")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> GetCatalogoCache([FromQuery] Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await offlineSyncService.ObtenerCatalogoPosOfflineAsync(bodegaId, cancellationToken));
    }

    [HttpPost("sincronizar")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> Sincronizar([FromBody] IReadOnlyCollection<VentaOfflineQueueDto> ventas, CancellationToken cancellationToken)
    {
        return Ok(await offlineSyncService.SincronizarVentasOfflineAsync(ventas, cancellationToken));
    }
}
