using TestDeIa.Domain.Modules.Compras.Entities;

namespace TestDeIa.Application.Modules.Compras.Ports.Out;

public interface ICompraRepository
{
    Task<Compra> CreateAsync(Compra compra, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetPendingLiquidacionIdsAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<Compra?> TryClaimLiquidacionAsync(Guid compraId, string workerId, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task MarkLiquidacionAsAuthorizedAsync(
        Guid compraId,
        string claveAcceso,
        string? numeroAutorizacion,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);
    Task MarkLiquidacionAsSignedPendingAsync(
        Guid compraId,
        string claveAcceso,
        string xmlFirmado,
        string mensaje,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);
    Task MarkLiquidacionAsUnsignedAsync(
        Guid compraId,
        string claveAcceso,
        string mensaje,
        string xmlGenerado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);
    Task MarkLiquidacionAsRejectedAsync(
        Guid compraId,
        string mensaje,
        string? xmlFirmado,
        DateTimeOffset fechaRespuesta,
        CancellationToken cancellationToken = default);
    Task MarkLiquidacionAsErrorAsync(
        Guid compraId,
        string mensaje,
        DateTimeOffset nextRetryAt,
        CancellationToken cancellationToken = default);
}
