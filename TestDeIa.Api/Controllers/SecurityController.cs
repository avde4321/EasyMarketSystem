using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Security.Ports.In;
using TestDeIa.Shared.Requests.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class SecurityController : ControllerBase
{
    private readonly ILoginUseCase loginUseCase;
    private readonly ISecurityManagementUseCase securityManagementUseCase;

    public SecurityController(
        ILoginUseCase loginUseCase,
        ISecurityManagementUseCase securityManagementUseCase)
    {
        this.loginUseCase = loginUseCase;
        this.securityManagementUseCase = securityManagementUseCase;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await loginUseCase.LoginAsync(request, cancellationToken);

        if (!response.Succeeded)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await securityManagementUseCase.GetUsersAsync(cancellationToken));
    }

    [HttpGet("roles")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        return Ok(await securityManagementUseCase.GetRolesAsync(cancellationToken));
    }

    [HttpPost("users")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateUser([FromBody] SecurityUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await securityManagementUseCase.CreateUserAsync(request, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("users/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] SecurityUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await securityManagementUseCase.UpdateUserAsync(id, request, cancellationToken);
            return user is null ? NotFound() : Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
