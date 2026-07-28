using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.UseCases;

public sealed class CuentaPorPagarService(ICompraUseCase compraUseCase)
{
    public Task<CuentaPorPagarResponse> RegistrarPagoAsync(RegistrarAbonoCxPRequest request, CancellationToken cancellationToken = default)
    {
        return compraUseCase.RegistrarAbonoAsync(request, cancellationToken);
    }
}
