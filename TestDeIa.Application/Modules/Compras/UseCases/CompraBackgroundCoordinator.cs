using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Application.Modules.Compras.UseCases;

public sealed class CompraBackgroundCoordinator : ICompraBackgroundCoordinator
{
    private readonly ICompraRepository compraRepository;
    private readonly ISriCompraProcessor sriCompraProcessor;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public CompraBackgroundCoordinator(
        ICompraRepository compraRepository,
        ISriCompraProcessor sriCompraProcessor,
        ITenantContextAccessor tenantContextAccessor)
    {
        this.compraRepository = compraRepository;
        this.sriCompraProcessor = sriCompraProcessor;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task ProcessCompraAsync(Guid compraId, string workerId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.IsSystemContext = true;
        var compra = await compraRepository.TryClaimLiquidacionAsync(compraId, workerId, DateTimeOffset.UtcNow, cancellationToken);
        if (compra is null)
        {
            return;
        }

        await ProcessClaimedCompraAsync(compra, cancellationToken);
    }

    public async Task ProcessPendingBatchAsync(int batchSize, string workerId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        tenantContextAccessor.IsSystemContext = true;
        var ids = await compraRepository.GetPendingLiquidacionIdsAsync(batchSize, now, cancellationToken);

        foreach (var compraId in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var compra = await compraRepository.TryClaimLiquidacionAsync(compraId, workerId, now, cancellationToken);
            if (compra is null)
            {
                continue;
            }

            await ProcessClaimedCompraAsync(compra, cancellationToken);
        }
    }

    private async Task ProcessClaimedCompraAsync(Domain.Modules.Compras.Entities.Compra compra, CancellationToken cancellationToken)
    {
        try
        {
            tenantContextAccessor.IsSystemContext = false;
            tenantContextAccessor.EmpresaId = compra.EmpresaId;

            var result = await sriCompraProcessor.ProcessAsync(compra, cancellationToken);

            if (string.IsNullOrWhiteSpace(compra.ClaveAccesoGenerada))
            {
                throw new InvalidOperationException("La liquidacion reclamada no tiene clave de acceso generada.");
            }

            if (!string.Equals(compra.ClaveAccesoGenerada, result.ClaveAcceso, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Se detecto una violacion de idempotencia en la liquidacion {compra.Id}. La clave almacenada es {compra.ClaveAccesoGenerada} y el motor devolvio {result.ClaveAcceso}.");
            }

            if (result.EstadoFinal == FacturaEstado.AUTORIZADO)
            {
                await compraRepository.MarkLiquidacionAsAuthorizedAsync(
                    compra.Id,
                    result.ClaveAcceso,
                    result.NumeroAutorizacion,
                    result.XmlFirmado ?? throw new InvalidOperationException("El motor devolvio AUTORIZADO sin XML firmado."),
                    result.Mensaje,
                    result.FechaRespuesta,
                    cancellationToken);
                return;
            }

            if (result.EstadoFinal == FacturaEstado.PENDIENTE)
            {
                await compraRepository.MarkLiquidacionAsSignedPendingAsync(
                    compra.Id,
                    result.ClaveAcceso,
                    result.XmlFirmado ?? throw new InvalidOperationException("El motor marco PENDIENTE sin XML firmado."),
                    result.Mensaje,
                    result.FechaRespuesta,
                    cancellationToken);
                return;
            }

            if (result.EstadoFinal == FacturaEstado.NO_FIRMADO)
            {
                await compraRepository.MarkLiquidacionAsUnsignedAsync(
                    compra.Id,
                    result.ClaveAcceso,
                    result.Mensaje,
                    result.XmlGenerado ?? compra.XmlGenerado ?? string.Empty,
                    result.FechaRespuesta,
                    cancellationToken);
                return;
            }

            await compraRepository.MarkLiquidacionAsRejectedAsync(
                compra.Id,
                result.Mensaje,
                result.XmlFirmado,
                result.FechaRespuesta,
                cancellationToken);
        }
        catch (Exception exception)
        {
            tenantContextAccessor.IsSystemContext = true;
            await compraRepository.MarkLiquidacionAsErrorAsync(
                compra.Id,
                $"{exception.GetType().Name}: {exception.Message}",
                DateTimeOffset.UtcNow.AddSeconds(20),
                cancellationToken);
        }
        finally
        {
            tenantContextAccessor.EmpresaId = null;
            tenantContextAccessor.IsSystemContext = false;
        }
    }
}
