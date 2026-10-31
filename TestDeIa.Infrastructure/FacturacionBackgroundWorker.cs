using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;

namespace TestDeIa.Infrastructure;

public sealed class FacturacionBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IFacturaBackgroundQueue backgroundQueue;
    private readonly ILogger<FacturacionBackgroundWorker> logger;
    private readonly string workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    public FacturacionBackgroundWorker(
        IServiceScopeFactory scopeFactory,
        IFacturaBackgroundQueue backgroundQueue,
        ILogger<FacturacionBackgroundWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.backgroundQueue = backgroundQueue;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var readTimeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                readTimeout.CancelAfter(TimeSpan.FromSeconds(5));
                Guid? queuedFacturaId = null;
                try
                {
                    queuedFacturaId = await backgroundQueue.DequeueAsync(readTimeout.Token);
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // La lectura termina antes de iniciar otra; no quedan consumidores huérfanos.
                }

                using var scope = scopeFactory.CreateScope();
                var coordinator = scope.ServiceProvider.GetRequiredService<IFacturacionBackgroundCoordinator>();

                if (queuedFacturaId.HasValue)
                {
                    await coordinator.ProcessFacturaAsync(queuedFacturaId.Value, workerId, stoppingToken);
                }
                else
                {
                    await coordinator.ProcessPendingBatchAsync(10, workerId, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Error procesando la cola de facturacion. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}, Error: {Mensaje}",
                    null,
                    null,
                    exception.Message);
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }
}
