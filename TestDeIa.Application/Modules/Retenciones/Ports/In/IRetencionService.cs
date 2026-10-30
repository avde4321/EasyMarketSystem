using TestDeIa.Shared.Requests.Retenciones;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Retenciones;

namespace TestDeIa.Application.Modules.Retenciones.Ports.In;

public interface IRetencionService
{
    Task ActualizarParteRelacionadaAsync(Guid id, bool parteRelacionada, CancellationToken cancellationToken = default);
    Task<PagedResultResponse<ComprobanteRetencionResponse>> GetPagedAsync(
        string? term,
        DateTimeOffset fechaDesde,
        DateTimeOffset fechaHasta,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<ComprobanteRetencionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ComprobanteRetencionResponse> CreateAsync(ComprobanteRetencionRequest request, CancellationToken cancellationToken = default);

    Task<ComprobanteRetencionResponse> CrearRetencionDesdeCompraAsync(Guid compraId, CancellationToken cancellationToken = default);

    Task<ComprobanteRetencionResponse> ProcesarRetencionSRIAsync(Guid id, CancellationToken cancellationToken = default);
}
