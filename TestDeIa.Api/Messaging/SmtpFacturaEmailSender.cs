using System.Net;
using System.Net.Mail;
using System.Text;
using TestDeIa.Api.Reporting;
using TestDeIa.Application.Modules.Empresa.Ports.In;

namespace TestDeIa.Api.Messaging;

public sealed class SmtpFacturaEmailSender(
    IEmpresaSmtpCredentialProvider credentialProvider,
    ILogger<SmtpFacturaEmailSender> logger)
{
    public async Task SendFacturaAsync(
        FacturaEmailNotificationDocument document,
        byte[] ridePdfBytes,
        CancellationToken cancellationToken)
    {
        var credentials = await GetCredentialsAsync(document.EmpresaId, requireEnabled: true, cancellationToken);
        using var message = new MailMessage
        {
            From = new MailAddress(credentials.CorreoRemitente, credentials.NombreRemitente, Encoding.UTF8),
            Subject = $"Factura electrónica {document.NumeroComprobante}",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = BuildHtmlBody(document)
        };

        message.To.Add(new MailAddress(document.DestinatarioEmail, document.DestinatarioNombre, Encoding.UTF8));
        message.Attachments.Add(new Attachment(new MemoryStream(Encoding.UTF8.GetBytes(document.XmlAutorizado)),
            $"FACTURA-{document.NumeroComprobante}.xml", "application/xml"));
        message.Attachments.Add(new Attachment(new MemoryStream(ridePdfBytes),
            $"RIDE-{document.NumeroComprobante}.pdf", "application/pdf"));
        await SendAsync(credentials, message, cancellationToken);
    }

    public async Task SendTestAsync(Guid empresaId, string destinatario, CancellationToken cancellationToken)
    {
        var credentials = await GetCredentialsAsync(empresaId, requireEnabled: false, cancellationToken);
        using var message = new MailMessage
        {
            From = new MailAddress(credentials.CorreoRemitente, credentials.NombreRemitente, Encoding.UTF8),
            Subject = "Prueba de configuración SMTP - EasyMarket Systems",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = "<p>La configuración SMTP de su empresa está operativa.</p>"
        };
        message.To.Add(new MailAddress(destinatario));
        await SendAsync(credentials, message, cancellationToken);
    }

    private async Task<EmpresaSmtpCredentials> GetCredentialsAsync(Guid empresaId, bool requireEnabled, CancellationToken cancellationToken)
    {
        var credentials = await credentialProvider.ObtenerCredencialesAsync(empresaId, cancellationToken);
        if (credentials is null || (requireEnabled && !credentials.Habilitado))
            throw new InvalidOperationException(requireEnabled
                ? "La empresa no tiene habilitado su servidor SMTP."
                : "Guarda primero la configuración SMTP de esta empresa.");
        if (credentials.RequiereAutenticacion &&
            (string.IsNullOrWhiteSpace(credentials.Usuario) || string.IsNullOrEmpty(credentials.Contrasena)))
            throw new InvalidOperationException("La configuración SMTP requiere usuario y contraseña.");
        return credentials;
    }

    private async Task SendAsync(EmpresaSmtpCredentials credentials, MailMessage message, CancellationToken cancellationToken)
    {
        var isGmail = credentials.Host.Equals("smtp.gmail.com", StringComparison.OrdinalIgnoreCase)
            || credentials.Host.Equals("smtp.googlemail.com", StringComparison.OrdinalIgnoreCase);
        if (credentials.Puerto == 465)
            throw new InvalidOperationException(
                "El emisor SMTP actual de EasyMarket negocia STARTTLS y no admite SSL implícito del puerto 465. Configura el puerto 587 con SSL/TLS activado.");
        if (isGmail && (credentials.Puerto != 587 || !credentials.UsarSsl))
            throw new InvalidOperationException(
                "Para Gmail configura smtp.gmail.com, puerto 587 y SSL/TLS activado (STARTTLS). Usa una contraseña de aplicación de Google.");

        using var smtpClient = new SmtpClient(credentials.Host, credentials.Puerto)
        {
            EnableSsl = credentials.UsarSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (credentials.RequiereAutenticacion)
            smtpClient.Credentials = new NetworkCredential(credentials.Usuario, credentials.Contrasena);

        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (SmtpException exception)
        {
            logger.LogError(exception,
                "Falló el envío SMTP. EmpresaId: {EmpresaId}, Host: {Host}, Puerto: {Puerto}, SSL/TLS: {UsarSsl}, Autenticación: {RequiereAutenticacion}, DominioRemitente: {DominioRemitente}, DominioDestinatario: {DominioDestinatario}",
                credentials.EmpresaId,
                credentials.Host,
                credentials.Puerto,
                credentials.UsarSsl,
                credentials.RequiereAutenticacion,
                GetDomain(message.From?.Address),
                GetDomain(message.To.FirstOrDefault()?.Address));
            throw;
        }
    }

    private static string? GetDomain(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        var separator = address.LastIndexOf('@');
        return separator >= 0 && separator < address.Length - 1 ? address[(separator + 1)..] : null;
    }

    private static string BuildHtmlBody(FacturaEmailNotificationDocument document)
    {
        var total = document.Total.ToString("0.00");
        var fecha = document.FechaEmision.LocalDateTime.ToString("dd/MM/yyyy HH:mm");
        var emisor = WebUtility.HtmlEncode(document.Ride.EmisorNombreComercial);
        var cliente = WebUtility.HtmlEncode(document.DestinatarioNombre);
        var comprobante = WebUtility.HtmlEncode(document.NumeroComprobante);

        return $"""
<!DOCTYPE html>
<html lang="es"><body style="margin:0;padding:24px;background:#eef6fb;font-family:Segoe UI,Arial,sans-serif;color:#1f3b57;">
  <div style="max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #d7e7f3;border-radius:18px;overflow:hidden;">
    <div style="padding:24px 28px;background:#f7fbff;border-bottom:1px solid #d7e7f3;">
      <div style="font-size:22px;font-weight:700;color:#124a7b;">{emisor}</div>
      <div style="margin-top:6px;font-size:14px;color:#64829c;">Factura electrónica autorizada</div>
    </div>
    <div style="padding:28px;">
      <p style="margin:0 0 18px 0;font-size:15px;">Hola {cliente},</p>
      <p style="margin:0 0 20px 0;font-size:15px;line-height:1.6;">Adjuntamos tu comprobante electrónico autorizado junto con su RIDE en PDF.</p>
      <div style="background:#f3fbf8;border:1px solid #c9e7dc;border-radius:14px;padding:18px 20px;">
        <div style="margin-bottom:10px;font-size:14px;"><strong>Comprobante:</strong> {comprobante}</div>
        <div style="margin-bottom:10px;font-size:14px;"><strong>Fecha:</strong> {fecha}</div>
        <div style="font-size:14px;"><strong>Importe total:</strong> ${total}</div>
      </div>
      <p style="margin:20px 0 0 0;font-size:13px;color:#6b8398;line-height:1.6;">Este correo incluye el XML autorizado y el PDF del RIDE para tus respaldos.</p>
    </div>
  </div>
</body></html>
""";
    }
}
