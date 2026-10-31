using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.Ports.In;

public interface ICompraUseCase
{
    Task<CompraResponse> RegistrarAsync(RegistrarCompraRequest request, CancellationToken cancellationToken = default);
    Task<PagedResultResponse<CuentaPorPagarResponse>> GetCuentasPorPagarAsync(string? term, Guid? proveedorId, int skip, int take, CancellationToken cancellationToken = default);
    Task<CuentasPorPagarResumenResponse> GetCuentasPorPagarResumenAsync(CancellationToken cancellationToken = default);
    Task<CuentaPorPagarResponse> RegistrarAbonoAsync(RegistrarAbonoCxPRequest request, CancellationToken cancellationToken = default);
}
