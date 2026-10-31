using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;

namespace TestDeIa.Application.Modules.Compras.Ports.In;

public interface ICuentaPorPagarUseCase
{
    Task<PagedResultResponse<CuentaPorPagarResponse>> GetPagedAsync(string? term, Guid? proveedorId, int skip, int take, CancellationToken cancellationToken = default);
    Task<CuentasPorPagarResumenResponse> GetResumenAsync(CancellationToken cancellationToken = default);
    Task<CuentaPorPagarResponse> RegistrarAbonoAsync(RegistrarAbonoCxPRequest request, CancellationToken cancellationToken = default);
}
