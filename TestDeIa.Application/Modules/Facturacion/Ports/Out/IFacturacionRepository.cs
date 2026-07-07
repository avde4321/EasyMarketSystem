using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.Ports.Out;

public interface IFacturacionRepository
{
    Task<PagedResultResponse<PosClienteResponse>> SearchClientesAsync(string term, int skip, int take, CancellationToken cancellationToken = default);

    Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync(CancellationToken cancellationToken = default);

    Task<FacturaEmissionResponse> CreatePendingFacturaAsync(
        EmitirFacturaRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResultResponse<FacturaMonitorResponse>> GetMonitorAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> GetPendingFacturaIdsAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<Factura?> TryClaimFacturaAsync(Guid facturaId, string workerId, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task MarkFacturaAsReceivedAsync(Guid facturaId, string mensaje, CancellationToken cancellationToken = default);

    Task MarkFacturaAsAuthorizedAsync(
        Guid facturaId,
        string claveAcceso,
        string? numeroAutorizacion,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);

    Task MarkFacturaAsSignedPendingAsync(
        Guid facturaId,
        string claveAcceso,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);

    Task MarkFacturaAsRejectedAsync(
        Guid facturaId,
        string mensaje,
        string? xmlFirmado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);

    Task MarkFacturaAsUnsignedAsync(
        Guid facturaId,
        string claveAcceso,
        string mensaje,
        string xmlGenerado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);

    Task MarkFacturaAsErrorAsync(
        Guid facturaId,
        string mensaje,
        DateTimeOffset nextRetryAt,
        CancellationToken cancellationToken = default);
}
