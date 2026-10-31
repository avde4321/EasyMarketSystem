using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
using TestDeIa.Shared.Responses.Integraciones;

namespace TestDeIa.Infrastructure.Adapters.Out.Integraciones;

public sealed class PayPhonePagoService(
    HttpClient httpClient,
    TestDeIaDbContext dbContext,
    IOptions<IntegracionesOptions> options,
    ILogger<PayPhonePagoService> logger) : IPasarelaPagoService
{
    private readonly PayPhoneOptions payPhoneOptions = options.Value.PayPhone;

    public async Task<RespuestaPagoDigitalDto> GenerarEnlaceCobroAsync(
        GenerarLinkPagoDto dto,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var config = await GetConfigAsync(empresaId, cancellationToken);
        if (config.PasarelaPagoActiva == PasarelaPagoActiva.Ninguna)
        {
            throw new InvalidOperationException("La empresa no tiene una pasarela de pago activa.");
        }

        var factura = dto.FacturaId.HasValue
            ? await dbContext.Facturas
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Id == dto.FacturaId.Value, cancellationToken)
            : null;

        var clienteId = factura?.ClienteId ?? await ResolveDefaultClienteIdAsync(empresaId, cancellationToken);
        var transactionId = $"EM-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..42];
        var link = await TryCreateProviderPaymentLinkAsync(config, dto, transactionId, cancellationToken);
        var qr = BuildQrPlaceholder(link);

        var entity = new TransaccionPagoDigitalEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            FacturaId = dto.FacturaId,
            ClienteId = clienteId,
            Monto = Math.Round(dto.Monto, 4, MidpointRounding.AwayFromZero),
            Pasarela = config.PasarelaPagoActiva.ToString(),
            TransactionIdPasarela = transactionId,
            EstadoPago = EstadoPagoDigital.Pendiente,
            LinkPagoUrl = link,
            QrCodeBase64 = qr,
            FechaCreacion = DateTimeOffset.UtcNow
        };

        dbContext.TransaccionesPagosDigitales.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Link de pago generado. Empresa={EmpresaId}; Factura={FacturaId}; Transaccion={TransactionId}; Pasarela={Pasarela}; Monto={Monto}.",
            empresaId,
            dto.FacturaId,
            transactionId,
            entity.Pasarela,
            entity.Monto);

        return new RespuestaPagoDigitalDto
        {
            TransactionId = entity.TransactionIdPasarela,
            LinkPagoUrl = entity.LinkPagoUrl,
            QrCodeBase64 = entity.QrCodeBase64,
            Estado = entity.EstadoPago.ToString()
        };
    }

    public async Task<RespuestaPagoDigitalDto> ProcesarWebhookPagoAsync(
        PayPhoneWebhookDto dto,
        string? authorizationToken,
        CancellationToken cancellationToken = default)
    {
        var config = await GetConfigAsync(dto.EmpresaId, cancellationToken);
        ValidateWebhookToken(config, authorizationToken);

        var transaction = await dbContext.TransaccionesPagosDigitales
            .IgnoreQueryFilters()
            .Include(current => current.Factura)
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == dto.EmpresaId &&
                current.TransactionIdPasarela == dto.TransactionId,
                cancellationToken)
            ?? throw new InvalidOperationException("La transaccion de pago digital no existe.");

        var normalizedState = NormalizePaymentState(dto.Estado);
        transaction.EstadoPago = normalizedState;
        transaction.UpdatedAt = DateTimeOffset.UtcNow;

        if (normalizedState == EstadoPagoDigital.Aprobado)
        {
            transaction.FechaAprobacion = DateTimeOffset.UtcNow;
            MarkFacturaAsPaidEvidence(transaction.Factura, transaction);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Webhook de pago procesado. Empresa={EmpresaId}; Transaccion={TransactionId}; Estado={Estado}; Monto={Monto}.",
            dto.EmpresaId,
            dto.TransactionId,
            normalizedState,
            dto.Monto);

        return new RespuestaPagoDigitalDto
        {
            TransactionId = transaction.TransactionIdPasarela,
            LinkPagoUrl = transaction.LinkPagoUrl,
            QrCodeBase64 = transaction.QrCodeBase64,
            Estado = transaction.EstadoPago.ToString()
        };
    }

    public async Task<RespuestaPagoDigitalDto> ConsultarEstadoAsync(
        string transactionId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.TransaccionesPagosDigitales
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.TransactionIdPasarela == transactionId,
                cancellationToken)
            ?? throw new InvalidOperationException("La transaccion de pago digital no existe.");

        return new RespuestaPagoDigitalDto
        {
            TransactionId = transaction.TransactionIdPasarela,
            LinkPagoUrl = transaction.LinkPagoUrl,
            QrCodeBase64 = transaction.QrCodeBase64,
            Estado = transaction.EstadoPago.ToString()
        };
    }

    private async Task<string> TryCreateProviderPaymentLinkAsync(
        EmpresaConfiguracionServiciosEntity config,
        GenerarLinkPagoDto dto,
        string transactionId,
        CancellationToken cancellationToken)
    {
        if (config.PasarelaPagoActiva != PasarelaPagoActiva.PayPhone)
        {
            return BuildLocalPaymentLink(config.PasarelaPagoActiva, transactionId);
        }

        if (string.IsNullOrWhiteSpace(config.PayPhoneToken))
        {
            throw new InvalidOperationException("La empresa no tiene configurado el token de PayPhone.");
        }

        var baseUrl = config.ModopagosAmbiente == ModoPagosAmbiente.Produccion
            ? payPhoneOptions.ProduccionBaseUrl
            : payPhoneOptions.PruebasBaseUrl;

        var payload = new
        {
            amount = ToCents(dto.Monto),
            amountWithTax = ToCents(dto.Monto),
            amountWithoutTax = 0,
            tax = 0,
            service = 0,
            tip = 0,
            clientTransactionId = transactionId,
            reference = dto.Concepto,
            email = dto.EmailCliente,
            phoneNumber = dto.TelefonoCliente
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/api/Links")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.PayPhoneToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "PayPhone no genero link real. Status={Status}; Payload={Payload}. Se usara link local de contingencia.",
                response.StatusCode,
                content);

            return BuildLocalPaymentLink(PasarelaPagoActiva.PayPhone, transactionId);
        }

        using var document = JsonDocument.Parse(content);
        if (TryGetString(document.RootElement, "payWithPayPhone", out var payPhoneLink) ||
            TryGetString(document.RootElement, "paymentUrl", out payPhoneLink) ||
            TryGetString(document.RootElement, "url", out payPhoneLink))
        {
            return payPhoneLink;
        }

        return BuildLocalPaymentLink(PasarelaPagoActiva.PayPhone, transactionId);
    }

    private async Task<EmpresaConfiguracionServiciosEntity> GetConfigAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        return await dbContext.EmpresaConfiguracionServicios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("La empresa no tiene configuracion de servicios/pagos digitales.");
    }

    private async Task<Guid> ResolveDefaultClienteIdAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var clienteId = await dbContext.Clientes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId && current.IsActive)
            .Select(current => current.PersonaId)
            .FirstOrDefaultAsync(cancellationToken);

        return clienteId == Guid.Empty
            ? throw new InvalidOperationException("No existe un cliente activo para asociar la transaccion de pago.")
            : clienteId;
    }

    private void ValidateWebhookToken(EmpresaConfiguracionServiciosEntity config, string? authorizationToken)
    {
        var expected = config.PayPhoneToken;
        if (string.IsNullOrWhiteSpace(expected))
        {
            throw new InvalidOperationException("La empresa no tiene token de validacion para webhooks.");
        }

        var normalized = authorizationToken?.Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        if (!string.Equals(normalized, expected, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Token de webhook invalido.");
        }
    }

    private static EstadoPagoDigital NormalizePaymentState(string state)
    {
        return state.Trim().ToUpperInvariant() switch
        {
            "APPROVED" or "APROBADO" or "SUCCESS" or "SUCCEEDED" or "PAID" => EstadoPagoDigital.Aprobado,
            "REJECTED" or "RECHAZADO" or "FAILED" or "ERROR" => EstadoPagoDigital.Rechazado,
            "CANCELLED" or "CANCELED" or "CANCELADO" => EstadoPagoDigital.Cancelado,
            _ => EstadoPagoDigital.Pendiente
        };
    }

    private static void MarkFacturaAsPaidEvidence(FacturaEntity? factura, TransaccionPagoDigitalEntity transaction)
    {
        if (factura is null)
        {
            return;
        }

        var note = $"Pago digital aprobado {transaction.Pasarela} / {transaction.TransactionIdPasarela} el {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.";
        factura.MensajeEstado = string.IsNullOrWhiteSpace(factura.MensajeEstado)
            ? note
            : $"{factura.MensajeEstado} | {note}";
        factura.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static int ToCents(decimal amount)
    {
        return (int)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
    }

    private static string BuildLocalPaymentLink(PasarelaPagoActiva pasarela, string transactionId)
    {
        return $"https://payments.easymarket.local/{pasarela.ToString().ToLowerInvariant()}/{transactionId}";
    }

    private static string BuildQrPlaceholder(string link)
    {
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg" width="360" height="360" viewBox="0 0 360 360">
              <rect width="360" height="360" rx="28" fill="#f5fbff"/>
              <rect x="34" y="34" width="292" height="292" rx="18" fill="#ffffff" stroke="#b9d8eb" stroke-width="3"/>
              <text x="180" y="160" text-anchor="middle" font-family="Arial" font-size="28" font-weight="700" fill="#0d4775">QR PAGO</text>
              <text x="180" y="198" text-anchor="middle" font-family="Arial" font-size="15" fill="#477095">Escanea o abre el enlace</text>
              <text x="180" y="232" text-anchor="middle" font-family="Arial" font-size="10" fill="#254c6d">{{System.Security.SecurityElement.Escape(link)}}</text>
            </svg>
            """;
        return $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svg))}";
    }
}
