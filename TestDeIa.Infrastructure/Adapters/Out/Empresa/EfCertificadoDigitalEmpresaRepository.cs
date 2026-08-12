using Microsoft.EntityFrameworkCore;
using System.Data;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Infrastructure.Adapters.Out.Empresa;

public sealed class EfCertificadoDigitalEmpresaRepository(TestDeIaDbContext dbContext) : ICertificadoDigitalEmpresaRepository
{
    public async Task<PagedResultResponse<CertificadoDigitalEmpresa>> GetPagedAsync(
        string? term,
        Guid? empresaId,
        bool soloAlertas,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term?.Trim();
        var alertLimit = DateTimeOffset.UtcNow.AddDays(30);

        var query = dbContext.CertificadosDigitalesEmpresa
            .AsNoTracking()
            .Include(certificado => certificado.Empresa)
            .AsQueryable();

        if (empresaId.HasValue)
        {
            query = query.Where(certificado => certificado.EmpresaId == empresaId.Value);
        }

        if (soloAlertas)
        {
            query = query.Where(certificado => certificado.IsActive && certificado.FechaFinVigencia <= alertLimit);
        }

        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(certificado =>
                certificado.Nombre.Contains(normalizedTerm) ||
                certificado.NombreArchivo.Contains(normalizedTerm) ||
                certificado.Empresa.RazonSocial.Contains(normalizedTerm) ||
                certificado.Empresa.Ruc.Contains(normalizedTerm) ||
                (certificado.HuellaDigital != null && certificado.HuellaDigital.Contains(normalizedTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var certificados = await query
            .OrderByDescending(certificado => certificado.EsPrincipal)
            .ThenBy(certificado => certificado.FechaFinVigencia)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<CertificadoDigitalEmpresa>
        {
            Items = certificados.Select(Map).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<CertificadoDigitalEmpresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificado = await dbContext.CertificadosDigitalesEmpresa
            .AsNoTracking()
            .Include(current => current.Empresa)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return certificado is null ? null : Map(certificado);
    }

    public async Task<IReadOnlyCollection<CertificadoDigitalEmpresa>> GetAlertasAsync(CancellationToken cancellationToken = default)
    {
        var alertLimit = DateTimeOffset.UtcNow.AddDays(30);
        var certificados = await dbContext.CertificadosDigitalesEmpresa
            .AsNoTracking()
            .Include(current => current.Empresa)
            .Where(current => current.IsActive && current.FechaFinVigencia <= alertLimit)
            .OrderBy(current => current.FechaFinVigencia)
            .ToListAsync(cancellationToken);

        return certificados.Select(Map).ToArray();
    }

    public async Task<CertificadoDigitalEmpresa> SaveAsync(CertificadoDigitalEmpresa certificado, bool activarComoPrincipal, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        if (activarComoPrincipal)
        {
            await dbContext.CertificadosDigitalesEmpresa
                .Where(current => current.EmpresaId == certificado.EmpresaId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(current => current.EsPrincipal, false)
                    .SetProperty(current => current.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

        var entity = new CertificadoDigitalEmpresaEntity
        {
            Id = certificado.Id,
            EmpresaId = certificado.EmpresaId,
            Nombre = certificado.Nombre,
            NombreArchivo = certificado.NombreArchivo,
            Contenido = certificado.Contenido,
            Clave = certificado.Clave,
            Sujeto = certificado.Sujeto,
            Emisor = certificado.Emisor,
            NumeroSerie = certificado.NumeroSerie,
            HuellaDigital = certificado.HuellaDigital,
            FechaInicioVigencia = certificado.FechaInicioVigencia,
            FechaFinVigencia = certificado.FechaFinVigencia,
            IsActive = true,
            EsPrincipal = activarComoPrincipal,
            CreatedAt = certificado.CreatedAt,
            UsuarioCreacionId = certificado.UsuarioCreacionId
        };

        dbContext.CertificadosDigitalesEmpresa.Add(entity);

        if (activarComoPrincipal)
        {
            await ApplyPrincipalCertificateAsync(certificado.EmpresaId, entity, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var saved = await dbContext.CertificadosDigitalesEmpresa
            .AsNoTracking()
            .Include(current => current.Empresa)
            .FirstAsync(current => current.Id == entity.Id, cancellationToken);

        return Map(saved);
    }

    public async Task<CertificadoDigitalEmpresa> ActivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var certificado = await dbContext.CertificadosDigitalesEmpresa
            .Include(current => current.Empresa)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el certificado digital.");

        await dbContext.CertificadosDigitalesEmpresa
            .Where(current => current.EmpresaId == certificado.EmpresaId && current.Id != id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(current => current.EsPrincipal, false)
                .SetProperty(current => current.UpdatedAt, DateTimeOffset.UtcNow),
                cancellationToken);

        certificado.IsActive = true;
        certificado.EsPrincipal = true;
        certificado.UpdatedAt = DateTimeOffset.UtcNow;
        await ApplyPrincipalCertificateAsync(certificado.EmpresaId, certificado, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Map(certificado);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificado = await dbContext.CertificadosDigitalesEmpresa
            .Include(current => current.Empresa)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el certificado digital.");

        var wasPrincipal = certificado.EsPrincipal;
        certificado.IsActive = false;
        certificado.EsPrincipal = false;
        certificado.UpdatedAt = DateTimeOffset.UtcNow;

        if (wasPrincipal)
        {
            certificado.Empresa.CertificadoNombreArchivo = null;
            certificado.Empresa.CertificadoContenido = null;
            certificado.Empresa.CertificadoClave = null;
            certificado.Empresa.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyPrincipalCertificateAsync(Guid empresaId, CertificadoDigitalEmpresaEntity certificado, CancellationToken cancellationToken)
    {
        var empresa = await dbContext.EmpresasEmisoras
            .FirstOrDefaultAsync(current => current.Id == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró la empresa asociada al certificado.");

        empresa.CertificadoNombreArchivo = certificado.NombreArchivo;
        empresa.CertificadoContenido = certificado.Contenido;
        empresa.CertificadoClave = certificado.Clave;
        empresa.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static CertificadoDigitalEmpresa Map(CertificadoDigitalEmpresaEntity entity)
    {
        return new CertificadoDigitalEmpresa(
            entity.Id,
            entity.EmpresaId,
            entity.Empresa.RazonSocial,
            entity.Empresa.Ruc,
            entity.Nombre,
            entity.NombreArchivo,
            entity.Contenido,
            entity.Clave,
            entity.Sujeto,
            entity.Emisor,
            entity.NumeroSerie,
            entity.HuellaDigital,
            entity.FechaInicioVigencia,
            entity.FechaFinVigencia,
            entity.IsActive,
            entity.EsPrincipal,
            entity.CreatedAt,
            entity.UsuarioCreacionId,
            entity.UpdatedAt,
            entity.UsuarioModificacionId);
    }
}
