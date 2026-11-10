using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Authentication;
using TestDeIa.Api.Messaging;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Authorize(Policy = SecurityPolicyNames.EmpresaConfigurar)]
[Route("api/empresa/smtp")]
public sealed class EmpresaSmtpController(
    IEmpresaSmtpConfiguracionService configuracionService,
    SmtpFacturaEmailSender emailSender,
    ILogger<EmpresaSmtpController> logger,
    TestDeIa.Application.Common.ITenantContextAccessor tenant) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken cancellationToken) =>
        Ok(await configuracionService.ObtenerAsync(cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Guardar(
        [FromBody] EmpresaSmtpConfiguracionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await configuracionService.GuardarAsync(request, cancellationToken));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("probar")]
    public async Task<IActionResult> Probar(
        [FromBody] EmpresaSmtpPruebaRequest request,
        CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId;
        try
        {
            if (empresaId is null || empresaId == Guid.Empty)
                return BadRequest(new { message = "Seleccione una empresa antes de probar el correo." });

            await emailSender.SendTestAsync(empresaId.Value, request.Destinatario, cancellationToken);
            return Ok(new { message = "Correo de prueba enviado correctamente." });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception,
                "Falló la prueba SMTP. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}", empresaId, tenant.UserId);
            return Problem(statusCode: StatusCodes.Status502BadGateway,
                title: "No se pudo enviar el correo de prueba",
                detail: BuildSafeSmtpDiagnostic(exception));
        }
    }

    private static string BuildSafeSmtpDiagnostic(Exception exception)
    {
        var chain = EnumerateExceptions(exception).ToArray();
        if (chain.Any(current => current is AuthenticationException))
            return "Falló la negociación SSL/TLS. Para Gmail usa smtp.gmail.com, puerto 587 y SSL/TLS activado.";

        var smtpException = chain.OfType<SmtpException>().FirstOrDefault();
        if (smtpException is not null)
        {
            var serverMessage = smtpException.Message;
            if (serverMessage.Contains("5.7.8", StringComparison.OrdinalIgnoreCase) ||
                serverMessage.Contains("Username and Password not accepted", StringComparison.OrdinalIgnoreCase) ||
                serverMessage.Contains("authentication", StringComparison.OrdinalIgnoreCase))
                return "Gmail rechazó la autenticación. Confirma que el usuario sea el correo completo y que la contraseña sea una contraseña de aplicación de Google, no la contraseña normal. Se guardan las claves quitando espacios.";

            if (serverMessage.Contains("5.7.0", StringComparison.OrdinalIgnoreCase) ||
                serverMessage.Contains("Must issue a STARTTLS command", StringComparison.OrdinalIgnoreCase))
                return "El servidor exige una conexión segura. Configura el puerto 587 con SSL/TLS activado.";

            if (serverMessage.Contains("protocol violation", StringComparison.OrdinalIgnoreCase) ||
                serverMessage.Contains("unexpected response", StringComparison.OrdinalIgnoreCase))
                return "Gmail requiere smtp.gmail.com, puerto 587 y SSL/TLS activado (STARTTLS); el puerto 465 no es compatible con el emisor actual. Si esos valores ya están correctos, confirma que el usuario SMTP sea el correo completo, que la clave sea una contraseña de aplicación y que el remitente sea esa misma cuenta o un alias autorizado. Revisa el log del API para ver en qué fase falló; nunca se registran las credenciales.";

            var safeMessage = serverMessage.Length > 360 ? serverMessage[..360] : serverMessage;
            return $"El servidor SMTP respondió con estado {smtpException.StatusCode}: {safeMessage}";
        }

        if (chain.Any(current => current is System.Net.Sockets.SocketException))
            return "No se pudo establecer conexión con el servidor SMTP. Verifica host y puerto, y que la red permita conexiones salientes al puerto 587.";

        return "No fue posible completar la conexión SMTP. Revisa host, puerto, SSL/TLS y credenciales; el detalle técnico quedó registrado en el log del API.";
    }

    private static IEnumerable<Exception> EnumerateExceptions(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
            yield return current;
    }
}
