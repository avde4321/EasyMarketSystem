using TestDeIa.Application.Modules.Facturacion.Models;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Application.Modules.Facturacion.Ports.Out;

public interface ISriFacturaProcessor
{
    Task<SriFacturaProcessingResult> ProcessAsync(Factura factura, CancellationToken cancellationToken = default);
}
