using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Reporteria.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Infrastructure.Adapters.Out.Reporteria;

public sealed class EfReporteVentasRepository(TestDeIaDbContext dbContext) : IReporteVentasRepository
{
    public async Task<ReporteVentasResponse> GetVentasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
    {
        var desde = new DateTimeOffset(fechaInicio.Date);
        var hasta = new DateTimeOffset(fechaFin.Date.AddDays(1));
        var diasPeriodo = Math.Max(1, (fechaFin.Date - fechaInicio.Date).Days + 1);

        var facturasQuery = dbContext.Facturas
            .AsNoTracking()
            .Where(factura =>
                factura.FechaEmision >= desde &&
                factura.FechaEmision < hasta &&
                (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE));

        var resumenBase = await facturasQuery
            .GroupBy(_ => 1)
            .Select(grouped => new
            {
                TotalVentas = grouped.Sum(factura => factura.Total),
                Subtotal = grouped.Sum(factura => factura.Subtotal),
                Iva = grouped.Sum(factura => factura.IvaTotal),
                Descuentos = grouped.Sum(factura => factura.TotalDescuento),
                Comprobantes = grouped.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var costoEstimado = await (
            from factura in facturasQuery
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            select detalle.Cantidad * producto.CostoPromedio)
            .SumAsync(costo => (decimal?)costo, cancellationToken) ?? 0m;

        var totalVentas = resumenBase?.TotalVentas ?? 0m;
        var margenEstimado = totalVentas - costoEstimado;
        var promedioDiario = totalVentas / diasPeriodo;
        var margenPromedioDiario = margenEstimado / diasPeriodo;

        var ventasMensuales = await facturasQuery
            .GroupBy(factura => new
            {
                factura.FechaEmision.Year,
                factura.FechaEmision.Month
            })
            .Select(grouped => new ReporteVentasPeriodoResponse
            {
                Anio = grouped.Key.Year,
                Mes = grouped.Key.Month,
                Periodo = $"{grouped.Key.Month.ToString().PadLeft(2, '0')}/{grouped.Key.Year}",
                TotalVentas = Math.Round(grouped.Sum(factura => factura.Total), 2, MidpointRounding.AwayFromZero),
                Comprobantes = grouped.Count()
            })
            .OrderBy(current => current.Anio)
            .ThenBy(current => current.Mes)
            .ToArrayAsync(cancellationToken);

        var topProductos = await (
            from factura in facturasQuery
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            group new { detalle, producto } by new
            {
                detalle.ProductoId,
                detalle.CodigoProducto,
                detalle.NombreProducto
            } into grouped
            orderby grouped.Sum(current => current.detalle.Total) descending
            select new ReporteVentasProductoResponse
            {
                ProductoId = grouped.Key.ProductoId,
                Codigo = grouped.Key.CodigoProducto,
                Producto = grouped.Key.NombreProducto,
                Cantidad = Math.Round(grouped.Sum(current => current.detalle.Cantidad), 2, MidpointRounding.AwayFromZero),
                Total = Math.Round(grouped.Sum(current => current.detalle.Total), 2, MidpointRounding.AwayFromZero),
                MargenEstimado = Math.Round(
                    grouped.Sum(current => current.detalle.Total) -
                    grouped.Sum(current => current.detalle.Cantidad * current.producto.CostoPromedio),
                    2,
                    MidpointRounding.AwayFromZero)
            })
            .Take(12)
            .ToArrayAsync(cancellationToken);

        var ventasPorUsuario = await (
            from factura in facturasQuery
            join usuario in dbContext.SecurityUsers.AsNoTracking() on factura.UsuarioId equals usuario.Id
            group factura by new
            {
                UsuarioId = usuario.Id,
                Usuario = usuario.DisplayName
            } into grouped
            orderby grouped.Sum(current => current.Total) descending
            select new ReporteVentasVendedorResponse
            {
                UsuarioId = grouped.Key.UsuarioId,
                Usuario = grouped.Key.Usuario,
                Comprobantes = grouped.Count(),
                TotalVentas = Math.Round(grouped.Sum(current => current.Total), 2, MidpointRounding.AwayFromZero),
                TicketPromedio = Math.Round(grouped.Sum(current => current.Total) / grouped.Count(), 2, MidpointRounding.AwayFromZero)
            })
            .Take(15)
            .ToArrayAsync(cancellationToken);

        return new ReporteVentasResponse
        {
            FechaInicio = fechaInicio.Date,
            FechaFin = fechaFin.Date,
            Resumen = new ReporteVentasResumenResponse
            {
                TotalVentas = Math.Round(totalVentas, 2, MidpointRounding.AwayFromZero),
                Subtotal = Math.Round(resumenBase?.Subtotal ?? 0m, 2, MidpointRounding.AwayFromZero),
                Iva = Math.Round(resumenBase?.Iva ?? 0m, 2, MidpointRounding.AwayFromZero),
                Descuentos = Math.Round(resumenBase?.Descuentos ?? 0m, 2, MidpointRounding.AwayFromZero),
                Comprobantes = resumenBase?.Comprobantes ?? 0,
                TicketPromedio = resumenBase?.Comprobantes > 0 ? Math.Round(totalVentas / resumenBase.Comprobantes, 2, MidpointRounding.AwayFromZero) : 0m,
                CostoEstimado = Math.Round(costoEstimado, 2, MidpointRounding.AwayFromZero),
                MargenEstimado = Math.Round(margenEstimado, 2, MidpointRounding.AwayFromZero),
                PromedioDiario = Math.Round(promedioDiario, 2, MidpointRounding.AwayFromZero)
            },
            VentasMensuales = ventasMensuales,
            TopProductos = topProductos,
            ProductividadVendedores = ventasPorUsuario,
            Proyecciones =
            [
                BuildProjection("Trimestral", 90, promedioDiario, margenPromedioDiario),
                BuildProjection("Semestral", 180, promedioDiario, margenPromedioDiario),
                BuildProjection("Anual", 365, promedioDiario, margenPromedioDiario)
            ]
        };
    }

    private static ReporteVentasProyeccionResponse BuildProjection(string horizonte, int dias, decimal promedioDiario, decimal margenPromedioDiario)
    {
        return new ReporteVentasProyeccionResponse
        {
            Horizonte = horizonte,
            DiasEstimados = dias,
            TotalProyectado = Math.Round(promedioDiario * dias, 2, MidpointRounding.AwayFromZero),
            MargenProyectado = Math.Round(margenPromedioDiario * dias, 2, MidpointRounding.AwayFromZero)
        };
    }
}
