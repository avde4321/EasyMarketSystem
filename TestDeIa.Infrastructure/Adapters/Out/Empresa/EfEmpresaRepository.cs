using Microsoft.EntityFrameworkCore;
using System.Data;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Infrastructure.Adapters.Out.Contabilidad;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Infrastructure.Adapters.Out.Empresa;

public sealed class EfEmpresaRepository : IEmpresaRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;
    private readonly CatalogoNiifSeed catalogoNiifSeed;

    public EfEmpresaRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor, CatalogoNiifSeed catalogoNiifSeed)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
        this.catalogoNiifSeed = catalogoNiifSeed;
    }

    public async Task<EmpresaEmisora?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            return null;
        }

        var entity = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(empresa => empresa.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .FirstOrDefaultAsync(empresa => empresa.Id == tenantContextAccessor.EmpresaId.Value, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyCollection<EmpresaEmisora>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(empresa => empresa.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .OrderByDescending(empresa => empresa.IsActive)
            .ThenBy(empresa => empresa.RazonSocial)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<PagedResultResponse<EmpresaEmisora>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term?.Trim();
        var query = dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(empresa => empresa.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(empresa =>
                empresa.Ruc.Contains(normalizedTerm) ||
                empresa.RazonSocial.Contains(normalizedTerm) ||
                (empresa.NombreComercial != null && empresa.NombreComercial.Contains(normalizedTerm)) ||
                empresa.AmbienteSri.Contains(normalizedTerm) ||
                empresa.PuntosEmision.Any(punto =>
                    punto.Establecimiento.Contains(normalizedTerm) ||
                    punto.PuntoEmision.Contains(normalizedTerm) ||
                    (punto.DireccionEstablecimiento != null && punto.DireccionEstablecimiento.Contains(normalizedTerm))));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(empresa => empresa.IsActive)
            .ThenBy(empresa => empresa.RazonSocial)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<EmpresaEmisora>
        {
            Items = entities.Select(Map).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<EmpresaEmisora?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(empresa => empresa.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<EmpresaEmisora> SaveAsync(EmpresaEmisora empresa, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await dbContext.EmpresasEmisoras
            .FirstOrDefaultAsync(current => current.Id == empresa.Id, cancellationToken);

        var isNewEmpresa = entity is null;
        if (isNewEmpresa)
        {
            entity = new EmpresaEmisoraEntity
            {
                Id = empresa.Id,
                OwnerUserId = empresa.OwnerUserId,
                CreatedAt = empresa.CreatedAt
            };
            dbContext.EmpresasEmisoras.Add(entity);
        }

        var persistedEntity = entity ?? throw new InvalidOperationException("No se pudo inicializar la empresa emisora.");

        persistedEntity.OwnerUserId = empresa.OwnerUserId;
        persistedEntity.RazonSocial = empresa.RazonSocial;
        persistedEntity.NombreComercial = empresa.NombreComercial;
        persistedEntity.Ruc = empresa.Ruc;
        persistedEntity.DireccionMatriz = empresa.DireccionMatriz;
        persistedEntity.RegionCodigo = empresa.RegionCodigo;
        persistedEntity.ProvinciaCodigo = empresa.ProvinciaCodigo;
        persistedEntity.CiudadCodigo = empresa.CiudadCodigo;
        persistedEntity.SectorCodigo = empresa.SectorCodigo;
        persistedEntity.DireccionEstablecimiento = empresa.DireccionEstablecimiento;
        persistedEntity.Establecimiento = empresa.Establecimiento;
        persistedEntity.PuntoEmision = empresa.PuntoEmision;
        persistedEntity.AmbienteSri = empresa.AmbienteSri;
        persistedEntity.ModoDesarrollo = empresa.ModoDesarrollo;
        persistedEntity.TipoEmision = empresa.TipoEmision;
        persistedEntity.ObligadoContabilidad = empresa.ObligadoContabilidad;
        persistedEntity.ContribuyenteEspecial = empresa.ContribuyenteEspecial;
        persistedEntity.RegimenRimpe = empresa.RegimenRimpe;
        persistedEntity.AgenteRetencionResolucion = empresa.AgenteRetencionResolucion;
        persistedEntity.CertificadoNombreArchivo = empresa.CertificadoNombreArchivo;
        persistedEntity.CertificadoContenido = empresa.CertificadoContenido;
        persistedEntity.CertificadoClave = empresa.CertificadoClave;
        persistedEntity.IsActive = empresa.IsActive;
        persistedEntity.UpdatedAt = empresa.UpdatedAt;

        if (tenantContextAccessor.UserId.HasValue &&
            !await dbContext.SecurityUserEmpresas.AnyAsync(
                current => current.SecurityUserId == tenantContextAccessor.UserId.Value && current.EmpresaId == persistedEntity.Id,
                cancellationToken))
        {
            dbContext.SecurityUserEmpresas.Add(new SecurityUserEmpresaEntity
            {
                SecurityUserId = tenantContextAccessor.UserId.Value,
                EmpresaId = persistedEntity.Id,
                IsDefault = !await dbContext.SecurityUserEmpresas.AnyAsync(current => current.SecurityUserId == tenantContextAccessor.UserId.Value, cancellationToken),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.EmpresaPuntosEmision
            .Where(current => current.EmpresaEmisoraId == persistedEntity.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (empresa.PuntosEmision.Count > 0)
        {
            var points = new List<EmpresaPuntoEmisionEntity>();
            foreach (var punto in empresa.PuntosEmision)
            {
                points.Add(new EmpresaPuntoEmisionEntity
                {
                    Id = punto.Id == Guid.Empty ? Guid.NewGuid() : punto.Id,
                    EmpresaEmisoraId = persistedEntity.Id,
                    BodegaId = await ResolvePuntoBodegaIdAsync(persistedEntity.Id, punto.BodegaId, cancellationToken),
                    Establecimiento = punto.Establecimiento,
                    PuntoEmision = punto.PuntoEmision,
                    DireccionEstablecimiento = punto.DireccionEstablecimiento,
                    IsDefault = punto.IsDefault
                });
            }

            dbContext.EmpresaPuntosEmision.AddRange(points);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (isNewEmpresa && !await dbContext.CuentasContables.AnyAsync(current => current.EmpresaId == persistedEntity.Id, cancellationToken))
        {
            dbContext.CuentasContables.AddRange(catalogoNiifSeed.BuildForEmpresa(persistedEntity.Id));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        var persisted = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(current => current.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .FirstAsync(current => current.Id == persistedEntity.Id, cancellationToken);

        return Map(persisted);
    }

    private static EmpresaEmisora Map(EmpresaEmisoraEntity entity)
    {
        return new EmpresaEmisora(
            entity.Id,
            entity.OwnerUserId,
            entity.RazonSocial,
            entity.NombreComercial,
            entity.Ruc,
            entity.DireccionMatriz,
            entity.RegionCodigo,
            entity.ProvinciaCodigo,
            entity.CiudadCodigo,
            entity.SectorCodigo,
            entity.DireccionEstablecimiento,
            entity.Establecimiento,
            entity.PuntoEmision,
            entity.AmbienteSri,
            entity.ModoDesarrollo,
            entity.TipoEmision,
            entity.ObligadoContabilidad,
            entity.ContribuyenteEspecial,
            entity.RegimenRimpe,
            entity.AgenteRetencionResolucion,
            entity.CertificadoNombreArchivo,
            entity.CertificadoContenido,
            entity.CertificadoClave,
            entity.PuntosEmision
                .OrderByDescending(punto => punto.IsDefault)
                .ThenBy(punto => punto.Establecimiento)
                .ThenBy(punto => punto.PuntoEmision)
                .Select(punto => new EmpresaPuntoEmision(
                    punto.Id,
                    entity.Id,
                    punto.Establecimiento,
                    punto.PuntoEmision,
                    punto.DireccionEstablecimiento,
                    punto.IsDefault,
                    punto.BodegaId,
                    punto.Bodega.Nombre))
                .ToArray(),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private async Task<Guid> ResolvePuntoBodegaIdAsync(Guid empresaId, Guid? requestedBodegaId, CancellationToken cancellationToken)
    {
        if (requestedBodegaId.HasValue && requestedBodegaId.Value != Guid.Empty)
        {
            var requestedBodega = await dbContext.Bodegas
                .FirstOrDefaultAsync(current => current.Id == requestedBodegaId.Value && current.EmpresaId == empresaId, cancellationToken)
                ?? throw new InvalidOperationException("La bodega seleccionada no pertenece a la empresa.");

            if (!requestedBodega.IsActive)
            {
                throw new InvalidOperationException("La bodega seleccionada no se encuentra activa.");
            }

            return requestedBodega.Id;
        }

        var principal = await dbContext.Bodegas
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.IsActive &&
                current.Nombre == "Principal",
                cancellationToken);

        if (principal is not null)
        {
            return principal.Id;
        }

        var firstActive = await dbContext.Bodegas
            .Where(current => current.EmpresaId == empresaId && current.IsActive)
            .OrderBy(current => current.Nombre)
            .FirstOrDefaultAsync(cancellationToken);

        if (firstActive is not null)
        {
            return firstActive.Id;
        }

        var nuevaPrincipal = new BodegaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Codigo = "001",
            Nombre = "Principal",
            Direccion = null,
            EsPrincipal = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Bodegas.Add(nuevaPrincipal);
        await dbContext.SaveChangesAsync(cancellationToken);
        return nuevaPrincipal.Id;
    }
}
