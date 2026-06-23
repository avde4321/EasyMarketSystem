using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Modules.Security.Ports.In;
using TestDeIa.Shared.Requests.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SecurityController : ControllerBase
{
    private readonly ILoginUseCase loginUseCase;

    public SecurityController(ILoginUseCase loginUseCase)
    {
        this.loginUseCase = loginUseCase;
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
}
