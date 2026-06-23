namespace TestDeIa.Application.Modules.Facturacion.Ports.Out;

public interface IFacturaBackgroundQueue
{
    void Enqueue(Guid facturaId);

    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
}
