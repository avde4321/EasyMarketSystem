using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Application.Modules.Facturacion.UseCases;

public sealed class FacturacionUseCase : IFacturacionUseCase
{
    private readonly IFacturacionRepository facturacionRepository;
    private readonly IFacturaBackgroundQueue facturaBackgroundQueue;

    public FacturacionUseCase(
        IFacturacionRepository facturacionRepository,
        IFacturaBackgroundQueue facturaBackgroundQueue)
    {
        this.facturacionRepository = facturacionRepository;
        this.facturaBackgroundQueue = facturaBackgroundQueue;
    }

    public Task<IReadOnlyCollection<PosClienteResponse>> SearchClientesAsync(string term, CancellationToken cancellationToken = default)
    {
        return facturacionRepository.SearchClientesAsync(term, cancellationToken);
    }

    public Task<IReadOnlyCollection<PosProductoResponse>> SearchProductosAsync(string term, CancellationToken cancellationToken = default)
    {
        return facturacionRepository.SearchProductosAsync(term, cancellationToken);
    }

    public async Task<FacturaEmissionResponse> EmitirFacturaAsync(EmitirFacturaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var response = await facturacionRepository.CreatePendingFacturaAsync(
            request,
            cancellationToken);

        facturaBackgroundQueue.Enqueue(response.FacturaId);
        return response;
    }

    public Task<IReadOnlyCollection<FacturaMonitorResponse>> GetMonitorAsync(CancellationToken cancellationToken = default)
    {
        return facturacionRepository.GetMonitorAsync(cancellationToken);
    }

    private static void ValidateRequest(EmitirFacturaRequest request)
    {
        if (request.ClienteId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe seleccionar un cliente para emitir la factura.");
        }

        if (string.IsNullOrWhiteSpace(request.FormaPago))
        {
            throw new InvalidOperationException("La forma de pago es obligatoria.");
        }

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("La factura debe tener al menos un producto.");
        }

        if (request.Items.Any(item => item.ProductoId == Guid.Empty || item.Cantidad <= 0))
        {
            throw new InvalidOperationException("La factura contiene productos invalidos.");
        }
    }
}
