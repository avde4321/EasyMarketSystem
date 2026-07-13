using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.Ports.In;

public interface IFacturacionUseCase
{
    Task<PagedResultResponse<PosClienteResponse>> SearchClientesAsync(string term, int skip, int take, CancellationToken cancellationToken = default);

    Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take, Guid? bodegaId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync(CancellationToken cancellationToken = default);

    Task<FacturaEmissionResponse> EmitirFacturaAsync(EmitirFacturaRequest request, CancellationToken cancellationToken = default);

    Task<PagedResultResponse<FacturaMonitorResponse>> GetMonitorAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);
}
