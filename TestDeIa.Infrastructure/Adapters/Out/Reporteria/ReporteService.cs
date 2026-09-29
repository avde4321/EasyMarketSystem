using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Reporteria.Ports.In;
using TestDeIa.Domain.Modules.Caja.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Infrastructure.Adapters.Out.Reporteria;

public sealed class ReporteService(TestDeIaDbContext dbContext) : IReporteService
{
    public async Task<PagedResultResponse<KardexValorizadoReporteItemResponse>> ObtenerKardexValorizadoAsync(
        DateTime desde,
        DateTime hasta,
        Guid? bodegaId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var inicio = new DateTimeOffset(desde.Date, TimeSpan.Zero);
        var fin = new DateTimeOffset(hasta.Date.AddDays(1), TimeSpan.Zero);
        var pageSkip = Math.Max(0, skip);
        var pageTake = Math.Clamp(take, 1, 500);

        var query = dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(movimiento => movimiento.FechaMovimiento >= inicio && movimiento.FechaMovimiento < fin);

        if (bodegaId.HasValue && bodegaId.Value != Guid.Empty)
        {
            query = query.Where(movimiento => movimiento.BodegaId == bodegaId.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(movimiento => movimiento.FechaMovimiento)
            .ThenBy(movimiento => movimiento.Producto.Nombre)
            .Skip(pageSkip)
            .Take(pageTake)
            .Select(movimiento => new KardexValorizadoReporteItemResponse
            {
                FechaMovimiento = movimiento.FechaMovimiento,
                ProductoId = movimiento.ProductoId,
                CodigoProducto = movimiento.Producto.Codigo,
                Producto = movimiento.Producto.Nombre,
                BodegaId = movimiento.BodegaId,
                Bodega = movimiento.Bodega.Nombre,
                TipoMovimiento = movimiento.TipoMovimiento,
                Referencia = movimiento.Referencia,
                CantidadEntrada = movimiento.CantidadEntrada,
                CantidadSalida = movimiento.CantidadSalida,
                CostoUnitario = movimiento.CostoUnitario,
                CostoTotal = movimiento.CostoTotal,
                SaldoCantidad = movimiento.SaldoCantidad,
                SaldoValor = movimiento.SaldoValor
            })
            .ToArrayAsync(cancellationToken);

        return new PagedResultResponse<KardexValorizadoReporteItemResponse>
        {
            Items = items,
            TotalCount = total,
            Skip = pageSkip,
            Take = pageTake
        };
    }

    public async Task<PagedResultResponse<CierreCajaReporteItemResponse>> ObtenerCierresCajaAsync(
        DateTime desde,
        DateTime hasta,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var inicio = new DateTimeOffset(desde.Date, TimeSpan.Zero);
        var fin = new DateTimeOffset(hasta.Date.AddDays(1), TimeSpan.Zero);
        var pageSkip = Math.Max(0, skip);
        var pageTake = Math.Clamp(take, 1, 500);

        var query = dbContext.CajaSesiones
            .AsNoTracking()
            .Where(caja =>
                caja.FechaApertura < fin &&
                (caja.FechaCierre == null || caja.FechaCierre >= inicio));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(caja => caja.FechaApertura)
            .Skip(pageSkip)
            .Take(pageTake)
            .Select(caja => new CierreCajaReporteItemResponse
            {
                CajaSesionId = caja.Id,
                FechaApertura = caja.FechaApertura,
                FechaCierre = caja.FechaCierre,
                UsuarioId = caja.UsuarioId,
                EstadoCaja = caja.EstadoCaja.ToString(),
                MontoApertura = caja.MontoApertura,
                MontoCalculadoTotal = caja.MontoCalculadoTotal,
                MontoDeclaradoTotal = caja.MontoDeclaradoTotal ?? caja.MontoFisicoEfectivoReal + caja.MontoFisicoTarjetaReal + caja.MontoFisicoTransferenciaReal,
                DiferenciaMonto = caja.DiferenciaMonto != 0m ? caja.DiferenciaMonto : caja.Diferencia
            })
            .ToArrayAsync(cancellationToken);

        return new PagedResultResponse<CierreCajaReporteItemResponse>
        {
            Items = items,
            TotalCount = total,
            Skip = pageSkip,
            Take = pageTake
        };
    }
}
