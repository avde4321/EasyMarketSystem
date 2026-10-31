using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Route("api/saas")]
public sealed class SaasController(TestDeIaDbContext dbContext) : ControllerBase
{
    [Authorize]
    [HttpGet("subscription-status")]
    public async Task<IActionResult> GetSubscriptionStatus(
        [FromServices] TestDeIa.Application.Common.ITenantContextAccessor tenant,
        [FromServices] TestDeIa.Application.Modules.Saas.Ports.In.ISaaSQuotaValidationService service,
        CancellationToken cancellationToken)
    {
        if (!tenant.EmpresaId.HasValue)
            return Ok(new SubscriptionStatusResponse(false, "EMPRESA_REQUIRED"));
        try
        {
            await service.EnsureActiveSubscriptionAsync(tenant.EmpresaId.Value, cancellationToken);
            return Ok(new SubscriptionStatusResponse(true, null));
        }
        catch (TestDeIa.Application.Common.SubscriptionRequiredException)
        {
            return Ok(new SubscriptionStatusResponse(false, "SUBSCRIPTION_REQUIRED"));
        }
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken = default)
    {
        var plans = await dbContext.SaasPlanes
            .AsNoTracking()
            .Where(plan => plan.EsActivo)
            .OrderBy(plan => plan.Id)
            .Select(plan => new SaasPlanResponse(
                plan.Id,
                plan.Nombre,
                plan.LimiteFacturasMensuales,
                plan.LimiteUsuarios,
                plan.LimiteSucursales,
                plan.PermiteModuloSRI,
                plan.PermiteModuloKardexAvanzado,
                plan.PrecioMensual,
                plan.EsActivo))
            .ToArrayAsync(cancellationToken);

        return Ok(plans);
    }
}
