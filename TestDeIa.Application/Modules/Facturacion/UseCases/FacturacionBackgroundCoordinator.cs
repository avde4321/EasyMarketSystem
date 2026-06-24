using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Application.Modules.Facturacion.UseCases;

public sealed class FacturacionBackgroundCoordinator : IFacturacionBackgroundCoordinator
{
    private readonly IFacturacionRepository facturacionRepository;
    private readonly ISriFacturaProcessor sriFacturaProcessor;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public FacturacionBackgroundCoordinator(
        IFacturacionRepository facturacionRepository,
        ISriFacturaProcessor sriFacturaProcessor,
        ITenantContextAccessor tenantContextAccessor)
    {
        this.facturacionRepository = facturacionRepository;
        this.sriFacturaProcessor = sriFacturaProcessor;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task ProcessFacturaAsync(Guid facturaId, string workerId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.IsSystemContext = true;
        var factura = await facturacionRepository.TryClaimFacturaAsync(
            facturaId,
            workerId,
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (factura is null)
        {
            return;
        }

        await ProcessClaimedFacturaAsync(factura, cancellationToken);
    }

    public async Task ProcessPendingBatchAsync(int batchSize, string workerId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        tenantContextAccessor.IsSystemContext = true;
        var pendingIds = await facturacionRepository.GetPendingFacturaIdsAsync(batchSize, now, cancellationToken);

        foreach (var facturaId in pendingIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var factura = await facturacionRepository.TryClaimFacturaAsync(
                facturaId,
                workerId,
                now,
                cancellationToken);

            if (factura is null)
            {
                continue;
            }

            await ProcessClaimedFacturaAsync(factura, cancellationToken);
        }
    }

    private async Task ProcessClaimedFacturaAsync(
        Domain.Modules.Facturacion.Entities.Factura factura,
        CancellationToken cancellationToken)
    {
        try
        {
            tenantContextAccessor.IsSystemContext = false;
            tenantContextAccessor.EmpresaId = factura.EmpresaId;

            await facturacionRepository.MarkFacturaAsReceivedAsync(
                factura.Id,
                "Comprobante recibido por el motor de procesamiento.",
                cancellationToken);

            var result = await sriFacturaProcessor.ProcessAsync(factura, cancellationToken);

            if (string.Equals(result.EstadoFinal, FacturaEstados.Autorizado, StringComparison.OrdinalIgnoreCase))
            {
                await facturacionRepository.MarkFacturaAsAuthorizedAsync(
                    factura.Id,
                    result.ClaveAcceso,
                    result.NumeroAutorizacion,
                    result.XmlFirmado,
                    result.Mensaje,
                    result.FechaRespuesta,
                    cancellationToken);

                return;
            }

            if (string.Equals(result.EstadoFinal, FacturaEstados.NoFirmado, StringComparison.OrdinalIgnoreCase))
            {
                await facturacionRepository.MarkFacturaAsUnsignedAsync(
                    factura.Id,
                    result.ClaveAcceso,
                    result.Mensaje,
                    result.XmlGenerado ?? factura.XmlGenerado ?? string.Empty,
                    result.FechaRespuesta,
                    cancellationToken);

                return;
            }

            await facturacionRepository.MarkFacturaAsRejectedAsync(
                factura.Id,
                result.Mensaje,
                result.XmlFirmado,
                result.FechaRespuesta,
                cancellationToken);
        }
        catch (Exception exception)
        {
            tenantContextAccessor.IsSystemContext = true;
            await facturacionRepository.MarkFacturaAsErrorAsync(
                factura.Id,
                exception.Message,
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
