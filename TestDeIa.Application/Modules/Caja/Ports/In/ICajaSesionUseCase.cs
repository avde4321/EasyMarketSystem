using TestDeIa.Shared.Requests.Caja;
using TestDeIa.Shared.Responses.Caja;

namespace TestDeIa.Application.Modules.Caja.Ports.In;

public interface ICajaSesionUseCase
{
    Task<CajaSesionResponse?> GetActivaAsync(CancellationToken cancellationToken = default);
    Task<CajaSesionResponse> AbrirAsync(AbrirCajaRequest request, CancellationToken cancellationToken = default);
    Task<CajaSesionResponse> CerrarAsync(CerrarCajaRequest request, CancellationToken cancellationToken = default);
}
