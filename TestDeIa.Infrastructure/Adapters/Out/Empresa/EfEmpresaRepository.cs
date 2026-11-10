using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
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
    private readonly TestDeIa.Application.Modules.Saas.Ports.In.ISaaSQuotaValidationService quota;
    private readonly TestDeIa.Application.Modules.Saas.Ports.In.ITenantOnboardingService onboarding;

    public EfEmpresaRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor, CatalogoNiifSeed catalogoNiifSeed,
        TestDeIa.Application.Modules.Saas.Ports.In.ISaaSQuotaValidationService? quota = null,
        TestDeIa.Application.Modules.Saas.Ports.In.ITenantOnboardingService? onboarding = null)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
        this.catalogoNiifSeed = catalogoNiifSeed;
        this.quota = quota ?? new TestDeIa.Infrastructure.Adapters.Out.Saas.SaaSQuotaValidationService(dbContext);
        this.onboarding = onboarding ?? new TestDeIa.Infrastructure.Adapters.Out.Saas.TenantOnboardingService(dbContext, this.quota, new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
    }

    public async Task<EmpresaEmisora?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            return null;
        }

        var entity = await AuthorizedCompanyReadQuery()
            .AsNoTracking()
            .Include(empresa => empresa.CertificadosDigitales)
            .Include(empresa => empresa.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .FirstOrDefaultAsync(empresa => empresa.Id == tenantContextAccessor.EmpresaId.Value, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyCollection<EmpresaEmisora>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var entities = await AuthorizedCompanyReadQuery()
            .AsNoTracking()
            .Include(empresa => empresa.CertificadosDigitales)
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
        var query = AuthorizedCompanyReadQuery()
            .AsNoTracking()
            .Include(empresa => empresa.CertificadosDigitales)
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
        var entity = await AuthorizedCompanyReadQuery()
            .AsNoTracking()
            .Include(empresa => empresa.CertificadosDigitales)
            .Include(empresa => empresa.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    /// <summary>
    /// Persiste la empresa y sus puntos dentro del alcance del propietario/tenant. Los bypasses se
    /// limitan por EmpresaId, excepto la validación global de RUC, que devuelve únicamente si existe.
    /// </summary>
    public async Task<EmpresaEmisora> SaveAsync(EmpresaEmisora empresa, CancellationToken cancellationToken = default)
    {
        // El RUC es único en toda la plataforma. Consultar solo existencia evita exponer datos de otros tenants.
        var rucRegistrado = await dbContext.EmpresasEmisoras
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(current => current.Ruc == empresa.Ruc && current.Id != empresa.Id, cancellationToken);
        if (rucRegistrado)
        {
            throw new InvalidOperationException("El RUC ya está registrado. Para modificar una empresa existente, utiliza la opción Editar; si no tienes acceso, contacta al administrador.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await quota.BloquearCuotaAsync(empresa.Id, cancellationToken);

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
        if (empresa.CertificadoContenido is { Length: > 0 } && !string.IsNullOrWhiteSpace(empresa.CertificadoClave))
        {
            using var certificate = X509CertificateLoader.LoadPkcs12(
                empresa.CertificadoContenido, empresa.CertificadoClave, X509KeyStorageFlags.EphemeralKeySet);
            var certificados = await dbContext.CertificadosDigitalesEmpresa
                .Where(current => current.EmpresaId == empresa.Id).ToListAsync(cancellationToken);
            var existente = certificados.FirstOrDefault(current => current.HuellaDigital == certificate.Thumbprint);
            foreach (var current in certificados)
            {
                current.EsPrincipal = false;
            }
            if (existente is null)
            {
                existente = new CertificadoDigitalEmpresaEntity
                {
                    Id = Guid.NewGuid(), EmpresaId = empresa.Id,
                    Nombre = $"Certificado {certificate.NotAfter:yyyy}",
                    NombreArchivo = empresa.CertificadoNombreArchivo ?? "certificado.p12",
                    Contenido = empresa.CertificadoContenido, Clave = empresa.CertificadoClave,
                    Sujeto = certificate.Subject, Emisor = certificate.Issuer,
                    NumeroSerie = certificate.SerialNumber, HuellaDigital = certificate.Thumbprint,
                    FechaInicioVigencia = new DateTimeOffset(certificate.NotBefore.ToUniversalTime()),
                    FechaFinVigencia = new DateTimeOffset(certificate.NotAfter.ToUniversalTime()),
                    CreatedAt = DateTimeOffset.UtcNow, UsuarioCreacionId = tenantContextAccessor.UserId
                };
                dbContext.CertificadosDigitalesEmpresa.Add(existente);
            }
            existente.IsActive = true;
            existente.EsPrincipal = true;
        }
        persistedEntity.CertificadoNombreArchivo = null;
        persistedEntity.CertificadoContenido = null;
        persistedEntity.CertificadoClave = null;
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

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException sqlException &&
            sqlException.Errors.Cast<SqlError>().Any(error =>
                error.Number is 2601 or 2627 &&
                error.Message.Contains("IX_EmpresasEmisoras_Ruc", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("El RUC ya está registrado. Revisa la empresa existente o contacta al administrador.", exception);
        }

        if (isNewEmpresa)
            await onboarding.AsignarSuscripcionInicialAsync(persistedEntity.Id, persistedEntity.OwnerUserId, empresa.PlanInicialId, cancellationToken);

        var existingPoints = await dbContext.EmpresaPuntosEmision
            .IgnoreQueryFilters()
            .Where(current => current.EmpresaEmisoraId == persistedEntity.Id)
            .ToListAsync(cancellationToken);
        var retainedIds = empresa.PuntosEmision.Select(point => point.Id).ToHashSet();
        foreach (var removed in existingPoints.Where(point => !retainedIds.Contains(point.Id)))
        {
            var hasHistory = await dbContext.CajaSesiones.IgnoreQueryFilters()
                .AnyAsync(caja => caja.EmpresaId == persistedEntity.Id && caja.PuntoEmisionId == removed.Id, cancellationToken);
            var assigned = await dbContext.Set<SecurityUserPuntoEmisionEntity>().IgnoreQueryFilters()
                .AnyAsync(link => link.EmpresaId == persistedEntity.Id && link.EmpresaPuntoEmisionId == removed.Id, cancellationToken);
            if (hasHistory || assigned)
                throw new InvalidOperationException($"No puedes quitar el punto {removed.Establecimiento}-{removed.PuntoEmision}: tiene cajas o usuarios asociados. Conserva el punto y edita sus datos.");
            dbContext.EmpresaPuntosEmision.Remove(removed);
        }

        foreach (var punto in empresa.PuntosEmision)
        {
            var point = existingPoints.FirstOrDefault(current => current.Id == punto.Id);
            if (point is null)
            {
                await quota.CanCrearSucursalAsync(persistedEntity.Id, cancellationToken);
                if (await dbContext.EmpresaPuntosEmision.IgnoreQueryFilters()
                    .AnyAsync(current => current.Id == punto.Id && current.EmpresaEmisoraId != persistedEntity.Id, cancellationToken))
                    throw new InvalidOperationException("El punto de emisión no pertenece a esta empresa.");
                point = new EmpresaPuntoEmisionEntity
                {
                    Id = punto.Id == Guid.Empty ? Guid.NewGuid() : punto.Id,
                    EmpresaEmisoraId = persistedEntity.Id
                };
                dbContext.EmpresaPuntosEmision.Add(point);
            }
            var bodegaId = await ResolvePuntoBodegaIdAsync(persistedEntity.Id, punto.BodegaId, cancellationToken);
            if (point.BodegaId != bodegaId && await dbContext.CajaSesiones.IgnoreQueryFilters()
                .AnyAsync(caja => caja.EmpresaId == persistedEntity.Id && caja.PuntoEmisionId == point.Id &&
                    caja.EstadoCaja == TestDeIa.Domain.Modules.Caja.Entities.CajaEstado.Abierta, cancellationToken))
                throw new InvalidOperationException("Cierra la caja de este punto antes de cambiar su bodega.");
            point.BodegaId = bodegaId;
            point.Establecimiento = punto.Establecimiento;
            point.PuntoEmision = punto.PuntoEmision;
            point.DireccionEstablecimiento = punto.DireccionEstablecimiento;
            point.IsDefault = punto.IsDefault;
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        if (isNewEmpresa && !await dbContext.CuentasContables.AnyAsync(current => current.EmpresaId == persistedEntity.Id, cancellationToken))
        {
            dbContext.CuentasContables.AddRange(catalogoNiifSeed.BuildForEmpresa(persistedEntity.Id));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        var persisted = await AuthorizedCompanyReadQuery()
            .AsNoTracking()
            .Include(current => current.CertificadosDigitales)
            .Include(current => current.PuntosEmision)
            .ThenInclude(punto => punto.Bodega)
            .FirstAsync(current => current.Id == persistedEntity.Id, cancellationToken);

        return Map(persisted);
    }

    /// <summary>Lista empresas propias o explícitamente vinculadas al actor; la consulta global nunca se devuelve sin validar propietario/rol/vínculo.</summary>
    private IQueryable<EmpresaEmisoraEntity> AuthorizedCompanyReadQuery()
    {
        // Solo para administración: los Includes deben leer la bodega de cada empresa autorizada,
        // aunque no sea la activa. La autorización se aplica explícitamente en la consulta raíz.
        var userId = tenantContextAccessor.UserId;
        return dbContext.EmpresasEmisoras
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(company => userId.HasValue &&
                (company.OwnerUserId == userId.Value ||
                 (!dbContext.SecurityUserRoles.IgnoreQueryFilters()
                      .Any(role => role.UserId == userId.Value && role.Role.IsActive &&
                                   role.Role.Name == TestDeIa.Shared.Security.SecurityRoleNames.SuperAdministrador) &&
                  dbContext.SecurityUserEmpresas.IgnoreQueryFilters()
                      .Any(access => access.EmpresaId == company.Id && access.SecurityUserId == userId.Value))));
    }

    private static EmpresaEmisora Map(EmpresaEmisoraEntity entity)
    {
        var certificado = entity.CertificadosDigitales.FirstOrDefault(current => current.IsActive && current.EsPrincipal);
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
            certificado?.NombreArchivo ?? entity.CertificadoNombreArchivo,
            certificado?.Contenido ?? entity.CertificadoContenido,
            certificado?.Clave ?? entity.CertificadoClave,
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
                    punto.Bodega?.Nombre))
                .ToArray(),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    /// <summary>Resuelve bodega del punto de emisión y rechaza cualquier bodega cuyo EmpresaId no coincida.</summary>
    private async Task<Guid?> ResolvePuntoBodegaIdAsync(Guid empresaId, Guid? requestedBodegaId, CancellationToken cancellationToken)
    {
        if (requestedBodegaId.HasValue && requestedBodegaId.Value != Guid.Empty)
        {
            var requestedBodega = await dbContext.Bodegas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(current => current.Id == requestedBodegaId.Value && current.EmpresaId == empresaId, cancellationToken)
                ?? throw new InvalidOperationException("La bodega seleccionada no pertenece a la empresa.");

            if (!requestedBodega.IsActive)
            {
                throw new InvalidOperationException("La bodega seleccionada no se encuentra activa.");
            }

            return requestedBodega.Id;
        }

        return null;
    }
}
