using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
