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
        if (request.MontoFisicoEfectivoReal < 0 ||
            request.MontoFisicoTarjetaReal < 0 ||
            request.MontoFisicoTransferenciaReal < 0 ||
            request.MontoFisicoOtrosReal < 0)
        {
            throw new InvalidOperationException("Los montos fisicos no pueden ser negativos.");
        }

        return Map(await cajaSesionRepository.CerrarAsync(
            request.MontoFisicoEfectivoReal,
            request.MontoFisicoTarjetaReal,
            request.MontoFisicoTransferenciaReal,
            request.MontoFisicoOtrosReal,
            request.ObservacionesCierre,
            cancellationToken));
    }

    public async Task<CajaSesionResponse> RegistrarMovimientoCajaAsync(RegistrarMovimientoCajaRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Monto <= 0)
        {
            throw new InvalidOperationException("El monto del movimiento debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(request.Concepto))
        {
            throw new InvalidOperationException("El concepto del movimiento es obligatorio.");
        }

        return Map(await cajaSesionRepository.RegistrarMovimientoCajaAsync(
            request.TipoMovimiento,
            request.Monto,
            request.Concepto,
            request.ComprobanteReferencia,
            cancellationToken));
    }

    public async Task<CajaSesionResponse> IniciarArqueoCiegoAsync(IniciarArqueoCiegoRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MontoDeclaradoEfectivo < 0 ||
            request.MontoDeclaradoTarjetas < 0 ||
            request.MontoDeclaradoTransferencias < 0 ||
            request.MontoDeclaradoOtros < 0)
        {
            throw new InvalidOperationException("Los montos declarados no pueden ser negativos.");
        }

        return Map(await cajaSesionRepository.IniciarArqueoCiegoAsync(
            request.MontoDeclaradoEfectivo,
            request.MontoDeclaradoTarjetas,
            request.MontoDeclaradoTransferencias,
            request.MontoDeclaradoOtros,
            request.ObservacionesCierre,
            cancellationToken));
    }

    private static CajaSesionResponse Map(CajaSesion caja)
    {
        return new CajaSesionResponse
        {
            Id = caja.Id,
            EmpresaId = caja.EmpresaId,
            UsuarioId = caja.UsuarioId,
            PuntoEmisionId = caja.PuntoEmisionId,
            BodegaId = caja.BodegaId,
            FechaApertura = caja.FechaApertura,
            FechaCierre = caja.FechaCierre,
            MontoApertura = caja.MontoApertura,
            TotalVentasEfectivoCalculado = caja.TotalVentasEfectivoCalculado,
            TotalVentasTarjetaCalculado = caja.TotalVentasTarjetaCalculado,
            TotalVentasTransferenciaCalculado = caja.TotalVentasTransferenciaCalculado,
            MontoFisicoEfectivoReal = caja.MontoFisicoEfectivoReal,
            MontoFisicoTarjetaReal = caja.MontoFisicoTarjetaReal,
            MontoFisicoTransferenciaReal = caja.MontoFisicoTransferenciaReal,
            EfectivoReportado = caja.EfectivoReportado,
            TarjetaReportada = caja.TarjetaReportada,
            TransferenciaReportada = caja.TransferenciaReportada,
            DiferenciaEfectivo = caja.DiferenciaEfectivo,
            DiferenciaTarjeta = caja.DiferenciaTarjeta,
            DiferenciaTransferencia = caja.DiferenciaTransferencia,
            Diferencia = caja.Diferencia,
            MontoDeclaradoEfectivo = caja.MontoDeclaradoEfectivo,
            MontoDeclaradoTarjetas = caja.MontoDeclaradoTarjetas,
            MontoDeclaradoTransferencias = caja.MontoDeclaradoTransferencias,
            MontoDeclaradoOtros = caja.MontoDeclaradoOtros,
            MontoDeclaradoTotal = caja.MontoDeclaradoTotal,
            MontoCalculadoEfectivo = caja.MontoCalculadoEfectivo,
            MontoCalculadoTarjetas = caja.MontoCalculadoTarjetas,
            MontoCalculadoTransferencias = caja.MontoCalculadoTransferencias,
            MontoCalculadoOtros = caja.MontoCalculadoOtros,
            MontoCalculadoTotal = caja.MontoCalculadoTotal,
            DiferenciaMonto = caja.DiferenciaMonto,
            ObservacionesCierre = caja.ObservacionesCierre,
            AsientoContableId = caja.AsientoContableId,
            EstadoCaja = caja.EstadoCaja.ToString()
        };
    }
}
