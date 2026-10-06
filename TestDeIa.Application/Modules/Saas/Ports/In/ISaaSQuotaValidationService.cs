namespace TestDeIa.Application.Modules.Saas.Ports.In;

public interface ISaaSQuotaValidationService
{
    Task CanEmitirFacturaAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task CanCrearUsuarioAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task CanCrearSucursalAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task IncrementarFacturasEmitidasMesActualAsync(Guid empresaId, CancellationToken cancellationToken = default);
}
