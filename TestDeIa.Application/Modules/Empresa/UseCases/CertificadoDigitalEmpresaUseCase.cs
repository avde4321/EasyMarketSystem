using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Application.Modules.Empresa.UseCases;

public sealed class CertificadoDigitalEmpresaUseCase(
    ICertificadoDigitalEmpresaRepository certificadoRepository,
    IEmpresaRepository empresaRepository,
    ICurrentUserAccessor currentUserAccessor) : ICertificadoDigitalEmpresaUseCase
{
    private const int MaxCertificateBytes = 5 * 1024 * 1024;

    public async Task<PagedResultResponse<CertificadoDigitalEmpresaResponse>> GetPagedAsync(
        string? term,
        Guid? empresaId,
        bool soloAlertas,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var page = await certificadoRepository.GetPagedAsync(term, empresaId, soloAlertas, skip, take, cancellationToken);
        return new PagedResultResponse<CertificadoDigitalEmpresaResponse>
        {
            Items = page.Items.Select(Map).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<CertificadoDigitalEmpresaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificado = await certificadoRepository.GetByIdAsync(id, cancellationToken);
        return certificado is null ? null : Map(certificado);
    }

    public async Task<IReadOnlyCollection<CertificadoDigitalEmpresaResponse>> GetAlertasAsync(CancellationToken cancellationToken = default)
    {
        var certificados = await certificadoRepository.GetAlertasAsync(cancellationToken);
        return certificados.Select(Map).ToArray();
    }

    public async Task<CertificadoDigitalEmpresaResponse> CreateAsync(CertificadoDigitalEmpresaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var empresa = await empresaRepository.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró la empresa asociada al certificado.");

        var certificate = LoadCertificate(request);
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException("El certificado no contiene una clave privada válida para firmar comprobantes electrónicos.");
        }

        var fechaFin = new DateTimeOffset(certificate.NotAfter.ToUniversalTime());
        if (request.ActivarComoPrincipal && fechaFin.Date < DateTimeOffset.UtcNow.Date)
        {
            throw new InvalidOperationException("No se puede activar como principal un certificado caducado.");
        }

        var now = DateTimeOffset.UtcNow;
        var model = new CertificadoDigitalEmpresa(
            Guid.NewGuid(),
            request.EmpresaId,
            empresa.RazonSocial,
            empresa.Ruc,
            request.Nombre.Trim(),
            request.NombreArchivo.Trim(),
            request.Contenido,
            request.Clave,
            certificate.Subject,
            certificate.Issuer,
            certificate.SerialNumber,
            certificate.Thumbprint,
            new DateTimeOffset(certificate.NotBefore.ToUniversalTime()),
            fechaFin,
            true,
            request.ActivarComoPrincipal,
            now,
            currentUserAccessor.GetUserId(),
            null,
            null);

        var saved = await certificadoRepository.SaveAsync(model, request.ActivarComoPrincipal, cancellationToken);
        return Map(saved);
    }

    public async Task<CertificadoDigitalEmpresaResponse> ActivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificado = await certificadoRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el certificado digital.");

        if (certificado.FechaFinVigencia.Date < DateTimeOffset.UtcNow.Date)
        {
            throw new InvalidOperationException("No se puede activar un certificado caducado.");
        }

        return Map(await certificadoRepository.ActivarAsync(id, cancellationToken));
    }

    public Task DesactivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return certificadoRepository.DesactivarAsync(id, cancellationToken);
    }

    private static void ValidateRequest(CertificadoDigitalEmpresaRequest request)
    {
        if (request.EmpresaId == Guid.Empty)
        {
            throw new InvalidOperationException("Selecciona la empresa del certificado.");
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new InvalidOperationException("Ingresa un nombre descriptivo para el certificado.");
        }

        if (string.IsNullOrWhiteSpace(request.NombreArchivo) ||
            (!request.NombreArchivo.EndsWith(".p12", StringComparison.OrdinalIgnoreCase) &&
             !request.NombreArchivo.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Carga un archivo de firma válido con extensión .p12 o .pfx.");
        }

        if (request.Contenido.Length == 0 || request.Contenido.Length > MaxCertificateBytes)
        {
            throw new InvalidOperationException("El archivo del certificado es obligatorio y no debe superar 5 MB.");
        }

        if (string.IsNullOrWhiteSpace(request.Clave))
        {
            throw new InvalidOperationException("Ingresa la clave del certificado digital.");
        }
    }

    private static X509Certificate2 LoadCertificate(CertificadoDigitalEmpresaRequest request)
    {
        try
        {
            return X509CertificateLoader.LoadPkcs12(
                request.Contenido,
                request.Clave,
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        }
        catch (Exception exception) when (exception is CryptographicException or ArgumentException)
        {
            throw new InvalidOperationException("No se pudo leer el certificado. Verifica que el archivo y la clave sean correctos.", exception);
        }
    }

    private static CertificadoDigitalEmpresaResponse Map(CertificadoDigitalEmpresa certificado)
    {
        var dias = (certificado.FechaFinVigencia.Date - DateTimeOffset.UtcNow.Date).Days;
        return new CertificadoDigitalEmpresaResponse
        {
            Id = certificado.Id,
            EmpresaId = certificado.EmpresaId,
            EmpresaNombre = certificado.EmpresaNombre,
            EmpresaRuc = certificado.EmpresaRuc,
            Nombre = certificado.Nombre,
            NombreArchivo = certificado.NombreArchivo,
            Sujeto = certificado.Sujeto,
            Emisor = certificado.Emisor,
            NumeroSerie = certificado.NumeroSerie,
            HuellaDigital = certificado.HuellaDigital,
            FechaInicioVigencia = certificado.FechaInicioVigencia,
            FechaFinVigencia = certificado.FechaFinVigencia,
            DiasParaCaducar = dias,
            EstaVigente = dias >= 0,
            EstaCaducado = dias < 0,
            RequiereAlerta = dias <= 30,
            EstadoVigencia = dias < 0 ? "Caducado" : dias == 0 ? "Vence hoy" : dias <= 30 ? "Por vencer" : "Vigente",
            NivelAlerta = dias < 0 ? "caducado" : dias <= 15 ? "critico" : dias <= 20 ? "alto" : dias <= 30 ? "medio" : "normal",
            IsActive = certificado.IsActive,
            EsPrincipal = certificado.EsPrincipal,
            CreatedAt = certificado.CreatedAt,
            UpdatedAt = certificado.UpdatedAt
        };
    }
}
