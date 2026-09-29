using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Reporteria.Ports.In;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Inventario;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Reporteria;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Reporteria;

public sealed class DashboardService(TestDeIaDbContext dbContext) : IDashboardService
{
    public async Task<DashboardVentasDiariasResponse> ObtenerResumenVentasDiariasAsync(Guid empresaId, DateTime fecha, CancellationToken cancellationToken = default)
    {
        var desde = new DateTimeOffset(fecha.Date, TimeSpan.Zero);
        var hasta = desde.AddDays(1);

        var resumen = await dbContext.Facturas
            .AsNoTracking()
            .Where(factura =>
                factura.EmpresaId == empresaId &&
                factura.FechaEmision >= desde &&
                factura.FechaEmision < hasta &&
                (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE))
            .GroupBy(_ => 1)
            .Select(group => new DashboardVentasDiariasResponse
            {
                Fecha = fecha.Date,
                VentaNeta = group.Sum(factura => factura.Subtotal),
                IvaTotal = group.Sum(factura => factura.IvaTotal),
                TotalFacturado = group.Sum(factura => factura.Total),
                Comprobantes = group.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return resumen ?? new DashboardVentasDiariasResponse { Fecha = fecha.Date };
    }

    public async Task<IReadOnlyCollection<TopProductoRentabilidadResponse>> ObtenerTopProductosMasVendidosAsync(Guid empresaId, int top, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        var inicio = new DateTimeOffset(desde.Date, TimeSpan.Zero);
        var fin = new DateTimeOffset(hasta.Date.AddDays(1), TimeSpan.Zero);
        var take = Math.Clamp(top, 1, 100);

        var rows = await (
            from factura in dbContext.Facturas.AsNoTracking()
            where factura.EmpresaId == empresaId &&
                  factura.FechaEmision >= inicio &&
                  factura.FechaEmision < fin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            group new { detalle, producto } by new
            {
                detalle.ProductoId,
                detalle.CodigoProducto,
                detalle.NombreProducto,
                producto.CategoriaId
            } into grouped
            orderby grouped.Sum(item => item.detalle.Cantidad) descending
            select new TopProductoRentabilidadResponse
            {
                ProductoId = grouped.Key.ProductoId,
                Codigo = grouped.Key.CodigoProducto,
                Producto = grouped.Key.NombreProducto,
                Categoria = grouped.Key.CategoriaId.HasValue ? grouped.Key.CategoriaId.Value.ToString() : "Sin categoria",
                CantidadVendida = grouped.Sum(item => item.detalle.Cantidad),
                VentaNeta = grouped.Sum(item => item.detalle.Subtotal),
                CostoHistorico = grouped.Sum(item => item.detalle.CostoHistoricoTotal),
                UtilidadBruta = grouped.Sum(item => item.detalle.Subtotal) - grouped.Sum(item => item.detalle.CostoHistoricoTotal),
                MargenPorcentaje = grouped.Sum(item => item.detalle.Subtotal) == 0m
                    ? 0m
                    : ((grouped.Sum(item => item.detalle.Subtotal) - grouped.Sum(item => item.detalle.CostoHistoricoTotal)) / grouped.Sum(item => item.detalle.Subtotal)) * 100m
            })
            .Take(take)
            .ToArrayAsync(cancellationToken);

        return rows;
    }

    public async Task<IReadOnlyCollection<VentaBodegaMetodoPagoResponse>> ObtenerVentasPorBodegaYMetodoPagoAsync(Guid empresaId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        var inicio = new DateTimeOffset(desde.Date, TimeSpan.Zero);
        var fin = new DateTimeOffset(hasta.Date.AddDays(1), TimeSpan.Zero);

        return await (
            from pago in dbContext.FacturaPagos.AsNoTracking()
            join factura in dbContext.Facturas.AsNoTracking() on pago.FacturaId equals factura.Id
            join bodega in dbContext.Bodegas.AsNoTracking() on factura.BodegaId equals bodega.Id
            where pago.EmpresaId == empresaId &&
                  factura.FechaEmision >= inicio &&
                  factura.FechaEmision < fin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            group new { pago, bodega } by new
            {
                bodega.Id,
                bodega.Nombre,
                pago.FormaPagoCodigo
            } into grouped
            orderby grouped.Sum(item => item.pago.Monto) descending
            select new VentaBodegaMetodoPagoResponse
            {
                BodegaId = grouped.Key.Id,
                Bodega = grouped.Key.Nombre,
                FormaPagoCodigo = grouped.Key.FormaPagoCodigo,
                FormaPago = SriCatalogCodes.GetFormaPagoName(grouped.Key.FormaPagoCodigo),
                Total = grouped.Sum(item => item.pago.Monto),
                Transacciones = grouped.Count()
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<KpisRentabilidadResponse> ObtenerKpisRentabilidadAsync(Guid empresaId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        var inicio = new DateTimeOffset(desde.Date, TimeSpan.Zero);
        var fin = new DateTimeOffset(hasta.Date.AddDays(1), TimeSpan.Zero);

        var ventaNeta = await dbContext.FacturaDetalles
            .AsNoTracking()
            .Where(detalle =>
                detalle.Factura.EmpresaId == empresaId &&
                detalle.Factura.FechaEmision >= inicio &&
                detalle.Factura.FechaEmision < fin &&
                (detalle.Factura.Estado == FacturaEstado.AUTORIZADO || detalle.Factura.Estado == FacturaEstado.PENDIENTE))
            .SumAsync(detalle => (decimal?)detalle.Subtotal, cancellationToken) ?? 0m;

        var costoDetalle = await dbContext.FacturaDetalles
            .AsNoTracking()
            .Where(detalle =>
                detalle.Factura.EmpresaId == empresaId &&
                detalle.Factura.FechaEmision >= inicio &&
                detalle.Factura.FechaEmision < fin &&
                (detalle.Factura.Estado == FacturaEstado.AUTORIZADO || detalle.Factura.Estado == FacturaEstado.PENDIENTE))
            .SumAsync(detalle => (decimal?)detalle.CostoHistoricoTotal, cancellationToken) ?? 0m;

        var costoKardex = await dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(movimiento =>
                movimiento.EmpresaId == empresaId &&
                movimiento.FechaMovimiento >= inicio &&
                movimiento.FechaMovimiento < fin &&
                movimiento.TipoMovimiento == TipoMovimientoInventario.SalidaVenta)
            .SumAsync(movimiento => (decimal?)movimiento.CostoTotal, cancellationToken) ?? 0m;

        var costoVentas = costoDetalle > 0m ? costoDetalle : costoKardex;
        var utilidad = ventaNeta - costoVentas;

        return new KpisRentabilidadResponse
        {
            VentaNeta = Math.Round(ventaNeta, 2),
            Devoluciones = 0m,
            CostoVentas = Math.Round(costoVentas, 2),
            UtilidadBruta = Math.Round(utilidad, 2),
            MargenPorcentaje = ventaNeta == 0m ? 0m : Math.Round((utilidad / ventaNeta) * 100m, 2)
        };
    }
}
