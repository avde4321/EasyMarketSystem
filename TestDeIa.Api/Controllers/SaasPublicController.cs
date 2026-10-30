using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Route("api/v1/saas/public")]
public sealed class SaasPublicController(ITenantOnboardingService onboarding) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting(SecurityRateLimitPolicyNames.PublicRegistration)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(PublicTenantRegisterDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await onboarding.RegistrarPublicoAsync(dto, cancellationToken)); }
        catch (PublicTenantRegistrationException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Registro no disponible", detail: ex.Message,
                extensions: new Dictionary<string, object?> { ["code"] = "REGISTRATION_CONFLICT" });
        }
    }
}
