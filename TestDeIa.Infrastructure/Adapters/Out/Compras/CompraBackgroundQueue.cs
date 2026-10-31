using System.Threading.Channels;
using TestDeIa.Application.Modules.Compras.Ports.Out;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class CompraBackgroundQueue : ICompraBackgroundQueue
{
    private readonly Channel<Guid> channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false,
        AllowSynchronousContinuations = false
    });

    public void Enqueue(Guid compraId)
    {
        channel.Writer.TryWrite(compraId);
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        return channel.Reader.ReadAsync(cancellationToken);
    }
}
