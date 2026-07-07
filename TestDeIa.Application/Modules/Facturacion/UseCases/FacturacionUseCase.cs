using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.UseCases;

public sealed class FacturacionUseCase : IFacturacionUseCase
{
    private readonly IFacturacionRepository facturacionRepository;
    private readonly IFacturaBackgroundQueue facturaBackgroundQueue;
    private readonly ICatalogoRepository catalogoRepository;

    public FacturacionUseCase(
        IFacturacionRepository facturacionRepository,
        IFacturaBackgroundQueue facturaBackgroundQueue,
        ICatalogoRepository catalogoRepository)
    {
        this.facturacionRepository = facturacionRepository;
        this.facturaBackgroundQueue = facturaBackgroundQueue;
        this.catalogoRepository = catalogoRepository;
    }

    public Task<PagedResultResponse<PosClienteResponse>> SearchClientesAsync(string term, int skip, int take, CancellationToken cancellationToken = default)
    {
        return facturacionRepository.SearchClientesAsync(term, skip, take, cancellationToken);
    }

    public Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take, CancellationToken cancellationToken = default)
    {
        return facturacionRepository.SearchProductosAsync(term, skip, take, cancellationToken);
    }

    public Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync(CancellationToken cancellationToken = default)
    {
        return facturacionRepository.GetPuntosEmisionAsync(cancellationToken);
    }

    public async Task<FacturaEmissionResponse> EmitirFacturaAsync(EmitirFacturaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var response = await facturacionRepository.CreatePendingFacturaAsync(
            request,
            cancellationToken);

        facturaBackgroundQueue.Enqueue(response.FacturaId);

        return response;
    }

    public Task<PagedResultResponse<FacturaMonitorResponse>> GetMonitorAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        return facturacionRepository.GetMonitorAsync(term, skip, take, cancellationToken);
    }

    private async Task ValidateRequestAsync(EmitirFacturaRequest request, CancellationToken cancellationToken)
    {
        if (request.ClienteId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe seleccionar un cliente para emitir la factura.");
        }

        if (string.IsNullOrWhiteSpace(request.Establecimiento) || request.Establecimiento.Trim().Length != 3 || !request.Establecimiento.Trim().All(char.IsDigit))
        {
            throw new InvalidOperationException("Debe seleccionar un establecimiento valido para operar el POS.");
        }

        if (string.IsNullOrWhiteSpace(request.PuntoEmision) || request.PuntoEmision.Trim().Length != 3 || !request.PuntoEmision.Trim().All(char.IsDigit))
        {
            throw new InvalidOperationException("Debe seleccionar un punto de emision valido para operar el POS.");
        }

        if (string.IsNullOrWhiteSpace(request.FormaPago))
        {
            throw new InvalidOperationException("La forma de pago es obligatoria.");
        }

        if (!await catalogoRepository.ExistsActiveItemAsync("FORMA_PAGO_SRI", request.FormaPago.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("La forma de pago seleccionada no esta disponible en el catalogo activo.");
        }

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("La factura debe tener al menos un producto.");
        }

        if (request.Items.Any(item => item.ProductoId == Guid.Empty || item.Cantidad <= 0 || item.Descuento < 0))
        {
            throw new InvalidOperationException("La factura contiene productos invalidos.");
        }
    }
}
