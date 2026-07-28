using TestDeIa.Application.Modules.Compras.Models;
using TestDeIa.Domain.Modules.Compras.Entities;

namespace TestDeIa.Application.Modules.Compras.Ports.Out;

public interface ISriCompraProcessor
{
    Task<SriCompraProcessingResult> ProcessAsync(Compra compra, CancellationToken cancellationToken = default);
}
