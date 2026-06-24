using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

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

        var empresa = new EmpresaEmisora(
            current?.Id ?? Guid.NewGuid(),
            current?.OwnerUserId ?? currentUserAccessor.GetRequiredUserId(),
            request.RazonSocial.Trim(),
            NormalizeOptional(request.NombreComercial),
            request.Ruc.Trim(),
            request.DireccionMatriz.Trim(),
            NormalizeOptional(request.DireccionEstablecimiento),
            request.Establecimiento.Trim(),
            request.PuntoEmision.Trim(),
            request.AmbienteSri.Trim(),
            request.ModoDesarrollo,
            request.TipoEmision.Trim(),
            request.ObligadoContabilidad,
            NormalizeOptional(request.ContribuyenteEspecial),
            NormalizeOptional(request.RegimenRimpe),
            NormalizeOptional(request.AgenteRetencionResolucion),
            ResolveCertificadoNombreArchivo(request, current),
            ResolveCertificadoContenido(request, current),
            NormalizeOptional(request.CertificadoClave),
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
            UpdatedAt = empresa.UpdatedAt
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

        if (string.IsNullOrWhiteSpace(request.Establecimiento) || request.Establecimiento.Trim().Length != 3 || !request.Establecimiento.Trim().All(char.IsDigit))
        {
            throw new InvalidOperationException("El establecimiento debe tener 3 digitos.");
        }

        if (string.IsNullOrWhiteSpace(request.PuntoEmision) || request.PuntoEmision.Trim().Length != 3 || !request.PuntoEmision.Trim().All(char.IsDigit))
        {
            throw new InvalidOperationException("El punto de emision debe tener 3 digitos.");
        }

        if (!await catalogoRepository.ExistsActiveItemAsync("AMBIENTE_SRI", request.AmbienteSri.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("El ambiente SRI no existe en el catalogo parametrizado.");
        }

        if (request.ModoDesarrollo &&
            string.Equals(request.AmbienteSri, "Produccion", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("No se puede dejar el modo desarrollo activo con ambiente SRI en Produccion.");
        }

        if (!await catalogoRepository.ExistsActiveItemAsync("TIPO_EMISION", request.TipoEmision.Trim(), cancellationToken))
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

        if (string.Equals(request.AmbienteSri, "Produccion", StringComparison.OrdinalIgnoreCase) &&
            (!hasCertificateConfigured || !hasCertificatePassword))
        {
            throw new InvalidOperationException("Para trabajar en Produccion debes tener certificado digital y clave configurados.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
