using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Retenciones.Ports.In;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Retenciones.Enums;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Retenciones;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Retenciones;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Retenciones;

public sealed class RetencionService(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserAccessor currentUserAccessor,
    ClaveAccesoService claveAccesoService,
    SriXadesBesSigner signer,
    SriSoapClient soapClient,
    ILogger<RetencionService> logger) : IRetencionService
{
    public async Task<PagedResultResponse<ComprobanteRetencionResponse>> GetPagedAsync(
        string? term,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ComprobantesRetencion
            .AsNoTracking()
            .AsQueryable();

        var normalizedTerm = term?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(current =>
                current.Secuencial.Contains(normalizedTerm) ||
                current.Establecimiento.Contains(normalizedTerm) ||
                current.PuntoEmision.Contains(normalizedTerm) ||
                (current.ClaveAcceso != null && current.ClaveAcceso.Contains(normalizedTerm)) ||
                (current.NumeroAutorizacion != null && current.NumeroAutorizacion.Contains(normalizedTerm)));
        }

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(current => current.FechaEmision)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);

        return new PagedResultResponse<ComprobanteRetencionResponse>
        {
            Items = entities.Select(Map).ToArray(),
            TotalCount = total,
            Skip = skip,
            Take = take
        };
    }

    public async Task<ComprobanteRetencionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ComprobantesRetencion
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<ComprobanteRetencionResponse> CreateAsync(
        ComprobanteRetencionRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        ValidateRequest(request);

        await EnsureProveedorAsync(empresaId, request.ProveedorId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new ComprobanteRetencionEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            CompraId = request.CompraId,
            ProveedorId = request.ProveedorId,
            Establecimiento = NormalizeNumeric(request.Establecimiento, 3, "establecimiento"),
            PuntoEmision = NormalizeNumeric(request.PuntoEmision, 3, "punto de emision"),
            Secuencial = string.IsNullOrWhiteSpace(request.Secuencial)
                ? await GenerateNextSecuencialAsync(empresaId, request.Establecimiento, request.PuntoEmision, cancellationToken)
                : NormalizeNumeric(request.Secuencial, 9, "secuencial", padLeft: true),
            ClaveAcceso = NormalizeOptional(request.ClaveAcceso),
            FechaEmision = new DateTimeOffset(request.FechaEmision.Date, TimeSpan.Zero),
            AmbienteSRI = ParseAmbiente(request.AmbienteSRI),
            EstadoSRI = ParseEstado(request.EstadoSRI),
            NumeroAutorizacion = NormalizeOptional(request.NumeroAutorizacion),
            FechaAutorizacion = request.FechaAutorizacion.HasValue
                ? new DateTimeOffset(request.FechaAutorizacion.Value, TimeSpan.Zero)
                : null,
            MensajeErrorSRI = NormalizeOptional(request.MensajeErrorSRI),
            TotalRetenido = Round(request.TotalRetenido),
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var detail in request.Detalles)
        {
            entity.Detalles.Add(BuildDetail(entity.Id, detail));
        }

        dbContext.ComprobantesRetencion.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ComprobanteRetencionResponse> CrearRetencionDesdeCompraAsync(
        Guid compraId,
        CancellationToken cancellationToken = default)
    {
        var compra = await dbContext.Compras
            .Include(current => current.Detalles)
            .Include(current => current.Proveedor)
                .ThenInclude(current => current.Persona)
            .FirstOrDefaultAsync(current => current.Id == compraId, cancellationToken)
            ?? throw new InvalidOperationException("La compra no existe o no pertenece a la empresa activa.");

        var empresa = await dbContext.EmpresasEmisoras
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == compra.EmpresaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la empresa emisora activa.");

        var now = DateTimeOffset.UtcNow;
        var secuencial = await GenerateNextSecuencialAsync(compra.EmpresaId, empresa.Establecimiento, empresa.PuntoEmision, cancellationToken);
        var retencion = new ComprobanteRetencionEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = compra.EmpresaId,
            CompraId = compra.Id,
            ProveedorId = compra.ProveedorId,
            Establecimiento = NormalizeNumeric(empresa.Establecimiento, 3, "establecimiento"),
            PuntoEmision = NormalizeNumeric(empresa.PuntoEmision, 3, "punto de emision"),
            Secuencial = secuencial,
            FechaEmision = now,
            AmbienteSRI = ClaveAccesoService.GetAmbienteCode(empresa.AmbienteSri) == "2"
                ? AmbienteSriRetencion.Produccion
                : AmbienteSriRetencion.Pruebas,
            EstadoSRI = EstadoSriRetencion.Borrador,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var detail in BuildAutomaticDetails(compra, retencion.Id))
        {
            retencion.Detalles.Add(detail);
            retencion.TotalRetenido += detail.ValorRetenido;
        }

        if (retencion.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La compra no genera valores sujetos a retencion.");
        }

        retencion.TotalRetenido = Round(retencion.TotalRetenido);
        dbContext.ComprobantesRetencion.Add(retencion);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Retencion {RetencionId} generada desde compra {CompraId}. EmpresaId={EmpresaId}, TotalRetenido={TotalRetenido}.",
            retencion.Id,
            compra.Id,
            compra.EmpresaId,
            retencion.TotalRetenido);
        return Map(retencion);
    }

    public async Task<ComprobanteRetencionResponse> ProcesarRetencionSRIAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var retencion = await dbContext.ComprobantesRetencion
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("La retencion no existe o no pertenece a la empresa activa.");

        var empresa = await dbContext.EmpresasEmisoras
            .Include(current => current.CertificadosDigitales)
            .FirstOrDefaultAsync(current => current.Id == retencion.EmpresaId && current.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la empresa emisora activa.");

        var proveedor = await dbContext.Proveedores
            .AsNoTracking()
            .Include(current => current.Persona)
            .FirstOrDefaultAsync(current => current.PersonaId == retencion.ProveedorId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro el proveedor de la retencion.");

        if (string.IsNullOrWhiteSpace(retencion.ClaveAcceso))
        {
            retencion.ClaveAcceso = claveAccesoService.Generar(
                retencion.FechaEmision,
                "07",
                empresa.Ruc,
                ((byte)retencion.AmbienteSRI).ToString(),
                retencion.Establecimiento,
                retencion.PuntoEmision,
                retencion.Secuencial,
                BuildCodigoNumerico(retencion.Id),
                string.IsNullOrWhiteSpace(empresa.TipoEmision) ? "1" : empresa.TipoEmision);
        }

        var xmlGenerado = BuildXml(retencion, empresa, proveedor);
        logger.LogInformation(
            "XML de retencion generado. RetencionId={RetencionId}, ClaveAcceso={ClaveAcceso}, Ambiente={Ambiente}.",
            retencion.Id,
            retencion.ClaveAcceso,
            retencion.AmbienteSRI);

        if (empresa.ModoDesarrollo)
        {
            retencion.EstadoSRI = EstadoSriRetencion.Autorizado;
            retencion.NumeroAutorizacion = retencion.ClaveAcceso;
            retencion.FechaAutorizacion = DateTimeOffset.UtcNow;
            retencion.MensajeErrorSRI = null;
            retencion.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Retencion {RetencionId} autorizada en modo desarrollo con clave {ClaveAcceso}.",
                retencion.Id,
                retencion.ClaveAcceso);
            return Map(retencion);
        }

        var (certificadoContenido, certificadoClave) = ResolveCertificate(empresa);
        var xmlFirmado = signer.Sign(xmlGenerado, certificadoContenido, certificadoClave);
        logger.LogInformation("XML de retencion firmado localmente. RetencionId={RetencionId}.", retencion.Id);

        try
        {
            retencion.EstadoSRI = EstadoSriRetencion.Firmado;
            var recepcion = await soapClient.ValidarComprobanteAsync(((byte)retencion.AmbienteSRI).ToString(), Encoding.UTF8.GetBytes(xmlFirmado), cancellationToken);
            logger.LogInformation(
                "Respuesta de recepcion SRI para retencion {RetencionId}: {Estado}.",
                retencion.Id,
                recepcion.Estado);
            if (!string.Equals(recepcion.Estado, "RECIBIDA", StringComparison.OrdinalIgnoreCase))
            {
                retencion.EstadoSRI = EstadoSriRetencion.NoAutorizado;
                retencion.MensajeErrorSRI = JoinMensajes(recepcion.Mensajes, $"Retencion devuelta por SRI. Estado: {recepcion.Estado}");
                retencion.UpdatedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
                return Map(retencion);
            }

            retencion.EstadoSRI = EstadoSriRetencion.Enviado;
            var autorizacion = await soapClient.ConsultarAutorizacionAsync(((byte)retencion.AmbienteSRI).ToString(), retencion.ClaveAcceso, cancellationToken);
            logger.LogInformation(
                "Respuesta de autorizacion SRI para retencion {RetencionId}: {Estado}.",
                retencion.Id,
                autorizacion.Estado);
            if (string.Equals(autorizacion.Estado, "AUTORIZADO", StringComparison.OrdinalIgnoreCase))
            {
                retencion.EstadoSRI = EstadoSriRetencion.Autorizado;
                retencion.NumeroAutorizacion = autorizacion.NumeroAutorizacion ?? retencion.ClaveAcceso;
                retencion.FechaAutorizacion = autorizacion.FechaAutorizacion ?? DateTimeOffset.UtcNow;
                retencion.MensajeErrorSRI = null;
            }
            else
            {
                retencion.EstadoSRI = EstadoSriRetencion.NoAutorizado;
                retencion.MensajeErrorSRI = JoinMensajes(autorizacion.Mensajes, $"Retencion no autorizada por SRI. Estado: {autorizacion.Estado}");
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            retencion.EstadoSRI = EstadoSriRetencion.Enviado;
            retencion.MensajeErrorSRI = $"No fue posible completar el procesamiento SRI: {exception.Message}";
            logger.LogWarning(exception, "Procesamiento SRI de retencion {RetencionId} quedo en contingencia.", retencion.Id);
        }

        retencion.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(retencion);
    }

    private Guid ResolveEmpresaId()
    {
        return tenantContextAccessor.EmpresaId
            ?? currentUserAccessor.GetRequiredEmpresaId();
    }

    private async Task EnsureProveedorAsync(Guid empresaId, Guid proveedorId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Proveedores.AnyAsync(current => current.EmpresaId == empresaId && current.PersonaId == proveedorId, cancellationToken))
        {
            throw new InvalidOperationException("El proveedor seleccionado no existe en la empresa activa.");
        }
    }

    private async Task<string> GenerateNextSecuencialAsync(
        Guid empresaId,
        string establecimiento,
        string puntoEmision,
        CancellationToken cancellationToken)
    {
        var estab = NormalizeNumeric(establecimiento, 3, "establecimiento");
        var pto = NormalizeNumeric(puntoEmision, 3, "punto de emision");
        var lastSecuencial = await dbContext.ComprobantesRetencion
            .IgnoreQueryFilters()
            .Where(current => current.EmpresaId == empresaId && current.Establecimiento == estab && current.PuntoEmision == pto)
            .OrderByDescending(current => current.Secuencial)
            .Select(current => current.Secuencial)
            .FirstOrDefaultAsync(cancellationToken);

        var next = long.TryParse(lastSecuencial, out var last)
            ? last + 1
            : 1;

        return next.ToString("000000000");
    }

    private static IEnumerable<ComprobanteRetencionDetalleEntity> BuildAutomaticDetails(
        CompraEntity compra,
        Guid retencionId)
    {
        var numeroSustento = $"{compra.Establecimiento}-{compra.PuntoEmision}-{compra.Secuencial}";
        var codigoRenta = string.IsNullOrWhiteSpace(compra.Proveedor.CodigoRetencionRentaDefault)
            ? "312"
            : compra.Proveedor.CodigoRetencionRentaDefault.Trim();
        var codigoIva = string.IsNullOrWhiteSpace(compra.Proveedor.CodigoRetencionIvaDefault)
            ? "3440"
            : compra.Proveedor.CodigoRetencionIvaDefault.Trim();

        foreach (var detalle in compra.Detalles)
        {
            if (detalle.CostoTotalSinImpuesto > 0)
            {
                yield return new ComprobanteRetencionDetalleEntity
                {
                    Id = Guid.NewGuid(),
                    ComprobanteRetencionId = retencionId,
                    CodigoImpuesto = "1",
                    CodigoRetencionSRI = codigoRenta,
                    BaseImponible = Round(detalle.CostoTotalSinImpuesto),
                    PorcentajeRetencion = ResolveRentaPercent(codigoRenta),
                    ValorRetenido = Round(detalle.CostoTotalSinImpuesto * ResolveRentaPercent(codigoRenta) / 100m),
                    CodDocSustento = string.IsNullOrWhiteSpace(compra.TipoComprobanteSRI) ? "01" : compra.TipoComprobanteSRI,
                    NumDocSustento = numeroSustento,
                    FechaEmisionDocSustento = compra.FechaEmision
                };
            }

            var ivaBase = Math.Round(detalle.CostoTotalSinImpuesto * detalle.PorcentajeIva / 100m, 4, MidpointRounding.AwayFromZero);
            if (ivaBase > 0)
            {
                yield return new ComprobanteRetencionDetalleEntity
                {
                    Id = Guid.NewGuid(),
                    ComprobanteRetencionId = retencionId,
                    CodigoImpuesto = "2",
                    CodigoRetencionSRI = codigoIva,
                    BaseImponible = ivaBase,
                    PorcentajeRetencion = ResolveIvaPercent(codigoIva),
                    ValorRetenido = Round(ivaBase * ResolveIvaPercent(codigoIva) / 100m),
                    CodDocSustento = string.IsNullOrWhiteSpace(compra.TipoComprobanteSRI) ? "01" : compra.TipoComprobanteSRI,
                    NumDocSustento = numeroSustento,
                    FechaEmisionDocSustento = compra.FechaEmision
                };
            }
        }
    }

    private static ComprobanteRetencionDetalleEntity BuildDetail(Guid retencionId, ComprobanteRetencionDetalleRequest detail)
    {
        return new ComprobanteRetencionDetalleEntity
        {
            Id = Guid.NewGuid(),
            ComprobanteRetencionId = retencionId,
            CodigoImpuesto = NormalizeRequired(detail.CodigoImpuesto, 5, "codigo de impuesto"),
            CodigoRetencionSRI = NormalizeRequired(detail.CodigoRetencionSRI, 10, "codigo de retencion SRI"),
            BaseImponible = Round(detail.BaseImponible),
            PorcentajeRetencion = Math.Round(detail.PorcentajeRetencion, 2, MidpointRounding.AwayFromZero),
            ValorRetenido = Round(detail.ValorRetenido),
            CodDocSustento = NormalizeRequired(detail.CodDocSustento, 2, "documento sustento"),
            NumDocSustento = NormalizeRequired(detail.NumDocSustento, 15, "numero documento sustento"),
            FechaEmisionDocSustento = new DateTimeOffset(detail.FechaEmisionDocSustento.Date, TimeSpan.Zero)
        };
    }

    private static string BuildXml(
        ComprobanteRetencionEntity retencion,
        EmpresaEmisoraEntity empresa,
        ProveedorEntity proveedor)
    {
        XNamespace ns = "";
        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "comprobanteRetencion",
                new XAttribute("id", "comprobante"),
                new XAttribute("version", "2.0.0"),
                new XElement(ns + "infoTributaria",
                    new XElement(ns + "ambiente", ((byte)retencion.AmbienteSRI).ToString()),
                    new XElement(ns + "tipoEmision", string.IsNullOrWhiteSpace(empresa.TipoEmision) ? "1" : empresa.TipoEmision),
                    new XElement(ns + "razonSocial", empresa.RazonSocial),
                    new XElement(ns + "nombreComercial", empresa.NombreComercial ?? empresa.RazonSocial),
                    new XElement(ns + "ruc", empresa.Ruc),
                    new XElement(ns + "claveAcceso", retencion.ClaveAcceso),
                    new XElement(ns + "codDoc", "07"),
                    new XElement(ns + "estab", retencion.Establecimiento),
                    new XElement(ns + "ptoEmi", retencion.PuntoEmision),
                    new XElement(ns + "secuencial", retencion.Secuencial),
                    new XElement(ns + "dirMatriz", empresa.DireccionMatriz)),
                new XElement(ns + "infoCompRetencion",
                    new XElement(ns + "fechaEmision", retencion.FechaEmision.ToString("dd/MM/yyyy")),
                    new XElement(ns + "dirEstablecimiento", empresa.DireccionEstablecimiento ?? empresa.DireccionMatriz),
                    empresa.ContribuyenteEspecial is null ? null : new XElement(ns + "contribuyenteEspecial", empresa.ContribuyenteEspecial),
                    new XElement(ns + "obligadoContabilidad", empresa.ObligadoContabilidad ? "SI" : "NO"),
                    new XElement(ns + "tipoIdentificacionSujetoRetenido", SriCatalogCodes.NormalizeTipoIdentificacionCode(proveedor.Persona.TipoIdentificacion) ?? proveedor.Persona.TipoIdentificacion),
                    new XElement(ns + "razonSocialSujetoRetenido", proveedor.Persona.RazonSocialONombresCompletos),
                    new XElement(ns + "identificacionSujetoRetenido", proveedor.Persona.Identificacion),
                    new XElement(ns + "periodoFiscal", retencion.FechaEmision.ToString("MM/yyyy"))),
                BuildDocsSustentoElement(ns, retencion.Detalles)));

        return documento.ToString(SaveOptions.DisableFormatting);
    }

    private static XElement BuildDocsSustentoElement(
        XNamespace ns,
        IEnumerable<ComprobanteRetencionDetalleEntity> detalles)
    {
        return new XElement(
            ns + "docsSustento",
            detalles
                .GroupBy(current => new { current.CodDocSustento, current.NumDocSustento, current.FechaEmisionDocSustento })
                .Select(group =>
                {
                    var totalBase = group.Sum(item => item.BaseImponible);
                    var retenciones = group.Select(item => new XElement(
                        ns + "retencion",
                        new XElement(ns + "codigo", item.CodigoImpuesto),
                        new XElement(ns + "codigoRetencion", item.CodigoRetencionSRI),
                        new XElement(ns + "baseImponible", item.BaseImponible.ToString("0.00")),
                        new XElement(ns + "porcentajeRetener", item.PorcentajeRetencion.ToString("0.00")),
                        new XElement(ns + "valorRetenido", item.ValorRetenido.ToString("0.00"))));

                    return new XElement(
                        ns + "docSustento",
                        new XElement(ns + "codSustento", "01"),
                        new XElement(ns + "codDocSustento", group.Key.CodDocSustento),
                        new XElement(ns + "numDocSustento", group.Key.NumDocSustento.Replace("-", string.Empty)),
                        new XElement(ns + "fechaEmisionDocSustento", group.Key.FechaEmisionDocSustento.ToString("dd/MM/yyyy")),
                        new XElement(ns + "pagoLocExt", "01"),
                        new XElement(ns + "totalComprobantesReembolso", "0.00"),
                        new XElement(ns + "totalBaseImponibleReembolso", "0.00"),
                        new XElement(ns + "totalImpuestoReembolso", "0.00"),
                        new XElement(ns + "totalSinImpuestos", totalBase.ToString("0.00")),
                        new XElement(ns + "importeTotal", totalBase.ToString("0.00")),
                        new XElement(ns + "impuestosDocSustento"),
                        new XElement(ns + "retenciones", retenciones));
                }));
    }

    private static (byte[] Contenido, string Clave) ResolveCertificate(EmpresaEmisoraEntity empresa)
    {
        var certificado = empresa.CertificadosDigitales
            .Where(current => current.IsActive)
            .OrderByDescending(current => current.EsPrincipal)
            .ThenByDescending(current => current.FechaFinVigencia)
            .FirstOrDefault();

        if (certificado is not null)
        {
            return (certificado.Contenido, certificado.Clave);
        }

        if (empresa.CertificadoContenido is { Length: > 0 } && !string.IsNullOrWhiteSpace(empresa.CertificadoClave))
        {
            return (empresa.CertificadoContenido, empresa.CertificadoClave);
        }

        throw new InvalidOperationException("La empresa no tiene certificado digital activo para firmar retenciones.");
    }

    private static string BuildCodigoNumerico(Guid id)
    {
        var bytes = SHA256.HashData(id.ToByteArray());
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 100000000;
        return number.ToString("00000000");
    }

    private static decimal ResolveRentaPercent(string codigo)
    {
        return codigo.Trim() switch
        {
            "312" => 1.75m,
            "343" => 2.75m,
            "344" => 2.75m,
            _ => 1.75m
        };
    }

    private static decimal ResolveIvaPercent(string codigo)
    {
        return codigo.Trim() switch
        {
            "3430" => 30m,
            "3440" => 70m,
            "3450" => 100m,
            _ => 30m
        };
    }

    private static void ValidateRequest(ComprobanteRetencionRequest request)
    {
        if (request.ProveedorId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe seleccionar un proveedor para la retencion.");
        }

        if (request.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La retencion debe tener al menos un detalle.");
        }
    }

    private static AmbienteSriRetencion ParseAmbiente(byte ambiente)
    {
        return Enum.IsDefined(typeof(AmbienteSriRetencion), ambiente)
            ? (AmbienteSriRetencion)ambiente
            : throw new InvalidOperationException("El ambiente SRI no es valido.");
    }

    private static EstadoSriRetencion ParseEstado(int estado)
    {
        return Enum.IsDefined(typeof(EstadoSriRetencion), estado)
            ? (EstadoSriRetencion)estado
            : throw new InvalidOperationException("El estado SRI de retencion no es valido.");
    }

    private static string NormalizeNumeric(string? value, int length, string fieldName, bool padLeft = false)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (padLeft)
        {
            normalized = normalized.PadLeft(length, '0');
        }

        if (normalized.Length != length || !normalized.All(char.IsDigit))
        {
            throw new InvalidOperationException($"El campo {fieldName} debe contener {length} digitos.");
        }

        return normalized;
    }

    private static string NormalizeRequired(string? value, int maxLength, string fieldName)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"El campo {fieldName} es obligatorio y no puede superar {maxLength} caracteres.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal Round(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

    private static string JoinMensajes(IReadOnlyCollection<SriSoapMensaje> mensajes, string fallback)
    {
        var joined = string.Join(" | ", mensajes.Select(current => current.ToString()).Where(current => !string.IsNullOrWhiteSpace(current)));
        return string.IsNullOrWhiteSpace(joined) ? fallback : joined;
    }

    private static ComprobanteRetencionResponse Map(ComprobanteRetencionEntity entity)
    {
        return new ComprobanteRetencionResponse
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            CompraId = entity.CompraId,
            ProveedorId = entity.ProveedorId,
            Establecimiento = entity.Establecimiento,
            PuntoEmision = entity.PuntoEmision,
            Secuencial = entity.Secuencial,
            ClaveAcceso = entity.ClaveAcceso,
            FechaEmision = entity.FechaEmision.Date,
            AmbienteSRI = (byte)entity.AmbienteSRI,
            EstadoSRI = (int)entity.EstadoSRI,
            EstadoSRINombre = entity.EstadoSRI.ToString(),
            NumeroAutorizacion = entity.NumeroAutorizacion,
            FechaAutorizacion = entity.FechaAutorizacion?.Date,
            MensajeErrorSRI = entity.MensajeErrorSRI,
            TotalRetenido = entity.TotalRetenido,
            Detalles = entity.Detalles.Select(detail => new ComprobanteRetencionDetalleResponse
            {
                Id = detail.Id,
                CodigoImpuesto = detail.CodigoImpuesto,
                CodigoRetencionSRI = detail.CodigoRetencionSRI,
                BaseImponible = detail.BaseImponible,
                PorcentajeRetencion = detail.PorcentajeRetencion,
                ValorRetenido = detail.ValorRetenido,
                CodDocSustento = detail.CodDocSustento,
                NumDocSustento = detail.NumDocSustento,
                FechaEmisionDocSustento = detail.FechaEmisionDocSustento.Date
            }).ToArray()
        };
    }
}
