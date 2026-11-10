using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Application.Common;

namespace TestDeIa.Api.Messaging;

public sealed class FacturacionEmailNotifier : BackgroundService
{
    private const string EmailSentPrefix = "EMAIL_SENT|";
    private const string EmailFailedPrefix = "EMAIL_FAILED|";
    private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(20);

    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<FacturacionEmailNotifier> logger;

    public FacturacionEmailNotifier(
        IServiceScopeFactory scopeFactory,
        ILogger<FacturacionEmailNotifier> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ITenantContextAccessor? tenantContextAccessor = null;
            try
            {
                using var scope = scopeFactory.CreateScope();
                tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
                var dbContext = scope.ServiceProvider.GetRequiredService<TestDeIaDbContext>();
                var queryService = scope.ServiceProvider.GetRequiredService<Reporting.FacturaDocumentQueryService>();
                var renderer = scope.ServiceProvider.GetRequiredService<Reporting.FacturaRideRdlcRenderer>();
                var sender = scope.ServiceProvider.GetRequiredService<SmtpFacturaEmailSender>();

                tenantContextAccessor.IsSystemContext = true;
                var candidateIds = await dbContext.Facturas
                    .AsNoTracking()
                    .Join(dbContext.EmpresaSmtpConfiguraciones.AsNoTracking(),
                        factura => factura.EmpresaId,
                        smtp => smtp.EmpresaId,
                        (factura, smtp) => new { Factura = factura, Smtp = smtp })
                    .Where(current =>
                        current.Smtp.Habilitado &&
                        current.Factura.Estado == Domain.Modules.Facturacion.Entities.FacturaEstado.AUTORIZADO &&
                        current.Factura.XmlFirmado != null &&
                        current.Factura.ClienteEmail != null)
                    .OrderBy(current => current.Factura.FechaAutorizacion ?? current.Factura.UpdatedAt)
                    .Select(current => new { current.Factura.Id, current.Factura.EmpresaId })
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var candidate in candidateIds)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    tenantContextAccessor.IsSystemContext = false;
                    tenantContextAccessor.EmpresaId = candidate.EmpresaId;

                    if (!await ShouldProcessAsync(dbContext, candidate.Id, stoppingToken))
                    {
                        continue;
                    }

                    try
                    {
                        var document = await queryService.GetFacturaEmailNotificationDocumentAsync(candidate.EmpresaId, candidate.Id, stoppingToken);
                        if (document is null)
                        {
                            continue;
                        }

                        var rideBytes = await renderer.RenderAsync(document.Ride, stoppingToken);
                        await sender.SendFacturaAsync(document, rideBytes, stoppingToken);
                        await RegisterEventAsync(
                            dbContext,
                            candidate.Id,
                            Domain.Modules.Facturacion.Entities.FacturaEstado.AUTORIZADO,
                            $"{EmailSentPrefix}{document.DestinatarioEmail}|Correo enviado correctamente con XML y RIDE.",
                            stoppingToken);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "No se pudo enviar el correo de la factura {FacturaId}. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}", candidate.Id, candidate.EmpresaId, tenantContextAccessor.UserId);
                        await RegisterEventAsync(
                            dbContext,
                            candidate.Id,
                            Domain.Modules.Facturacion.Entities.FacturaEstado.AUTORIZADO,
                            $"{EmailFailedPrefix}{exception.GetType().Name}|{TrimMessage(exception.Message)}",
                            stoppingToken);
                    }
                }
                tenantContextAccessor.EmpresaId = null;
                tenantContextAccessor.UserId = null;
                tenantContextAccessor.IsSystemContext = false;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error en el notificador de correos de facturacion.");
            }
            finally
            {
                if (tenantContextAccessor is not null)
                {
                    tenantContextAccessor.EmpresaId = null;
                    tenantContextAccessor.UserId = null;
                    tenantContextAccessor.IsSystemContext = false;
                }
            }

            await Task.Delay(ScanInterval, stoppingToken);
        }
    }

    private static async Task<bool> ShouldProcessAsync(TestDeIaDbContext dbContext, Guid facturaId, CancellationToken cancellationToken)
    {
        var events = await dbContext.FacturaSriEventos
            .AsNoTracking()
            .Where(current => current.FacturaId == facturaId)
            .OrderByDescending(current => current.CreatedAt)
            .ToListAsync(cancellationToken);

        if (events.Any(current => current.Mensaje.StartsWith(EmailSentPrefix, StringComparison.Ordinal)))
        {
            return false;
        }

        var latestEmailEvent = events.FirstOrDefault(current =>
            current.Mensaje.StartsWith(EmailFailedPrefix, StringComparison.Ordinal) ||
            current.Mensaje.StartsWith(EmailSentPrefix, StringComparison.Ordinal));

        if (latestEmailEvent is null)
        {
            return true;
        }

        var failureCount = events.Count(current => current.Mensaje.StartsWith(EmailFailedPrefix, StringComparison.Ordinal));
        var nextAttemptAt = latestEmailEvent.CreatedAt.Add(ComputeRetryDelay(failureCount));
        return nextAttemptAt <= DateTimeOffset.UtcNow;
    }

    private static async Task RegisterEventAsync(
        TestDeIaDbContext dbContext,
        Guid facturaId,
        Domain.Modules.Facturacion.Entities.FacturaEstado estado,
        string mensaje,
        CancellationToken cancellationToken)
    {
        dbContext.FacturaSriEventos.Add(new FacturaSriEventoEntity
        {
            Id = Guid.NewGuid(),
            FacturaId = facturaId,
            Estado = estado,
            Mensaje = mensaje,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TimeSpan ComputeRetryDelay(int failureCount)
    {
        var safeCount = Math.Max(1, failureCount);
        var exponent = Math.Min(safeCount - 1, 5);
        var delayMinutes = Math.Min(5 * Math.Pow(2, exponent), 60);
        return TimeSpan.FromMinutes(delayMinutes);
    }

    private static string TrimMessage(string value)
    {
        return value.Length <= 300 ? value : value[..300];
    }
}
