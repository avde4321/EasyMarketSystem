using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/saas/billing")]
[EnableRateLimiting(SecurityRateLimitPolicyNames.IntegracionMasiva)]
public sealed class SaasBillingController(ISaasBillingService billing) : ControllerBase
{
    [HttpGet("admin/consumption")]
    [Authorize(Roles = SecurityRoleNames.SuperAdministrador)]
    public Task<IActionResult> Consumption([FromQuery] SaasConsumoQuery query, CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await billing.ConsultarConsumoAsync(query, cancellationToken)));

    [HttpGet("admin/dashboard")]
    [Authorize(Roles = SecurityRoleNames.SuperAdministrador)]
    public Task<IActionResult> AdminDashboard(CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await billing.ObtenerDashboardAdministracionAsync(cancellationToken)));

    [HttpGet("admin/payments")]
    [Authorize(Roles = SecurityRoleNames.SuperAdministrador)]
    public Task<IActionResult> AdminPayments([FromQuery] SaasPagosAdminQuery query, CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await billing.ConsultarPagosAdministracionAsync(query, cancellationToken)));

    [HttpGet("portal")]
    public Task<IActionResult> Portal(CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await billing.ObtenerPortalAsync(cancellationToken)));

    [HttpPost("checkout")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public Task<IActionResult> Checkout(CheckoutSaasDto dto, CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await billing.PrepararCheckoutAsync(dto.SaasPlanId, cancellationToken)));

    [HttpPost("register-transfer")]
    [Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
    public Task<IActionResult> RegisterTransfer(RegistrarTransferenciaSaasDto dto, CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await billing.RegistrarTransferenciaAsync(dto, cancellationToken)));

    // Confirmación administrativa. Un webhook público requiere un adaptador de pasarela autenticado.
    [HttpPost("confirm-payment")]
    [Authorize(Roles = SecurityRoleNames.SuperAdministrador)]
    public Task<IActionResult> ConfirmPayment(ConfirmarPagoSaasDto dto, CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(dto.Aprobar
            ? await billing.AprobarPagoAsync(dto.PagoId, dto.EmpresaId, dto.Observaciones, cancellationToken)
            : await billing.RechazarPagoAsync(dto.PagoId, dto.EmpresaId, dto.Observaciones, cancellationToken)));

    private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> operation)
    {
        try { return await operation(); }
        catch (UnauthorizedAccessException) { return Problem(statusCode: 403, title: "Acceso no autorizado", detail: "No tienes permisos para realizar esta operación."); }
        catch (KeyNotFoundException) { return Problem(statusCode: 404, title: "Pago no encontrado"); }
        catch (SaasBillingException ex) { return Problem(statusCode: 409, title: "No se pudo procesar el pago", detail: ex.Message); }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex) { return Problem(statusCode: 400, title: "Revisa los filtros o datos del pago", detail: ex.Message); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        { return Problem(statusCode: 409, title: "El pago cambió durante la operación", detail: "Actualiza y vuelve a consultar el estado del pago."); }
    }
}
