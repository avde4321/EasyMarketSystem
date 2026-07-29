using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestDeIa.Application.Modules.Dashboard.Ports.In;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardUseCase dashboardUseCase;

    public DashboardController(IDashboardUseCase dashboardUseCase)
    {
        this.dashboardUseCase = dashboardUseCase;
    }

    [HttpGet("overview")]
    [Authorize(Policy = SecurityPolicyNames.DashboardView)]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        return Ok(await dashboardUseCase.GetOverviewAsync(cancellationToken));
    }

    [HttpGet("cajero")]
    [Authorize(Roles = $"{SecurityRoleNames.Cajero},{SecurityRoleNames.AsesorComercial}")]
    public async Task<IActionResult> GetCajeroOverview(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        return Ok(await dashboardUseCase.GetCajeroOverviewAsync(userId, cancellationToken));
    }

    [HttpGet("bodeguero")]
    [Authorize(Roles = SecurityRoleNames.Bodeguero)]
    public async Task<IActionResult> GetBodegueroOverview(CancellationToken cancellationToken)
    {
        return Ok(await dashboardUseCase.GetBodegueroOverviewAsync(cancellationToken));
    }
}
