using System.Threading.Channels;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class FacturaBackgroundQueue : IFacturaBackgroundQueue
{
    private readonly Channel<Guid> channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false,
        AllowSynchronousContinuations = false
    });

    public void Enqueue(Guid facturaId)
    {
        channel.Writer.TryWrite(facturaId);
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        return channel.Reader.ReadAsync(cancellationToken);
    }
}
