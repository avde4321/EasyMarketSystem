using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Integraciones.Ports.In;
using TestDeIa.Shared.Requests.Integraciones;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/whatsapp")]
public sealed class WhatsAppController(
    IWhatsAppService whatsAppService,
    ITenantContextAccessor tenantContextAccessor) : ControllerBase
{
    [HttpPost("enviar-comprobante")]
    [Authorize(Policy = SecurityPolicyNames.FacturacionMonitor)]
    public async Task<IActionResult> EnviarComprobante(
        [FromBody] EnviarWhatsAppDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = ResolveEmpresaId();
            return Ok(await whatsAppService.EnviarComprobanteWhatsAppAsync(request, empresaId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("recordatorio-cobro")]
    [Authorize(Policy = SecurityPolicyNames.ComprasCuentasPorPagar)]
    public async Task<IActionResult> EnviarRecordatorioCobro(
        [FromBody] EnviarRecordatorioPagoDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = ResolveEmpresaId();
            return Ok(await whatsAppService.EnviarRecordatorioPagoAsync(request, empresaId, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    private Guid ResolveEmpresaId()
    {
        return tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa para ejecutar la integracion de WhatsApp.");
    }
}
