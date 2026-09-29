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
        decimal montoFisicoOtrosReal,
        string? observacionesCierre,
        CancellationToken cancellationToken = default);
    Task<CajaSesion> RegistrarMovimientoCajaAsync(
        string tipoMovimiento,
        decimal monto,
        string concepto,
        string? comprobanteReferencia,
        CancellationToken cancellationToken = default);
    Task<CajaSesion> IniciarArqueoCiegoAsync(
        decimal montoDeclaradoEfectivo,
        decimal montoDeclaradoTarjetas,
        decimal montoDeclaradoTransferencias,
        decimal montoDeclaradoOtros,
        string? observacionesCierre,
        CancellationToken cancellationToken = default);
}
