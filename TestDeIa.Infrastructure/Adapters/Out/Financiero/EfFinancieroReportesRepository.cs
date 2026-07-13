using TestDeIa.Application.Common;
using Microsoft.EntityFrameworkCore;
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

    public async Task<ConsolidadoIvaMensual> ObtenerConsolidadoIvaAsync(int mes, int anio, CancellationToken cancellationToken = default)
    {
        var ventas = await dbContext.Facturas
            .AsNoTracking()
            .Where(current =>
                current.FechaEmision.Year == anio &&
                current.FechaEmision.Month == mes &&
                current.Estado == FacturaEstado.AUTORIZADO)
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

        return new ConsolidadoIvaMensual
        {
            Mes = mes,
            Anio = anio,
            Ventas = ventas,
            Compras = compras
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
}
