using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Application.Modules.Caja.Ports.Out;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Application.Modules.Facturacion.UseCases;

public sealed class FacturacionUseCase : IFacturacionUseCase
{
    private readonly IFacturacionRepository facturacionRepository;
    private readonly IFacturaBackgroundQueue facturaBackgroundQueue;
    private readonly ICatalogoRepository catalogoRepository;
    private readonly ICajaSesionRepository cajaSesionRepository;

    public FacturacionUseCase(
        IFacturacionRepository facturacionRepository,
        IFacturaBackgroundQueue facturaBackgroundQueue,
        ICatalogoRepository catalogoRepository,
        ICajaSesionRepository cajaSesionRepository)
    {
        this.facturacionRepository = facturacionRepository;
        this.facturaBackgroundQueue = facturaBackgroundQueue;
        this.catalogoRepository = catalogoRepository;
        this.cajaSesionRepository = cajaSesionRepository;
    }

    public Task<PagedResultResponse<PosClienteResponse>> SearchClientesAsync(string term, int skip, int take, CancellationToken cancellationToken = default)
    {
        return facturacionRepository.SearchClientesAsync(term, skip, take, cancellationToken);
    }

    public Task<PagedResultResponse<PosProductoResponse>> SearchProductosAsync(string term, int skip, int take, Guid? bodegaId = null, CancellationToken cancellationToken = default)
    {
        return facturacionRepository.SearchProductosAsync(term, skip, take, bodegaId, cancellationToken);
    }

    public Task<IReadOnlyCollection<PosPuntoEmisionResponse>> GetPuntosEmisionAsync(CancellationToken cancellationToken = default)
    {
        return facturacionRepository.GetPuntosEmisionAsync(cancellationToken);
    }

    public Task<IReadOnlyCollection<PosOperadorResponse>> GetOperadoresAsync(CancellationToken cancellationToken = default)
    {
        return facturacionRepository.GetOperadoresAsync(cancellationToken);
    }

    public async Task<FacturaEmissionResponse> EmitirFacturaAsync(EmitirFacturaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var response = await facturacionRepository.CreatePendingFacturaAsync(
            request,
            cancellationToken);

        if (!string.Equals(response.Estado, FacturaEstado.AUTORIZADO.ToApiValue(), StringComparison.OrdinalIgnoreCase))
        {
            facturaBackgroundQueue.Enqueue(response.FacturaId);
        }

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

        var formaPago = SriCatalogCodes.NormalizeFormaPagoCode(request.FormaPago);
        if (formaPago is null ||
            !await catalogoRepository.ExistsActiveItemAsync("FORMA_PAGO_SRI", formaPago, cancellationToken))
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

        if (!await cajaSesionRepository.HasActiveSessionAsync(cancellationToken))
        {
            throw new InvalidOperationException("Debes abrir una caja antes de facturar en el POS.");
        }
    }
}
