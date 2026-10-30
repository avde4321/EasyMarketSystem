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
    SriDocumentSubmissionService submission,
    RetencionXmlGenerator xmlGenerator,
    TestDeIa.Infrastructure.Adapters.Out.Sri.SriXmlArtifactService artifacts,
    ILogger<RetencionService> logger,
    TestDeIa.Application.Modules.Saas.Ports.In.ISaaSQuotaValidationService quota) : IRetencionService
{
    public async Task ActualizarParteRelacionadaAsync(Guid id, bool parteRelacionada, CancellationToken cancellationToken = default)
    {
        await using var lease = await SriProcessingLease.AcquireAsync(dbContext, ResolveEmpresaId(), id, cancellationToken);
        var retencion = await dbContext.ComprobantesRetencion.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("No existe la retención en la empresa activa.");
        if (retencion.EstadoSRI != EstadoSriRetencion.Borrador || await dbContext.DocumentosAdjuntos.AnyAsync(x => x.EmpresaId == retencion.EmpresaId && x.EntidadId == id && x.TipoAdjunto == "XML_FIRMADO", cancellationToken))
            throw new InvalidOperationException("Solo puede editar los datos fiscales antes de firmar el documento.");
        retencion.ParteRelacionada = parteRelacionada;
        retencion.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResultResponse<ComprobanteRetencionResponse>> GetPagedAsync(
        string? term,
        DateTimeOffset fechaDesde,
        DateTimeOffset fechaHasta,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var desde = fechaDesde.DateTime;
        var hasta = fechaHasta.DateTime;
        var query = dbContext.ComprobantesRetencion
            .AsNoTracking()
            .Where(current => current.FechaEmision >= desde && current.FechaEmision <= hasta)
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
        var items = await query
            .OrderByDescending(current => current.FechaEmision)
            .Skip(skip)
            .Take(take)
            .Select(current => new ComprobanteRetencionResponse
            {
                Id = current.Id,
                EmpresaId = current.EmpresaId,
                CompraId = current.CompraId,
                ProveedorId = current.ProveedorId,
                ParteRelacionada = current.ParteRelacionada,
                Establecimiento = current.Establecimiento,
                PuntoEmision = current.PuntoEmision,
                Secuencial = current.Secuencial,
                ClaveAcceso = current.ClaveAcceso,
                FechaEmision = current.FechaEmision.Date,
                AmbienteSRI = (byte)current.AmbienteSRI,
                EstadoSRI = (int)current.EstadoSRI,
                EstadoSRINombre = current.EstadoSRI.ToString(),
                NumeroAutorizacion = current.NumeroAutorizacion,
                FechaAutorizacion = current.FechaAutorizacion.HasValue ? current.FechaAutorizacion.Value.Date : null,
                MensajeErrorSRI = current.MensajeErrorSRI,
                TotalRetenido = current.TotalRetenido
            })
            .ToArrayAsync(cancellationToken);

        return new PagedResultResponse<ComprobanteRetencionResponse>
        {
            Items = items,
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
        await quota.EnsureModuleAsync(empresaId, TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri, cancellationToken);
        ValidateRequest(request);

        await EnsureProveedorAsync(empresaId, request.ProveedorId, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var reservado = await GenerateNextSecuencialAsync(empresaId, request.Establecimiento, request.PuntoEmision, ((int)ParseAmbiente(request.AmbienteSRI)).ToString(), cancellationToken);
        if (!string.IsNullOrWhiteSpace(request.Secuencial) && NormalizeNumeric(request.Secuencial, 9, "secuencial", padLeft: true) != reservado)
            throw new InvalidOperationException("El secuencial debe coincidir con la numeración configurada. Ajuste el contador en Configuración.");

        var now = DateTimeOffset.UtcNow;
        var entity = new ComprobanteRetencionEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            CompraId = request.CompraId,
            ProveedorId = request.ProveedorId,
            ParteRelacionada = request.ParteRelacionada,
            Establecimiento = NormalizeNumeric(request.Establecimiento, 3, "establecimiento"),
            PuntoEmision = NormalizeNumeric(request.PuntoEmision, 3, "punto de emision"),
            Secuencial = reservado,
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
        await transaction.CommitAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ComprobanteRetencionResponse> CrearRetencionDesdeCompraAsync(
        Guid compraId,
        CancellationToken cancellationToken = default)
    {
        await quota.EnsureModuleAsync(ResolveEmpresaId(), TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri, cancellationToken);
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
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var secuencial = await GenerateNextSecuencialAsync(compra.EmpresaId, empresa.Establecimiento, empresa.PuntoEmision, ClaveAccesoService.GetAmbienteCode(empresa.AmbienteSri), cancellationToken);
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
        await transaction.CommitAsync(cancellationToken);
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
        await quota.EnsureModuleAsync(ResolveEmpresaId(), TestDeIa.Domain.Modules.Saas.Enums.SaasModule.Sri, cancellationToken);
        await using var lease = await SriProcessingLease.AcquireAsync(dbContext, ResolveEmpresaId(), id, cancellationToken);
        return await ProcesarRetencionCoreAsync(id, cancellationToken);
    }

    private async Task<ComprobanteRetencionResponse> ProcesarRetencionCoreAsync(Guid id, CancellationToken cancellationToken)
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

        if (retencion.EstadoSRI == EstadoSriRetencion.Autorizado && !string.IsNullOrWhiteSpace(retencion.NumeroAutorizacion))
            return Map(retencion);
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

        var compra = retencion.CompraId.HasValue
            ? await dbContext.Compras.AsNoTracking().Include(x => x.Detalles).SingleOrDefaultAsync(x => x.EmpresaId == retencion.EmpresaId && x.Id == retencion.CompraId, cancellationToken)
            : null;
        if (compra is null)
            throw new InvalidOperationException("Vincule una compra con totales, impuestos y forma de pago para emitir una retención ATS 2.0.0. No se inventarán datos del sustento.");
        var xmlGenerado = xmlGenerator.Build(retencion, empresa, proveedor, compra);
        var xmlFirmado = await artifacts.ReadAsync(retencion.EmpresaId, "ComprobanteRetencion", retencion.Id, "XML_FIRMADO", cancellationToken);
        var retry = SriDocumentSubmissionService.HasSignature(xmlFirmado);
        if (!retry)
        {
            var certificado = SriCertificateSelector.Resolve(empresa);
            xmlFirmado = signer.Sign(xmlGenerado, certificado.Contenido, certificado.Clave);
            await artifacts.SaveAsync(retencion.EmpresaId, "SRI", "ComprobanteRetencion", retencion.Id, "XML_GENERADO", $"07-{retencion.Id}-generado.xml", xmlGenerado, cancellationToken);
            await artifacts.SaveAsync(retencion.EmpresaId, "SRI", "ComprobanteRetencion", retencion.Id, "XML_FIRMADO", $"07-{retencion.Id}-firmado.xml", xmlFirmado, cancellationToken);
            retencion.EstadoSRI = EstadoSriRetencion.Firmado;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        try
        {
            var result = await submission.SubmitAsync(((byte)retencion.AmbienteSRI).ToString(), retencion.ClaveAcceso!, xmlFirmado!, retry, cancellationToken);
            retencion.EstadoSRI = result.Estado == "AUTORIZADO" ? EstadoSriRetencion.Autorizado
                : result.Estado is "DEVUELTA" or "NO_AUTORIZADO" ? EstadoSriRetencion.NoAutorizado : EstadoSriRetencion.Enviado;
            retencion.NumeroAutorizacion = result.NumeroAutorizacion;
            retencion.FechaAutorizacion = result.FechaAutorizacion;
            retencion.MensajeErrorSRI = result.Estado == "AUTORIZADO" ? null : result.Mensaje;
            await artifacts.SaveAsync(retencion.EmpresaId, "SRI", "ComprobanteRetencion", retencion.Id, "XML_AUTORIZADO", $"07-{retencion.Id}-autorizado.xml", result.XmlAutorizado, cancellationToken);
        }
        catch (Exception exception) when ((exception is HttpRequestException or TaskCanceledException) && !cancellationToken.IsCancellationRequested)
        {
            retencion.EstadoSRI = EstadoSriRetencion.Enviado;
            retencion.MensajeErrorSRI = "Comunicación SRI pendiente: " + exception.Message;
            logger.LogWarning(exception, "Retención pendiente. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}, RetencionId: {RetencionId}", retencion.EmpresaId, tenantContextAccessor.UserId, retencion.Id);
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
        string ambiente,
        CancellationToken cancellationToken)
    {
        var estab = NormalizeNumeric(establecimiento, 3, "establecimiento");
        var pto = NormalizeNumeric(puntoEmision, 3, "punto de emision");
        var next = await TestDeIa.Infrastructure.Adapters.Out.Facturacion.SriNumeracionAllocator.ReserveAsync(dbContext, empresaId, ambiente, "07", estab, pto, cancellationToken);
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
            ParteRelacionada = entity.ParteRelacionada,
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
