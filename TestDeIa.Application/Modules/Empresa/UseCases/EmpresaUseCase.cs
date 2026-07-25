using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empresa;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Application.Modules.Empresa.UseCases;

public sealed class EmpresaUseCase : IEmpresaUseCase
{
    private readonly IEmpresaRepository empresaRepository;
    private readonly ICatalogoRepository catalogoRepository;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public EmpresaUseCase(
        IEmpresaRepository empresaRepository,
        ICatalogoRepository catalogoRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.empresaRepository = empresaRepository;
        this.catalogoRepository = catalogoRepository;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<EmpresaResponse?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var empresa = await empresaRepository.GetCurrentAsync(cancellationToken);
        return empresa is null ? null : MapResponse(empresa);
    }

    public async Task<IReadOnlyCollection<EmpresaOptionResponse>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await empresaRepository.GetMineAsync(cancellationToken);
        return empresas.Select(MapOption).ToArray();
    }

    public async Task<PagedResultResponse<EmpresaResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var page = await empresaRepository.GetPagedAsync(term, skip, take, cancellationToken);
        return new PagedResultResponse<EmpresaResponse>
        {
            Items = page.Items.Select(MapResponse).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<EmpresaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empresa = await empresaRepository.GetByIdAsync(id, cancellationToken);
        return empresa is null ? null : MapResponse(empresa);
    }

    public async Task<EmpresaResponse> SaveAsync(Guid? id, EmpresaRequest request, CancellationToken cancellationToken = default)
    {
        var current = id.HasValue
            ? await empresaRepository.GetByIdAsync(id.Value, cancellationToken)
            : null;

        await ValidateRequestAsync(request, current, cancellationToken);

        var ambienteSri = SriCatalogCodes.NormalizeAmbienteCode(request.AmbienteSri)
            ?? throw new InvalidOperationException("El ambiente SRI no existe en el catalogo parametrizado.");
        var tipoEmision = SriCatalogCodes.NormalizeTipoEmisionCode(request.TipoEmision)
            ?? throw new InvalidOperationException("El tipo de emision no existe en el catalogo parametrizado.");
        var empresaId = current?.Id ?? Guid.NewGuid();
        var puntosEmision = BuildPuntosEmision(request, empresaId);
        var puntoDefault = puntosEmision.First(currentPunto => currentPunto.IsDefault);

        var empresa = new EmpresaEmisora(
            empresaId,
            current?.OwnerUserId ?? currentUserAccessor.GetRequiredUserId(),
            request.RazonSocial.Trim(),
            NormalizeOptional(request.NombreComercial),
            request.Ruc.Trim(),
            request.DireccionMatriz.Trim(),
            puntoDefault.DireccionEstablecimiento,
            puntoDefault.Establecimiento,
            puntoDefault.PuntoEmision,
            ambienteSri,
            request.ModoDesarrollo,
            tipoEmision,
            request.ObligadoContabilidad,
            NormalizeOptional(request.ContribuyenteEspecial),
            NormalizeOptional(request.RegimenRimpe),
            NormalizeOptional(request.AgenteRetencionResolucion),
            ResolveCertificadoNombreArchivo(request, current),
            ResolveCertificadoContenido(request, current),
            NormalizeOptional(request.CertificadoClave),
            puntosEmision,
            request.IsActive,
            current?.CreatedAt ?? DateTimeOffset.UtcNow,
            current is null ? null : DateTimeOffset.UtcNow);

        return MapResponse(await empresaRepository.SaveAsync(empresa, cancellationToken));
    }

    private static EmpresaResponse MapResponse(EmpresaEmisora empresa)
    {
        return new EmpresaResponse
        {
            Id = empresa.Id,
            OwnerUserId = empresa.OwnerUserId,
            RazonSocial = empresa.RazonSocial,
            NombreComercial = empresa.NombreComercial,
            Ruc = empresa.Ruc,
            DireccionMatriz = empresa.DireccionMatriz,
            DireccionEstablecimiento = empresa.DireccionEstablecimiento,
            Establecimiento = empresa.Establecimiento,
            PuntoEmision = empresa.PuntoEmision,
            AmbienteSri = empresa.AmbienteSri,
            ModoDesarrollo = empresa.ModoDesarrollo,
            TipoEmision = empresa.TipoEmision,
            ObligadoContabilidad = empresa.ObligadoContabilidad,
            ContribuyenteEspecial = empresa.ContribuyenteEspecial,
            RegimenRimpe = empresa.RegimenRimpe,
            AgenteRetencionResolucion = empresa.AgenteRetencionResolucion,
            CertificadoNombreArchivo = empresa.CertificadoNombreArchivo,
            TieneCertificadoConfigurado = empresa.CertificadoContenido is { Length: > 0 },
            IsActive = empresa.IsActive,
            CreatedAt = empresa.CreatedAt,
            UpdatedAt = empresa.UpdatedAt,
            PuntosEmision = empresa.PuntosEmision
                .OrderByDescending(punto => punto.IsDefault)
                .ThenBy(punto => punto.Establecimiento)
                .ThenBy(punto => punto.PuntoEmision)
                .Select(punto => new EmpresaPuntoEmisionResponse
                {
                    Id = punto.Id,
                    BodegaId = punto.BodegaId ?? Guid.Empty,
                    BodegaNombre = punto.BodegaNombre,
                    DireccionEstablecimiento = punto.DireccionEstablecimiento,
                    Establecimiento = punto.Establecimiento,
                    PuntoEmision = punto.PuntoEmision,
                    IsDefault = punto.IsDefault
                })
                .ToArray()
        };
    }

    private static EmpresaOptionResponse MapOption(EmpresaEmisora empresa)
    {
        return new EmpresaOptionResponse
        {
            Id = empresa.Id,
            RazonSocial = empresa.RazonSocial,
            NombreComercial = empresa.NombreComercial,
            Ruc = empresa.Ruc,
            AmbienteSri = SriCatalogCodes.NormalizeAmbienteCode(empresa.AmbienteSri) ?? empresa.AmbienteSri,
            IsActive = empresa.IsActive
        };
    }

    private async Task ValidateRequestAsync(EmpresaRequest request, EmpresaEmisora? current, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RazonSocial))
        {
            throw new InvalidOperationException("La razon social es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(request.Ruc) || request.Ruc.Trim().Length != 13 || !request.Ruc.Trim().All(char.IsDigit))
        {
            throw new InvalidOperationException("El RUC debe tener 13 digitos numericos.");
        }

        if (string.IsNullOrWhiteSpace(request.DireccionMatriz))
        {
            throw new InvalidOperationException("La direccion matriz es obligatoria.");
        }

        if (request.PuntosEmision.Count == 0)
        {
            throw new InvalidOperationException("Debes registrar al menos un punto de emision.");
        }

        if (request.PuntosEmision.Count(currentPunto => currentPunto.IsDefault) != 1)
        {
            throw new InvalidOperationException("Debes definir un unico punto de emision predeterminado.");
        }

        var combinaciones = new HashSet<string>(StringComparer.Ordinal);
        foreach (var punto in request.PuntosEmision)
        {
            if (string.IsNullOrWhiteSpace(punto.Establecimiento) || punto.Establecimiento.Trim().Length != 3 || !punto.Establecimiento.Trim().All(char.IsDigit))
            {
                throw new InvalidOperationException("Cada establecimiento debe tener 3 digitos.");
            }

            if (string.IsNullOrWhiteSpace(punto.PuntoEmision) || punto.PuntoEmision.Trim().Length != 3 || !punto.PuntoEmision.Trim().All(char.IsDigit))
            {
                throw new InvalidOperationException("Cada punto de emision debe tener 3 digitos.");
            }

            var clave = $"{punto.Establecimiento.Trim()}-{punto.PuntoEmision.Trim()}";
            if (!combinaciones.Add(clave))
            {
                throw new InvalidOperationException($"La combinacion {clave} esta repetida en los puntos de emision.");
            }
        }

        var ambienteSri = SriCatalogCodes.NormalizeAmbienteCode(request.AmbienteSri);
        if (ambienteSri is null ||
            !await catalogoRepository.ExistsActiveItemAsync("AMBIENTE_SRI", ambienteSri, cancellationToken))
        {
            throw new InvalidOperationException("El ambiente SRI no existe en el catalogo parametrizado.");
        }

        if (request.ModoDesarrollo &&
            SriCatalogCodes.IsProductionEnvironment(ambienteSri))
        {
            throw new InvalidOperationException("No se puede dejar el modo desarrollo activo con ambiente SRI en Produccion.");
        }

        var tipoEmision = SriCatalogCodes.NormalizeTipoEmisionCode(request.TipoEmision);
        if (tipoEmision is null ||
            !await catalogoRepository.ExistsActiveItemAsync("TIPO_EMISION", tipoEmision, cancellationToken))
        {
            throw new InvalidOperationException("El tipo de emision no existe en el catalogo parametrizado.");
        }

        if (!string.IsNullOrWhiteSpace(request.ContribuyenteEspecial) &&
            !request.ContribuyenteEspecial.Trim().All(char.IsDigit))
        {
            throw new InvalidOperationException("El numero de contribuyente especial debe contener solo digitos.");
        }

        if (request.CertificadoContenido is { Length: > 0 })
        {
            if (string.IsNullOrWhiteSpace(request.CertificadoNombreArchivo) ||
                !request.CertificadoNombreArchivo.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("El certificado debe ser un archivo .p12.");
            }

            if (request.CertificadoContenido.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("El certificado .p12 no puede superar 5 MB.");
            }
        }

        if (request.CertificadoContenido is { Length: > 0 } &&
            string.IsNullOrWhiteSpace(request.CertificadoClave))
        {
            throw new InvalidOperationException("La clave del certificado .p12 es obligatoria cuando se carga un certificado.");
        }

        var hasCertificateConfigured = request.CertificadoContenido is { Length: > 0 } ||
            current?.CertificadoContenido is { Length: > 0 };

        var hasCertificatePassword = !string.IsNullOrWhiteSpace(request.CertificadoClave) ||
            !string.IsNullOrWhiteSpace(current?.CertificadoClave);

        if (SriCatalogCodes.IsProductionEnvironment(ambienteSri) &&
            (!hasCertificateConfigured || !hasCertificatePassword))
        {
            throw new InvalidOperationException("Para trabajar en Produccion debes tener certificado digital y clave configurados.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static IReadOnlyCollection<EmpresaPuntoEmision> BuildPuntosEmision(EmpresaRequest request, Guid empresaId)
    {
        return request.PuntosEmision
            .Select((punto, index) => new EmpresaPuntoEmision(
                punto.Id ?? Guid.NewGuid(),
                empresaId,
                punto.Establecimiento.Trim(),
                punto.PuntoEmision.Trim(),
                NormalizeOptional(punto.DireccionEstablecimiento),
                punto.IsDefault || (index == 0 && request.PuntosEmision.Count(current => current.IsDefault) == 0),
                punto.BodegaId,
                null))
            .ToArray();
    }

    private static string? ResolveCertificadoNombreArchivo(EmpresaRequest request, EmpresaEmisora? current)
    {
        if (request.CertificadoContenido is { Length: > 0 })
        {
            return NormalizeOptional(request.CertificadoNombreArchivo);
        }

        return current?.CertificadoNombreArchivo;
    }

    private static byte[]? ResolveCertificadoContenido(EmpresaRequest request, EmpresaEmisora? current)
    {
        if (request.CertificadoContenido is { Length: > 0 })
        {
            return request.CertificadoContenido;
        }

        return current?.CertificadoContenido;
    }
}
