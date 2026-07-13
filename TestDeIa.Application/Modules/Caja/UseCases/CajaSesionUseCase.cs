using TestDeIa.Application.Modules.Caja.Ports.In;
using TestDeIa.Application.Modules.Caja.Ports.Out;
using TestDeIa.Domain.Modules.Caja.Entities;
using TestDeIa.Shared.Requests.Caja;
using TestDeIa.Shared.Responses.Caja;

namespace TestDeIa.Application.Modules.Caja.UseCases;

public sealed class CajaSesionUseCase(ICajaSesionRepository cajaSesionRepository) : ICajaSesionUseCase
{
    public async Task<CajaSesionResponse?> GetActivaAsync(CancellationToken cancellationToken = default)
    {
        var caja = await cajaSesionRepository.GetActivaAsync(cancellationToken);
        return caja is null ? null : Map(caja);
    }

    public async Task<CajaSesionResponse> AbrirAsync(AbrirCajaRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MontoApertura < 0)
        {
            throw new InvalidOperationException("El monto de apertura no puede ser negativo.");
        }

        return Map(await cajaSesionRepository.AbrirAsync(request.MontoApertura, cancellationToken));
    }

    public async Task<CajaSesionResponse> CerrarAsync(CerrarCajaRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MontoFisicoEfectivoReal < 0 || request.MontoFisicoTarjetaReal < 0)
        {
            throw new InvalidOperationException("Los montos fisicos no pueden ser negativos.");
        }

        return Map(await cajaSesionRepository.CerrarAsync(
            request.MontoFisicoEfectivoReal,
            request.MontoFisicoTarjetaReal,
            cancellationToken));
    }

    private static CajaSesionResponse Map(CajaSesion caja)
    {
        return new CajaSesionResponse
        {
            Id = caja.Id,
            EmpresaId = caja.EmpresaId,
            UsuarioId = caja.UsuarioId,
            FechaApertura = caja.FechaApertura,
            FechaCierre = caja.FechaCierre,
            MontoApertura = caja.MontoApertura,
            TotalVentasEfectivoCalculado = caja.TotalVentasEfectivoCalculado,
            TotalVentasTarjetaCalculado = caja.TotalVentasTarjetaCalculado,
            MontoFisicoEfectivoReal = caja.MontoFisicoEfectivoReal,
            MontoFisicoTarjetaReal = caja.MontoFisicoTarjetaReal,
            DiferenciaEfectivo = caja.DiferenciaEfectivo,
            DiferenciaTarjeta = caja.DiferenciaTarjeta,
            EstadoCaja = caja.EstadoCaja.ToString()
        };
    }
}
