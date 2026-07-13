using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Security.Ports.In;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Security;

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
        var response = await loginUseCase.LoginAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (!response.Succeeded)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpGet("users")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
    public async Task<IActionResult> GetUsers([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        return Ok(await securityManagementUseCase.GetUsersPagedAsync(term, skip, take, cancellationToken));
    }

    [HttpGet("roles")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        return Ok(await securityManagementUseCase.GetRolesAsync(cancellationToken));
    }

    [HttpGet("auditoria")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
    public async Task<IActionResult> GetAuditoria([FromQuery] string? term, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 25);
        skip = Math.Max(0, skip);
        return Ok(await securityManagementUseCase.GetAuditLogsPagedAsync(term, skip, take, cancellationToken));
    }

    [HttpPost("users")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
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
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
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

    [HttpPost("users/{id:guid}/reset-password")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await securityManagementUseCase.ResetPasswordAsync(
                id,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return user is null ? NotFound() : Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("usuarios/{id:guid}/perfil")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
    public async Task<IActionResult> UpdatePerfil(Guid id, [FromBody] UpdateUserPerfilRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await securityManagementUseCase.UpdatePerfilAsync(
                id,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return user is null ? NotFound() : Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("usuarios/{id:guid}/estado")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
    public async Task<IActionResult> UpdateEstado(Guid id, [FromBody] UpdateUserEstadoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await securityManagementUseCase.UpdateEstadoAsync(
                id,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return user is null ? NotFound() : Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("users/{id:guid}/unlock")]
    [Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
    public async Task<IActionResult> UnlockUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await securityManagementUseCase.UnlockUserAsync(
                id,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return user is null ? NotFound() : Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
