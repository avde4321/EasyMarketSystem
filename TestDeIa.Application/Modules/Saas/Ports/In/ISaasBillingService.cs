using TestDeIa.Shared.Requests.Saas;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Application.Modules.Saas.Ports.In;

public interface ISaasBillingService
{
    Task<SaasBillingPortalResponse> ObtenerPortalAsync(CancellationToken cancellationToken = default);
    Task<SaasCheckoutResponse> PrepararCheckoutAsync(int saasPlanId, CancellationToken cancellationToken = default);
    Task<SaasPagoResponse> RegistrarTransferenciaAsync(RegistrarTransferenciaSaasDto dto, CancellationToken cancellationToken = default);
    Task<SaasPagoResponse> AprobarPagoAsync(Guid pagoId, Guid empresaId, string? observaciones = null, CancellationToken cancellationToken = default);
    Task<SaasPagoResponse> RechazarPagoAsync(Guid pagoId, Guid empresaId, string? observaciones = null, CancellationToken cancellationToken = default);
}
