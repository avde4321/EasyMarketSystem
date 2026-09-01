using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Integraciones.Ports.In;
using TestDeIa.Infrastructure.Options;
using TestDeIa.Shared.Requests.Integraciones;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Route("api/pagos-digitales")]
public sealed class PagosDigitalesController(
    IPasarelaPagoService pasarelaPagoService,
    ITenantContextAccessor tenantContextAccessor,
    IOptions<IntegracionesOptions> options) : ControllerBase
{
    [HttpPost("generar-link")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> GenerarLink(
        [FromBody] GenerarLinkPagoDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = tenantContextAccessor.EmpresaId
                ?? throw new InvalidOperationException("No existe una empresa activa para generar el link de pago.");

            return Ok(await pasarelaPagoService.GenerarEnlaceCobroAsync(request, empresaId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("{transactionId}/estado")]
    [Authorize(Policy = SecurityPolicyNames.PosFacturar)]
    public async Task<IActionResult> ConsultarEstado(
        string transactionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = tenantContextAccessor.EmpresaId
                ?? throw new InvalidOperationException("No existe una empresa activa para consultar el pago.");

            return Ok(await pasarelaPagoService.ConsultarEstadoAsync(transactionId, empresaId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("webhook/payphone")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcesarWebhookPayPhone(
        [FromBody] PayPhoneWebhookDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = Request.Headers.Authorization.FirstOrDefault()
                ?? Request.Headers[options.Value.PayPhone.WebhookTokenHeaderName].FirstOrDefault();

            var result = await pasarelaPagoService.ProcesarWebhookPagoAsync(request, token, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Token de webhook invalido." });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

}
