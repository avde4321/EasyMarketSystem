namespace TestDeIa.Application.Modules.Compras.Ports.Out;

public interface ICompraBackgroundQueue
{
    void Enqueue(Guid compraId);

    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
}
