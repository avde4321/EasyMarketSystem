using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using TestDeIa.Api.Reporting;

namespace TestDeIa.Api.Messaging;

public sealed class SmtpFacturaEmailSender
{
    private readonly SmtpEmailOptions options;

    public SmtpFacturaEmailSender(IOptions<SmtpEmailOptions> options)
    {
        this.options = options.Value;
    }

    public bool IsEnabled =>
        options.Enabled &&
        !string.IsNullOrWhiteSpace(options.Host) &&
        !string.IsNullOrWhiteSpace(options.FromAddress);

    public async Task SendFacturaAsync(
        FacturaEmailNotificationDocument document,
        byte[] ridePdfBytes,
        CancellationToken cancellationToken)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("El servicio SMTP no esta configurado o se encuentra deshabilitado.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(options.FromAddress, string.IsNullOrWhiteSpace(options.FromName) ? document.Ride.EmisorNombreComercial : options.FromName, Encoding.UTF8),
            Subject = $"Factura electronica {document.NumeroComprobante}",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = BuildHtmlBody(document)
        };

        message.To.Add(new MailAddress(document.DestinatarioEmail, document.DestinatarioNombre, Encoding.UTF8));

        var xmlBytes = Encoding.UTF8.GetBytes(document.XmlAutorizado);
        var xmlStream = new MemoryStream(xmlBytes);
        var pdfStream = new MemoryStream(ridePdfBytes);
        message.Attachments.Add(new Attachment(xmlStream, $"FACTURA-{document.NumeroComprobante}.xml", "application/xml"));
        message.Attachments.Add(new Attachment(pdfStream, $"RIDE-{document.NumeroComprobante}.pdf", "application/pdf"));

        using var smtpClient = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            smtpClient.Credentials = new NetworkCredential(options.Username, options.Password);
        }
        else
        {
            smtpClient.UseDefaultCredentials = true;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await smtpClient.SendMailAsync(message, cancellationToken);
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
<html lang="es">
<body style="margin:0;padding:24px;background:#eef6fb;font-family:Segoe UI,Arial,sans-serif;color:#1f3b57;">
  <div style="max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #d7e7f3;border-radius:18px;overflow:hidden;">
    <div style="padding:24px 28px;background:#f7fbff;border-bottom:1px solid #d7e7f3;">
      <div style="font-size:22px;font-weight:700;color:#124a7b;">{emisor}</div>
      <div style="margin-top:6px;font-size:14px;color:#64829c;">Factura electrónica autorizada</div>
    </div>
    <div style="padding:28px;">
      <p style="margin:0 0 18px 0;font-size:15px;">Hola {cliente},</p>
      <p style="margin:0 0 20px 0;font-size:15px;line-height:1.6;">
        Adjuntamos tu comprobante electrónico autorizado junto con su RIDE en PDF.
      </p>
      <div style="background:#f3fbf8;border:1px solid #c9e7dc;border-radius:14px;padding:18px 20px;">
        <div style="margin-bottom:10px;font-size:14px;"><strong>Comprobante:</strong> {comprobante}</div>
        <div style="margin-bottom:10px;font-size:14px;"><strong>Fecha:</strong> {fecha}</div>
        <div style="font-size:14px;"><strong>Importe total:</strong> ${total}</div>
      </div>
      <p style="margin:20px 0 0 0;font-size:13px;color:#6b8398;line-height:1.6;">
        Este correo incluye el XML autorizado por el SRI y el PDF del RIDE para tus respaldos.
      </p>
    </div>
  </div>
</body>
</html>
""";
    }
}
