namespace TestDeIa.Application.Modules.Saas.Ports.In;

public interface ISaaSQuotaValidationService
{
    Task EnsureModuleAsync(Guid empresaId, TestDeIa.Domain.Modules.Saas.Enums.SaasModule module, CancellationToken cancellationToken = default);
    Task EnsureActiveSubscriptionAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task CanEmitirFacturaAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task CanCrearUsuarioAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task CanCrearSucursalAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task BloquearCuotaAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task ReservarCupoFacturaAsync(Guid empresaId, Guid facturaId, CancellationToken cancellationToken = default);
    Task ConfirmarFacturaAutorizadaAsync(Guid empresaId, Guid facturaId, CancellationToken cancellationToken = default);
    Task LiberarCupoFacturaAsync(Guid empresaId, Guid facturaId, CancellationToken cancellationToken = default);
}
