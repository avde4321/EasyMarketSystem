using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Application.Modules.Compras.Ports.Out;

namespace TestDeIa.Infrastructure;

public sealed class CompraBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ICompraBackgroundQueue backgroundQueue;
    private readonly ILogger<CompraBackgroundWorker> logger;
    private readonly string workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    public CompraBackgroundWorker(
        IServiceScopeFactory scopeFactory,
        ICompraBackgroundQueue backgroundQueue,
        ILogger<CompraBackgroundWorker> logger)
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
                var dequeueTask = backgroundQueue.DequeueAsync(stoppingToken).AsTask();
                var timerTask = Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                var completedTask = await Task.WhenAny(dequeueTask, timerTask);

                using var scope = scopeFactory.CreateScope();
                var coordinator = scope.ServiceProvider.GetRequiredService<ICompraBackgroundCoordinator>();

                if (completedTask == dequeueTask)
                {
                    var compraId = await dequeueTask;
                    await coordinator.ProcessCompraAsync(compraId, workerId, stoppingToken);
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
                logger.LogError(exception, "Error procesando la cola de liquidaciones de compra.");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }
}
