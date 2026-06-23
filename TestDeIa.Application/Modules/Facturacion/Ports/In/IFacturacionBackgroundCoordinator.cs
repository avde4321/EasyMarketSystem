namespace TestDeIa.Application.Modules.Facturacion.Ports.In;

public interface IFacturacionBackgroundCoordinator
{
    Task ProcessFacturaAsync(Guid facturaId, string workerId, CancellationToken cancellationToken = default);

    Task ProcessPendingBatchAsync(int batchSize, string workerId, CancellationToken cancellationToken = default);
}
