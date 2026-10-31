using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class EfEstudioMercadoRepository : IEstudioMercadoRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfEstudioMercadoRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<IReadOnlyCollection<EstudioMercadoTopProducto>> GetTopProductosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default)
    {
        var items = await (
            from factura in dbContext.Facturas.AsNoTracking()
            where factura.FechaEmision >= periodoInicio &&
                  factura.FechaEmision < periodoFin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            group new { detalle, producto } by new
            {
                detalle.ProductoId,
                detalle.CodigoProducto,
                detalle.NombreProducto
            } into grouped
            orderby grouped.Sum(current => current.detalle.Cantidad) descending,
                    grouped.Sum(current => current.detalle.Total - (current.detalle.Cantidad * current.producto.CostoPromedio)) descending
            select new EstudioMercadoTopProducto
            {
                ProductoId = grouped.Key.ProductoId,
                Codigo = grouped.Key.CodigoProducto,
                Nombre = grouped.Key.NombreProducto,
                CantidadVendida = Math.Round(grouped.Sum(current => current.detalle.Cantidad), 2, MidpointRounding.AwayFromZero),
                TotalVendido = Math.Round(grouped.Sum(current => current.detalle.Total), 2, MidpointRounding.AwayFromZero),
                CostoEstimado = Math.Round(grouped.Sum(current => current.detalle.Cantidad * current.producto.CostoPromedio), 2, MidpointRounding.AwayFromZero),
                MargenEstimado = Math.Round(grouped.Sum(current => current.detalle.Total - (current.detalle.Cantidad * current.producto.CostoPromedio)), 2, MidpointRounding.AwayFromZero),
                StockActual = 0m
            })
            .Take(take)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return items;
        }

        var productIds = items.Select(current => current.ProductoId).ToArray();
        var stockMap = await dbContext.ProductosBodega
            .AsNoTracking()
            .Where(current => productIds.Contains(current.ProductoId))
            .GroupBy(current => current.ProductoId)
            .Select(group => new
            {
                group.Key,
                StockActual = Math.Round(group.Sum(current => current.StockActual), 2, MidpointRounding.AwayFromZero)
            })
            .ToDictionaryAsync(current => current.Key, current => current.StockActual, cancellationToken);

        return items.Select(item => new EstudioMercadoTopProducto
        {
            ProductoId = item.ProductoId,
            Codigo = item.Codigo,
            Nombre = item.Nombre,
            CantidadVendida = item.CantidadVendida,
            TotalVendido = item.TotalVendido,
            CostoEstimado = item.CostoEstimado,
            MargenEstimado = item.MargenEstimado,
            StockActual = stockMap.GetValueOrDefault(item.ProductoId)
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductoConsumoHistorico>> GetHistorialConsumoAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, IReadOnlyCollection<Guid> productoIds, CancellationToken cancellationToken = default)
    {
        if (productoIds.Count == 0)
        {
            return Array.Empty<ProductoConsumoHistorico>();
        }

        var productos = await dbContext.Productos
            .AsNoTracking()
            .Where(current => productoIds.Contains(current.Id))
            .ToListAsync(cancellationToken);

        var stockMap = await dbContext.ProductosBodega
            .AsNoTracking()
            .Where(current => productoIds.Contains(current.ProductoId))
            .GroupBy(current => current.ProductoId)
            .Select(group => new
            {
                group.Key,
                StockActual = Math.Round(group.Sum(current => current.StockActual), 2, MidpointRounding.AwayFromZero)
            })
            .ToDictionaryAsync(current => current.Key, current => current.StockActual, cancellationToken);

        var movimientos = await (
            from factura in dbContext.Facturas.AsNoTracking()
            where factura.FechaEmision >= periodoInicio &&
                  factura.FechaEmision < periodoFin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            from detalle in factura.Detalles
            where productoIds.Contains(detalle.ProductoId)
            group detalle by new
            {
                detalle.ProductoId,
                Fecha = DateOnly.FromDateTime(factura.FechaEmision.LocalDateTime.Date)
            } into grouped
            select new
            {
                grouped.Key.ProductoId,
                grouped.Key.Fecha,
                Cantidad = Math.Round(grouped.Sum(current => current.Cantidad), 4, MidpointRounding.AwayFromZero)
            })
            .ToListAsync(cancellationToken);

        return productos.Select(producto => new ProductoConsumoHistorico
        {
            ProductoId = producto.Id,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            StockActual = stockMap.GetValueOrDefault(producto.Id),
            CostoPromedio = producto.CostoPromedio,
            ConsumosDiarios = movimientos
                .Where(current => current.ProductoId == producto.Id)
                .OrderBy(current => current.Fecha)
                .Select(current => new ConsumoDiarioHistoricoCompra
                {
                    Fecha = current.Fecha,
                    Cantidad = current.Cantidad
                })
                .ToArray()
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<EstudioMercadoCompra>> GetEstudiosHistoricosAsync(int mes, int anio, int take, CancellationToken cancellationToken = default)
    {
        var periodoActual = (anio * 12) + mes;
        var entities = await dbContext.EstudiosMercadoCompra
            .AsNoTracking()
            .Where(current => ((current.Anio * 12) + current.Mes) < periodoActual)
            .OrderByDescending(current => current.Anio)
            .ThenByDescending(current => current.Mes)
            .Take(take)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task SaveAsync(EstudioMercadoCompra estudio, CancellationToken cancellationToken = default)
    {
        var empresaId = tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa en el contexto actual.");

        var existing = await dbContext.EstudiosMercadoCompra
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Anio == estudio.Anio && current.Mes == estudio.Mes, cancellationToken);

        if (existing is null)
        {
            dbContext.EstudiosMercadoCompra.Add(new EstudioMercadoCompraEntity
            {
                Id = estudio.Id,
                EmpresaId = empresaId,
                Anio = estudio.Anio,
                Mes = estudio.Mes,
                TopProductosVendidosJson = estudio.TopProductosVendidosJson,
                SugerenciasCompraJson = estudio.SugerenciasCompraJson,
                AnalisisEstrategicoIA = estudio.AnalisisEstrategicoIA,
                CreatedAt = estudio.CreatedAt,
                UsuarioCreacionId = estudio.UsuarioCreacionId,
                UpdatedAt = estudio.UpdatedAt,
                UsuarioModificacionId = estudio.UsuarioModificacionId
            });
        }
        else
        {
            existing.TopProductosVendidosJson = estudio.TopProductosVendidosJson;
            existing.SugerenciasCompraJson = estudio.SugerenciasCompraJson;
            existing.AnalisisEstrategicoIA = estudio.AnalisisEstrategicoIA;
            existing.UpdatedAt = estudio.UpdatedAt;
            existing.UsuarioModificacionId = estudio.UsuarioModificacionId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static EstudioMercadoCompra Map(EstudioMercadoCompraEntity entity)
    {
        var topProductos = Deserialize<IReadOnlyCollection<EstudioMercadoTopProducto>>(entity.TopProductosVendidosJson);
        var sugerencias = Deserialize<IReadOnlyCollection<EstudioMercadoSugerenciaCompra>>(entity.SugerenciasCompraJson);

        return new EstudioMercadoCompra(
            entity.Id,
            entity.EmpresaId,
            entity.Anio,
            entity.Mes,
            entity.TopProductosVendidosJson,
            entity.SugerenciasCompraJson,
            entity.AnalisisEstrategicoIA,
            entity.CreatedAt,
            entity.UsuarioCreacionId,
            entity.UpdatedAt,
            entity.UsuarioModificacionId,
            topProductos ?? Array.Empty<EstudioMercadoTopProducto>(),
            sugerencias ?? Array.Empty<EstudioMercadoSugerenciaCompra>());
    }

    private static T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
