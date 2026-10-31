using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.OfflinePos.Ports.In;
using TestDeIa.Application.Modules.Tesoreria.Ports.In;
using TestDeIa.Application.Modules.XmlSri.Ports.In;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
[Route("api/diagnostics")]
public sealed class DiagnosticsController(
    IXmlComprasTestService xmlComprasTestService,
    IPosOfflineTestService posOfflineTestService,
    IConciliacionTestService conciliacionTestService,
    IBancoParserTestService bancoParserTestService) : ControllerBase
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

    [HttpGet("tesoreria/conciliacion")]
    public async Task<IActionResult> RunConciliacionSuite(CancellationToken cancellationToken)
    {
        return Ok(await conciliacionTestService.EjecutarSuiteAsync(cancellationToken));
    }

    [HttpGet("tesoreria/conciliacion/exacto")]
    public async Task<IActionResult> RunConciliacionExactMatchTest(CancellationToken cancellationToken)
    {
        return Ok(await conciliacionTestService.ValidarMatchingExactoMontoYReferenciaAsync(cancellationToken));
    }

    [HttpGet("tesoreria/conciliacion/rango-fechas")]
    public async Task<IActionResult> RunConciliacionDateRangeTest(CancellationToken cancellationToken)
    {
        return Ok(await conciliacionTestService.ValidarMatchingPorRangoDeFechasAsync(cancellationToken));
    }

    [HttpGet("tesoreria/conciliacion/comisiones")]
    public async Task<IActionResult> RunConciliacionBankFeesTest(CancellationToken cancellationToken)
    {
        return Ok(await conciliacionTestService.ValidarManejoComisionesYBancosAsync(cancellationToken));
    }

    [HttpGet("tesoreria/conciliacion/duplicados")]
    public async Task<IActionResult> RunConciliacionDuplicatesTest(CancellationToken cancellationToken)
    {
        return Ok(await conciliacionTestService.EvitarDuplicadosExtractoTestAsync(cancellationToken));
    }

    [HttpGet("tesoreria/parser")]
    public async Task<IActionResult> RunBancoParserSuite(CancellationToken cancellationToken)
    {
        return Ok(await bancoParserTestService.EjecutarSuiteAsync(cancellationToken));
    }

    [HttpGet("tesoreria/parser/multibanco")]
    public async Task<IActionResult> RunBancoParserMultiBankTest(CancellationToken cancellationToken)
    {
        return Ok(await bancoParserTestService.ValidarFormatosMultiBancoAsync(cancellationToken));
    }

    [HttpGet("tesoreria/parser/archivo-grande")]
    public async Task<IActionResult> RunBancoParserLargeFileTest(CancellationToken cancellationToken)
    {
        return Ok(await bancoParserTestService.ValidarParsingArchivoGrandeAsync(cancellationToken));
    }
}
