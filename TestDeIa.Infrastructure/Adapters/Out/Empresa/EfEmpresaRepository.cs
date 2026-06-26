using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Empresa;

public sealed class EfEmpresaRepository : IEmpresaRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfEmpresaRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
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
            .FirstOrDefaultAsync(empresa => empresa.Id == tenantContextAccessor.EmpresaId.Value, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyCollection<EmpresaEmisora>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(empresa => empresa.PuntosEmision)
            .OrderByDescending(empresa => empresa.IsActive)
            .ThenBy(empresa => empresa.RazonSocial)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<EmpresaEmisora?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .Include(empresa => empresa.PuntosEmision)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<EmpresaEmisora> SaveAsync(EmpresaEmisora empresa, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.EmpresasEmisoras
            .Include(current => current.PuntosEmision)
            .FirstOrDefaultAsync(current => current.Id == empresa.Id, cancellationToken);

        if (entity is null)
        {
            entity = new EmpresaEmisoraEntity
            {
                Id = empresa.Id,
                OwnerUserId = empresa.OwnerUserId,
                CreatedAt = empresa.CreatedAt
            };
            dbContext.EmpresasEmisoras.Add(entity);
        }

        entity.OwnerUserId = empresa.OwnerUserId;
        entity.RazonSocial = empresa.RazonSocial;
        entity.NombreComercial = empresa.NombreComercial;
        entity.Ruc = empresa.Ruc;
        entity.DireccionMatriz = empresa.DireccionMatriz;
        entity.DireccionEstablecimiento = empresa.DireccionEstablecimiento;
        entity.Establecimiento = empresa.Establecimiento;
        entity.PuntoEmision = empresa.PuntoEmision;
        entity.AmbienteSri = empresa.AmbienteSri;
        entity.ModoDesarrollo = empresa.ModoDesarrollo;
        entity.TipoEmision = empresa.TipoEmision;
        entity.ObligadoContabilidad = empresa.ObligadoContabilidad;
        entity.ContribuyenteEspecial = empresa.ContribuyenteEspecial;
        entity.RegimenRimpe = empresa.RegimenRimpe;
        entity.AgenteRetencionResolucion = empresa.AgenteRetencionResolucion;
        entity.CertificadoNombreArchivo = empresa.CertificadoNombreArchivo;
        entity.CertificadoContenido = empresa.CertificadoContenido;
        entity.CertificadoClave = empresa.CertificadoClave;
        entity.IsActive = empresa.IsActive;
        entity.UpdatedAt = empresa.UpdatedAt;

        entity.PuntosEmision.Clear();
        foreach (var punto in empresa.PuntosEmision)
        {
            entity.PuntosEmision.Add(new EmpresaPuntoEmisionEntity
            {
                Id = punto.Id,
                EmpresaEmisoraId = empresa.Id,
                Establecimiento = punto.Establecimiento,
                PuntoEmision = punto.PuntoEmision,
                DireccionEstablecimiento = punto.DireccionEstablecimiento,
                IsDefault = punto.IsDefault
            });
        }

        if (tenantContextAccessor.UserId.HasValue &&
            !await dbContext.SecurityUserEmpresas.AnyAsync(
                current => current.SecurityUserId == tenantContextAccessor.UserId.Value && current.EmpresaId == entity.Id,
                cancellationToken))
        {
            dbContext.SecurityUserEmpresas.Add(new SecurityUserEmpresaEntity
            {
                SecurityUserId = tenantContextAccessor.UserId.Value,
                EmpresaId = entity.Id,
                IsDefault = !await dbContext.SecurityUserEmpresas.AnyAsync(current => current.SecurityUserId == tenantContextAccessor.UserId.Value, cancellationToken),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
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
                    punto.IsDefault))
                .ToArray(),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
