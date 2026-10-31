using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.ActivosFijos.Ports.Out;
using TestDeIa.Domain.Modules.ActivosFijos.Entities;
using TestDeIa.Domain.Modules.ActivosFijos.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.ActivosFijos;

public sealed class EfActivoFijoRepository(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor) : IActivoFijoRepository
{
    public async Task<IReadOnlyCollection<ActivoFijo>> GetAsync(CategoriaSriActivoFijo? categoria, string? custodio, EstadoActivoFijo? estado, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ActivosFijos.AsNoTracking();

        if (categoria.HasValue)
        {
            query = query.Where(current => current.CategoriaSRI == categoria.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(current => current.EstadoActivo == estado.Value);
        }

        if (!string.IsNullOrWhiteSpace(custodio))
        {
            var normalized = custodio.Trim();
            query = query.Where(current => current.CustodioResponsable != null && current.CustodioResponsable.Contains(normalized));
        }

        var entities = await query
            .OrderBy(current => current.CodigoActivo)
            .ToArrayAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<ActivoFijo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ActivosFijos.AsNoTracking().FirstOrDefaultAsync(current => current.Id == id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<ActivoFijo> CreateAsync(ActivoFijo activoFijo, CancellationToken cancellationToken = default)
    {
        var entity = BuildEntity(activoFijo);
        dbContext.ActivosFijos.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ActivoFijo?> UpdateAsync(ActivoFijo activoFijo, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ActivosFijos.FirstOrDefaultAsync(current => current.Id == activoFijo.Id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Nombre = activoFijo.Nombre;
        entity.SerieMarca = activoFijo.SerieMarca;
        entity.CategoriaSRI = activoFijo.CategoriaSRI;
        entity.FechaAdquisicion = activoFijo.FechaAdquisicion;
        entity.CostoInicial = activoFijo.CostoInicial;
        entity.ValorResidual = activoFijo.ValorResidual;
        entity.VidaUtilAnios = activoFijo.VidaUtilAnios;
        entity.PorcentajeDepreciacionAnual = activoFijo.PorcentajeDepreciacionAnual;
        entity.UbicacionFisica = activoFijo.UbicacionFisica;
        entity.CustodioResponsable = activoFijo.CustodioResponsable;
        entity.EstadoActivo = activoFijo.EstadoActivo;
        entity.UpdatedAt = activoFijo.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public Task<bool> ExistsByCompraDetalleIdAsync(Guid compraDetalleId, CancellationToken cancellationToken = default)
    {
        return dbContext.ActivosFijos.AnyAsync(current => current.CompraDetalleId == compraDetalleId, cancellationToken);
    }

    public async Task<string> ReserveNextCodigoAsync(DateTime fechaAdquisicion, CancellationToken cancellationToken = default)
    {
        var empresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para generar codigo de activo fijo.");
        var prefix = $"AF-{fechaAdquisicion.Year}-";
        var count = await dbContext.ActivosFijos
            .IgnoreQueryFilters()
            .CountAsync(current => current.EmpresaId == empresaId && current.CodigoActivo.StartsWith(prefix), cancellationToken);

        return $"{prefix}{count + 1:000}";
    }

    private static ActivoFijoEntity BuildEntity(ActivoFijo activo) => new()
    {
        Id = activo.Id,
        EmpresaId = activo.EmpresaId,
        CompraDetalleId = activo.CompraDetalleId,
        CodigoActivo = activo.CodigoActivo,
        Nombre = activo.Nombre,
        SerieMarca = activo.SerieMarca,
        CategoriaSRI = activo.CategoriaSRI,
        FechaAdquisicion = activo.FechaAdquisicion,
        CostoInicial = activo.CostoInicial,
        ValorResidual = activo.ValorResidual,
        VidaUtilAnios = activo.VidaUtilAnios,
        PorcentajeDepreciacionAnual = activo.PorcentajeDepreciacionAnual,
        UbicacionFisica = activo.UbicacionFisica,
        CustodioResponsable = activo.CustodioResponsable,
        EstadoActivo = activo.EstadoActivo,
        CreatedAt = activo.CreatedAt,
        UpdatedAt = activo.UpdatedAt
    };

    private static ActivoFijo Map(ActivoFijoEntity entity) => new(
        entity.Id,
        entity.EmpresaId,
        entity.CompraDetalleId,
        entity.CodigoActivo,
        entity.Nombre,
        entity.SerieMarca,
        entity.CategoriaSRI,
        entity.FechaAdquisicion,
        entity.CostoInicial,
        entity.ValorResidual,
        entity.VidaUtilAnios,
        entity.PorcentajeDepreciacionAnual,
        entity.UbicacionFisica,
        entity.CustodioResponsable,
        entity.EstadoActivo,
        entity.CreatedAt,
        entity.UpdatedAt);
}
