using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Financiero.Ports.Out;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Financiero.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Financiero;

public sealed class EfFinancieroReportesRepository : IFinancieroReportesRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfFinancieroReportesRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<ConsolidadoIvaMensual> ObtenerConsolidadoIvaAsync(
        int mes,
        int anio,
        string? puntoEmision = null,
        string? cajero = null,
        CancellationToken cancellationToken = default)
    {
        var ventasDetallesQuery =
            from factura in dbContext.Facturas.AsNoTracking()
            where factura.FechaEmision.Year == anio &&
                  factura.FechaEmision.Month == mes &&
                  factura.Estado == FacturaEstado.AUTORIZADO
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            join usuario in dbContext.SecurityUsers.AsNoTracking() on factura.UsuarioId equals usuario.Id into usuarios
            from usuario in usuarios.DefaultIfEmpty()
            select new VentaDetalleFiscalProjection
            {
                FacturaId = factura.Id,
                PuntoEmision = factura.PuntoEmision,
                UsuarioUserName = usuario != null ? usuario.UserName : string.Empty,
                UsuarioDisplayName = usuario != null ? usuario.DisplayName : string.Empty,
                UsuarioIdentificacion = usuario != null ? usuario.Persona.Identificacion : string.Empty,
                ControlaStock = producto.ControlaStock,
                PorcentajeIva = detalle.PorcentajeIva,
                BaseImponible = detalle.Subtotal,
                IvaValor = detalle.IvaValor,
                TotalLinea = detalle.Total
            };

        var ventas = await BuildConsolidadoAsync(ventasDetallesQuery, cancellationToken);
        var ventasBienes = await BuildConsolidadoAsync(ventasDetallesQuery.Where(current => current.ControlaStock), cancellationToken);
        var ventasServicios = await BuildConsolidadoAsync(ventasDetallesQuery.Where(current => !current.ControlaStock), cancellationToken);

        var compras = await dbContext.Compras
            .AsNoTracking()
            .Where(current =>
                current.FechaEmision.Year == anio &&
                current.FechaEmision.Month == mes &&
                current.EstadoCompra == "Registrada")
            .GroupBy(_ => 1)
            .Select(group => new ConsolidadoIvaTarifa
            {
                Base0 = Math.Round(group.Sum(item => item.SubtotalIva0), 2, MidpointRounding.AwayFromZero),
                Base5 = Math.Round(group.Sum(item => item.SubtotalIva5), 2, MidpointRounding.AwayFromZero),
                Base8 = Math.Round(group.Sum(item => item.SubtotalIva8), 2, MidpointRounding.AwayFromZero),
                Base15 = Math.Round(group.Sum(item => item.SubtotalIva15), 2, MidpointRounding.AwayFromZero),
                Iva5 = Math.Round(group.Sum(item => item.SubtotalIva5 * 0.05m), 2, MidpointRounding.AwayFromZero),
                Iva8 = Math.Round(group.Sum(item => item.SubtotalIva8 * 0.08m), 2, MidpointRounding.AwayFromZero),
                Iva15 = Math.Round(group.Sum(item => item.SubtotalIva15 * 0.15m), 2, MidpointRounding.AwayFromZero)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new ConsolidadoIvaTarifa();

        var serviciosOperativosQuery = ventasDetallesQuery.Where(current => !current.ControlaStock);

        var puntoEmisionNormalizado = puntoEmision?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(puntoEmisionNormalizado))
        {
            serviciosOperativosQuery = serviciosOperativosQuery.Where(current => current.PuntoEmision == puntoEmisionNormalizado);
        }

        var cajeroNormalizado = cajero?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(cajeroNormalizado))
        {
            var cajeroUpper = cajeroNormalizado.ToUpperInvariant();
            serviciosOperativosQuery = serviciosOperativosQuery.Where(current =>
                current.UsuarioUserName.ToUpper().Contains(cajeroUpper) ||
                current.UsuarioDisplayName.ToUpper().Contains(cajeroUpper) ||
                current.UsuarioIdentificacion.Contains(cajeroNormalizado));
        }

        var serviciosOperativos = new ConsolidadoIvaServiciosOperativos
        {
            PuntoEmision = puntoEmisionNormalizado,
            Cajero = cajeroNormalizado,
            TotalFacturadoServicios = Math.Round(
                await serviciosOperativosQuery.SumAsync(current => (decimal?)current.TotalLinea, cancellationToken) ?? 0m,
                2,
                MidpointRounding.AwayFromZero),
            FacturasProcesadas = await serviciosOperativosQuery
                .Select(current => current.FacturaId)
                .Distinct()
                .CountAsync(cancellationToken)
        };

        return new ConsolidadoIvaMensual
        {
            Mes = mes,
            Anio = anio,
            Ventas = ventas,
            VentasBienes = ventasBienes,
            VentasServicios = ventasServicios,
            Compras = compras,
            ServiciosOperativos = serviciosOperativos
        };
    }

    public async Task<IReadOnlyCollection<MemoriaAnalisisFiscal>> ObtenerMemoriasHistoricasAsync(int mes, int anio, int cantidad, CancellationToken cancellationToken = default)
    {
        var periodoActual = (anio * 12) + mes;

        var memorias = await dbContext.MemoriasAnalisisFiscal
            .AsNoTracking()
            .Where(current => ((current.Anio * 12) + current.Mes) < periodoActual)
            .OrderByDescending(current => current.Anio)
            .ThenByDescending(current => current.Mes)
            .Take(cantidad)
            .ToListAsync(cancellationToken);

        return memorias
            .Select(Map)
            .ToArray();
    }

    public async Task GuardarMemoriaAnalisisFiscalAsync(MemoriaAnalisisFiscal memoria, CancellationToken cancellationToken = default)
    {
        var empresaId = tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa en el contexto actual.");

        var existing = await dbContext.MemoriasAnalisisFiscal
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.Mes == memoria.Mes &&
                current.Anio == memoria.Anio,
                cancellationToken);

        if (existing is null)
        {
            dbContext.MemoriasAnalisisFiscal.Add(new MemoriaAnalisisFiscalEntity
            {
                Id = memoria.Id,
                EmpresaId = empresaId,
                Mes = memoria.Mes,
                Anio = memoria.Anio,
                ResumenNumericoJson = memoria.ResumenNumericoJson,
                RazonamientoIA = memoria.RazonamientoIA,
                ContextoPrevioUtilizado = memoria.ContextoPrevioUtilizado,
                CreatedAt = memoria.CreatedAt,
                UsuarioCreacionId = memoria.UsuarioCreacionId
            });
        }
        else
        {
            existing.ResumenNumericoJson = memoria.ResumenNumericoJson;
            existing.RazonamientoIA = memoria.RazonamientoIA;
            existing.ContextoPrevioUtilizado = memoria.ContextoPrevioUtilizado;
            existing.UpdatedAt = memoria.UpdatedAt ?? DateTimeOffset.UtcNow;
            existing.UsuarioModificacionId = memoria.UsuarioModificacionId ?? tenantContextAccessor.UserId ?? Guid.Empty;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<ConsolidadoIvaTarifa> BuildConsolidadoAsync(IQueryable<VentaDetalleFiscalProjection> query, CancellationToken cancellationToken)
    {
        return await query
            .GroupBy(_ => 1)
            .Select(group => new ConsolidadoIvaTarifa
            {
                Base0 = Math.Round(group.Sum(item => item.PorcentajeIva == 0m ? item.BaseImponible : 0m), 2, MidpointRounding.AwayFromZero),
                Base5 = Math.Round(group.Sum(item => item.PorcentajeIva == 5m ? item.BaseImponible : 0m), 2, MidpointRounding.AwayFromZero),
                Base8 = Math.Round(group.Sum(item => item.PorcentajeIva == 8m ? item.BaseImponible : 0m), 2, MidpointRounding.AwayFromZero),
                Base15 = Math.Round(group.Sum(item => item.PorcentajeIva == 15m ? item.BaseImponible : 0m), 2, MidpointRounding.AwayFromZero),
                Iva5 = Math.Round(group.Sum(item => item.PorcentajeIva == 5m ? item.IvaValor : 0m), 2, MidpointRounding.AwayFromZero),
                Iva8 = Math.Round(group.Sum(item => item.PorcentajeIva == 8m ? item.IvaValor : 0m), 2, MidpointRounding.AwayFromZero),
                Iva15 = Math.Round(group.Sum(item => item.PorcentajeIva == 15m ? item.IvaValor : 0m), 2, MidpointRounding.AwayFromZero)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new ConsolidadoIvaTarifa();
    }

    private static MemoriaAnalisisFiscal Map(MemoriaAnalisisFiscalEntity entity)
    {
        return new MemoriaAnalisisFiscal
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            Mes = entity.Mes,
            Anio = entity.Anio,
            ResumenNumericoJson = entity.ResumenNumericoJson,
            RazonamientoIA = entity.RazonamientoIA,
            ContextoPrevioUtilizado = entity.ContextoPrevioUtilizado,
            CreatedAt = entity.CreatedAt,
            UsuarioCreacionId = entity.UsuarioCreacionId,
            UpdatedAt = entity.UpdatedAt,
            UsuarioModificacionId = entity.UsuarioModificacionId
        };
    }

    private sealed class VentaDetalleFiscalProjection
    {
        public Guid FacturaId { get; init; }
        public string PuntoEmision { get; init; } = string.Empty;
        public string UsuarioUserName { get; init; } = string.Empty;
        public string UsuarioDisplayName { get; init; } = string.Empty;
        public string UsuarioIdentificacion { get; init; } = string.Empty;
        public bool ControlaStock { get; init; }
        public decimal PorcentajeIva { get; init; }
        public decimal BaseImponible { get; init; }
        public decimal IvaValor { get; init; }
        public decimal TotalLinea { get; init; }
    }
}
