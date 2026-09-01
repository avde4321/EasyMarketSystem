using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Modules.Sri.Ports.Out;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Options;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure;

public sealed class SriOutboxProcessorWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<SriOutboxOptions> options,
    ILogger<SriOutboxProcessorWorker> logger) : BackgroundService
{
    private readonly SriOutboxOptions options = options.Value;
    private readonly string workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("SriOutboxProcessorWorker esta deshabilitado por configuracion.");
            return;
        }

        var delay = TimeSpan.FromSeconds(Math.Max(1, options.DelaySeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error no controlado en el worker de Outbox SRI. El proceso continuara en el siguiente ciclo.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDeIaDbContext>();
        var processor = scope.ServiceProvider.GetRequiredService<ISriComprobanteProcessor>();
        var now = DateTimeOffset.UtcNow;
        var batchSize = Math.Clamp(options.BatchSize, 1, 100);

        var candidates = await dbContext.ColaProcesamientoSRI
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current =>
                current.Estado == SriOutboxEstados.Pendiente ||
                (current.Estado == SriOutboxEstados.Error &&
                 current.Intentos < options.MaxIntentos &&
                 (!current.NextRetryAt.HasValue || current.NextRetryAt <= now)))
            .OrderBy(current => current.CreatedAt)
            .Take(batchSize)
            .Select(current => current.Id)
            .ToArrayAsync(stoppingToken);

        foreach (var candidateId in candidates)
        {
            await TryProcessItemAsync(dbContext, processor, candidateId, stoppingToken);
        }
    }

    private async Task TryProcessItemAsync(
        TestDeIaDbContext dbContext,
        ISriComprobanteProcessor processor,
        Guid candidateId,
        CancellationToken stoppingToken)
    {
        var now = DateTimeOffset.UtcNow;
        var claimed = await dbContext.ColaProcesamientoSRI
            .IgnoreQueryFilters()
            .Where(current =>
                current.Id == candidateId &&
                (current.Estado == SriOutboxEstados.Pendiente ||
                 (current.Estado == SriOutboxEstados.Error &&
                  current.Intentos < options.MaxIntentos &&
                  (!current.NextRetryAt.HasValue || current.NextRetryAt <= now))))
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(current => current.Estado, SriOutboxEstados.EnProceso)
                .SetProperty(current => current.ProcessingNode, workerId)
                .SetProperty(current => current.ProcessingStartedAt, now)
                .SetProperty(current => current.UpdatedAt, now),
                stoppingToken);

        if (claimed == 0)
        {
            return;
        }

        try
        {
            await processor.ProcessAsync(candidateId, workerId, stoppingToken);
            await dbContext.SaveChangesAsync(stoppingToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "No se pudo procesar item de Outbox SRI {ColaId}.", candidateId);
            await MarkAsFailedAsync(dbContext, candidateId, exception.Message, stoppingToken);
        }
    }

    private async Task MarkAsFailedAsync(
        TestDeIaDbContext dbContext,
        Guid candidateId,
        string error,
        CancellationToken stoppingToken)
    {
        var now = DateTimeOffset.UtcNow;
        var nextRetryAt = now.AddMinutes(ResolveRetryMinutes(candidateId));
        var normalizedError = error.Length > 2000 ? error.Substring(0, 2000) : error;

        await dbContext.ColaProcesamientoSRI
            .IgnoreQueryFilters()
            .Where(current => current.Id == candidateId && current.ProcessingNode == workerId)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(current => current.Estado, SriOutboxEstados.Error)
                .SetProperty(current => current.Intentos, current => current.Intentos + 1)
                .SetProperty(current => current.UltimoError, normalizedError)
                .SetProperty(current => current.Mensaje, "Error procesando Outbox SRI. Se programo reintento automatico.")
                .SetProperty(current => current.NextRetryAt, nextRetryAt)
                .SetProperty(current => current.UpdatedAt, now)
                .SetProperty(current => current.ProcessingNode, (string?)null)
                .SetProperty(current => current.ProcessingStartedAt, (DateTimeOffset?)null),
                stoppingToken);
    }

    private static int ResolveRetryMinutes(Guid candidateId)
    {
        return Math.Abs(candidateId.GetHashCode()) % 3 switch
        {
            0 => 1,
            1 => 5,
            _ => 15
        };
    }
}
