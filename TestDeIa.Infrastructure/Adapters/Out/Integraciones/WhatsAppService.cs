using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Modules.Integraciones.Ports.In;
using TestDeIa.Domain.Modules.Integraciones.Enums;
using TestDeIa.Infrastructure.Options;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Integraciones;

namespace TestDeIa.Infrastructure.Adapters.Out.Integraciones;

public sealed class WhatsAppService(
    HttpClient httpClient,
    TestDeIaDbContext dbContext,
    IOptions<IntegracionesOptions> options,
    ILogger<WhatsAppService> logger) : IWhatsAppService
{
    private readonly WhatsAppCloudApiOptions whatsAppOptions = options.Value.WhatsApp;

    public async Task<WhatsAppEnvioResponseDto> EnviarComprobanteWhatsAppAsync(
        EnviarWhatsAppDto dto,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var log = BuildLog(empresaId, dto.NumeroTelefono, dto.TipoDoc, dto.DocumentoId ?? Guid.Empty);
        dbContext.WhatsAppNotificacionesLog.Add(log);

        try
        {
            var config = await GetConfigAsync(empresaId, cancellationToken);
            var phone = NormalizePhone(dto.NumeroTelefono);
            var payload = BuildTemplatePayload(
                phone,
                whatsAppOptions.ComprobanteTemplateName,
                [
                    dto.NombreCliente,
                    dto.TipoDoc,
                    dto.LinkPdfRide ?? "No disponible",
                    dto.LinkXml ?? "No disponible",
                    dto.MensajePersonalizado ?? "Gracias por su compra."
                ]);

            var providerId = await SendToMetaAsync(config, payload, cancellationToken);
            log.EstadoEnvio = EstadoEnvioWhatsApp.Enviado;
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "WhatsApp comprobante enviado. Empresa={EmpresaId}; Destino={Destino}; Tipo={TipoDocumento}; ProviderId={ProviderId}.",
                empresaId,
                phone,
                dto.TipoDoc,
                providerId);

            return new WhatsAppEnvioResponseDto
            {
                LogId = log.Id,
                Succeeded = true,
                Estado = log.EstadoEnvio.ToString(),
                ProviderMessageId = providerId,
                Mensaje = "Comprobante enviado por WhatsApp."
            };
        }
        catch (Exception exception)
        {
            log.EstadoEnvio = EstadoEnvioWhatsApp.Fallido;
            log.MensajeError = exception.Message;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogWarning(exception, "No se pudo enviar comprobante por WhatsApp.");

            return new WhatsAppEnvioResponseDto
            {
                LogId = log.Id,
                Succeeded = false,
                Estado = log.EstadoEnvio.ToString(),
                Mensaje = exception.Message
            };
        }
    }

    public async Task<WhatsAppEnvioResponseDto> EnviarRecordatorioPagoAsync(
        EnviarRecordatorioPagoDto dto,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var log = BuildLog(empresaId, dto.NumeroTelefono, "RecordatorioPago", dto.DocumentoId ?? Guid.Empty);
        dbContext.WhatsAppNotificacionesLog.Add(log);

        try
        {
            var config = await GetConfigAsync(empresaId, cancellationToken);
            var phone = NormalizePhone(dto.NumeroTelefono);
            var payload = BuildTemplatePayload(
                phone,
                whatsAppOptions.RecordatorioPagoTemplateName,
                [
                    dto.NombreCliente,
                    dto.SaldoPendiente.ToString("0.00"),
                    dto.LinkPago ?? "No disponible",
                    dto.MensajePersonalizado ?? "Tiene un saldo pendiente por cancelar."
                ]);

            var providerId = await SendToMetaAsync(config, payload, cancellationToken);
            log.EstadoEnvio = EstadoEnvioWhatsApp.Enviado;
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "WhatsApp recordatorio de cobro enviado. Empresa={EmpresaId}; Destino={Destino}; ProviderId={ProviderId}.",
                empresaId,
                phone,
                providerId);

            return new WhatsAppEnvioResponseDto
            {
                LogId = log.Id,
                Succeeded = true,
                Estado = log.EstadoEnvio.ToString(),
                ProviderMessageId = providerId,
                Mensaje = "Recordatorio enviado por WhatsApp."
            };
        }
        catch (Exception exception)
        {
            log.EstadoEnvio = EstadoEnvioWhatsApp.Fallido;
            log.MensajeError = exception.Message;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogWarning(exception, "No se pudo enviar recordatorio de cobro por WhatsApp.");

            return new WhatsAppEnvioResponseDto
            {
                LogId = log.Id,
                Succeeded = false,
                Estado = log.EstadoEnvio.ToString(),
                Mensaje = exception.Message
            };
        }
    }

    private async Task<string?> SendToMetaAsync(
        EmpresaConfiguracionServiciosEntity config,
        object payload,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.WhatsAppApiToken) ||
            string.IsNullOrWhiteSpace(config.WhatsAppPhoneId))
        {
            throw new InvalidOperationException("La empresa no tiene configurada la API de WhatsApp Business.");
        }

        var url = $"{whatsAppOptions.GraphBaseUrl.TrimEnd('/')}/{whatsAppOptions.ApiVersion}/{config.WhatsAppPhoneId}/messages";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.WhatsAppApiToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"WhatsApp API rechazo el envio: {(int)response.StatusCode} - {content}");
        }

        using var document = JsonDocument.Parse(content);
        return document.RootElement
            .GetProperty("messages")
            .EnumerateArray()
            .FirstOrDefault()
            .TryGetProperty("id", out var id)
            ? id.GetString()
            : null;
    }

    private async Task<EmpresaConfiguracionServiciosEntity> GetConfigAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        return await dbContext.EmpresaConfiguracionServicios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("La empresa no tiene configurados servicios de integracion.");
    }

    private object BuildTemplatePayload(string phone, string templateName, IReadOnlyCollection<string> parameters)
    {
        return new
        {
            messaging_product = "whatsapp",
            to = phone,
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = whatsAppOptions.LanguageCode },
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters.Select(value => new { type = "text", text = value }).ToArray()
                    }
                }
            }
        };
    }

    private string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal))
        {
            digits = digits[2..];
        }

        if (digits.StartsWith("0", StringComparison.Ordinal))
        {
            digits = $"{whatsAppOptions.DefaultCountryCode}{digits[1..]}";
        }

        if (!digits.StartsWith(whatsAppOptions.DefaultCountryCode, StringComparison.Ordinal) && digits.Length <= 10)
        {
            digits = $"{whatsAppOptions.DefaultCountryCode}{digits}";
        }

        return digits;
    }

    private static WhatsAppNotificacionLogEntity BuildLog(Guid empresaId, string numero, string tipoDocumento, Guid documentoId)
    {
        return new WhatsAppNotificacionLogEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            NumeroDestino = numero,
            TipoDocumento = tipoDocumento,
            DocumentoId = documentoId,
            EstadoEnvio = EstadoEnvioWhatsApp.Pendiente,
            FechaEnvio = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
