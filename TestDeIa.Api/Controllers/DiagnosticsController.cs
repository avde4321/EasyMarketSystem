using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.OfflinePos.Ports.In;
using TestDeIa.Application.Modules.XmlSri.Ports.In;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
[Route("api/diagnostics")]
public sealed class DiagnosticsController(
    IXmlComprasTestService xmlComprasTestService,
    IPosOfflineTestService posOfflineTestService) : ControllerBase
{
    [HttpGet("xml-compras")]
    public async Task<IActionResult> RunXmlComprasSuite(CancellationToken cancellationToken)
    {
        return Ok(await xmlComprasTestService.EjecutarSuiteAsync(cancellationToken));
    }

    [HttpGet("xml-compras/estructura")]
    public async Task<IActionResult> RunXmlStructureTest(CancellationToken cancellationToken)
    {
        return Ok(await xmlComprasTestService.ValidarEstructuraXmlSriTestAsync(cancellationToken));
    }

    [HttpGet("xml-compras/duplicados")]
    public async Task<IActionResult> RunXmlDuplicatesTest(CancellationToken cancellationToken)
    {
        return Ok(await xmlComprasTestService.EvitarDuplicadosXmlTestAsync(cancellationToken));
    }

    [HttpGet("xml-compras/conversion-compra")]
    public async Task<IActionResult> RunXmlConversionTest(CancellationToken cancellationToken)
    {
        return Ok(await xmlComprasTestService.ConversionACompraEInventarioTestAsync(cancellationToken));
    }

    [HttpGet("pos-offline")]
    public async Task<IActionResult> RunPosOfflineSuite(CancellationToken cancellationToken)
    {
        return Ok(await posOfflineTestService.EjecutarSuiteAsync(cancellationToken));
    }

    [HttpGet("pos-offline/lote")]
    public async Task<IActionResult> RunPosBatchTest(CancellationToken cancellationToken)
    {
        return Ok(await posOfflineTestService.SincronizacionMasivaEnLoteTestAsync(cancellationToken));
    }

    [HttpGet("pos-offline/conflicto-stock")]
    public async Task<IActionResult> RunPosStockConflictTest(CancellationToken cancellationToken)
    {
        return Ok(await posOfflineTestService.ConflictoStockTestAsync(cancellationToken));
    }

    [HttpGet("pos-offline/idempotencia")]
    public async Task<IActionResult> RunPosIdempotencyTest(CancellationToken cancellationToken)
    {
        return Ok(await posOfflineTestService.IdempotenciaTestAsync(cancellationToken));
    }
}
