using TestDeIa.Domain.Modules.Caja.Entities;

namespace TestDeIa.Application.Modules.Caja.Ports.Out;

public interface ICajaSesionRepository
{
    Task<CajaSesion?> GetActivaAsync(CancellationToken cancellationToken = default);
    Task<bool> HasActiveSessionAsync(CancellationToken cancellationToken = default);
    Task<CajaSesion> AbrirAsync(decimal montoApertura, CancellationToken cancellationToken = default);
    Task<CajaSesion> CerrarAsync(
        decimal montoFisicoEfectivoReal,
        decimal montoFisicoTarjetaReal,
        decimal montoFisicoTransferenciaReal,
        CancellationToken cancellationToken = default);
}
