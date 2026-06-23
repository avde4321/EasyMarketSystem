using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.Ports.In;

public interface IFacturacionUseCase
{
    Task<IReadOnlyCollection<PosClienteResponse>> SearchClientesAsync(string term, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PosProductoResponse>> SearchProductosAsync(string term, CancellationToken cancellationToken = default);

    Task<FacturaEmissionResponse> EmitirFacturaAsync(EmitirFacturaRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FacturaMonitorResponse>> GetMonitorAsync(CancellationToken cancellationToken = default);
}
