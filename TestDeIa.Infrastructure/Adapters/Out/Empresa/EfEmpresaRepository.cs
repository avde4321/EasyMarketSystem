using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Empresa;

public sealed class EfEmpresaRepository : IEmpresaRepository
{
    private readonly TestDeIaDbContext dbContext;

    public EfEmpresaRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<EmpresaEmisora?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .OrderByDescending(empresa => empresa.IsActive)
            .ThenByDescending(empresa => empresa.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<EmpresaEmisora> UpsertAsync(EmpresaEmisora empresa, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.EmpresasEmisoras
            .FirstOrDefaultAsync(current => current.Id == empresa.Id, cancellationToken);

        if (entity is null)
        {
            entity = new EmpresaEmisoraEntity
            {
                Id = empresa.Id,
                CreatedAt = empresa.CreatedAt
            };
            dbContext.EmpresasEmisoras.Add(entity);
        }

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

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private static EmpresaEmisora Map(EmpresaEmisoraEntity entity)
    {
        return new EmpresaEmisora(
            entity.Id,
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
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
