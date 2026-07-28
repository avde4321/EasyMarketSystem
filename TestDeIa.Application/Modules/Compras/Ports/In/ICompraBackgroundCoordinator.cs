namespace TestDeIa.Application.Modules.Compras.Ports.In;

public interface ICompraBackgroundCoordinator
{
    Task ProcessCompraAsync(Guid compraId, string workerId, CancellationToken cancellationToken = default);

    Task ProcessPendingBatchAsync(int batchSize, string workerId, CancellationToken cancellationToken = default);
}
