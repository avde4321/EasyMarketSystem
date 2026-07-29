using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.Ports.In;

public interface INotaCreditoService
{
    Task<NotaCreditoOrigenResponseDto> GetFacturaOrigenAsync(Guid facturaId, CancellationToken cancellationToken = default);

    Task<NotaCreditoResponseDto> CrearNotaCredito(NotaCreditoRequestDto dto, CancellationToken cancellationToken = default);
}
