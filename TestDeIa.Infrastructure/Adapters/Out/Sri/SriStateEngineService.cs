using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Sri.Models;
using TestDeIa.Application.Modules.Sri.Ports.In;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Sri;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Sri;

public sealed class SriStateEngineService(
    TestDeIaDbContext dbContext,
    ILogger<SriStateEngineService> logger) : ISriStateEngineService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> AllowedTransitions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [SriOutboxEstados.Pendiente] = StateSet(SriEstadosComprobante.Generado, SriEstadosComprobante.Firmado, SriEstadosComprobante.Devuelta, SriOutboxEstados.Error),
            [SriOutboxEstados.Error] = StateSet(SriEstadosComprobante.Generado, SriEstadosComprobante.Firmado, SriEstadosComprobante.Devuelta),
            [SriOutboxEstados.Devuelto] = StateSet(SriEstadosComprobante.Generado, SriEstadosComprobante.Firmado),
            [SriEstadosComprobante.Generado] = StateSet(SriEstadosComprobante.Firmado, SriEstadosComprobante.Devuelta),
            [SriEstadosComprobante.Firmado] = StateSet(SriEstadosComprobante.EnProceso, SriEstadosComprobante.Devuelta),
            [SriEstadosComprobante.EnProceso] = StateSet(SriEstadosComprobante.Autorizado, SriEstadosComprobante.NoAutorizado, SriEstadosComprobante.Devuelta),
            [SriEstadosComprobante.Devuelta] = StateSet(SriEstadosComprobante.Generado, SriEstadosComprobante.Firmado),
            [SriEstadosComprobante.NoAutorizado] = StateSet(SriEstadosComprobante.Generado, SriEstadosComprobante.Firmado, SriEstadosComprobante.Anulado),
            [SriEstadosComprobante.Autorizado] = StateSet(SriEstadosComprobante.Anulado),
            [SriEstadosComprobante.Anulado] = StateSet()
        };

    public async Task<SriTransitionResult> ApplyTransitionAsync(
        SriTransitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var estadoNuevoCodigo = Normalize(request.EstadoNuevoCodigo);
        var estadoNuevo = await dbContext.SriEstadosComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Codigo == estadoNuevoCodigo, cancellationToken)
            ?? throw new InvalidOperationException($"El estado SRI '{request.EstadoNuevoCodigo}' no existe en el catálogo.");

        var outbox = await dbContext.ColaProcesamientoSRI
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == request.EmpresaId &&
                current.ComprobanteId == request.ComprobanteId &&
                current.TipoDocumentoId == request.TipoDocumentoId,
                cancellationToken);

        var estadoAnteriorCodigo = outbox?.Estado ?? await ResolveEstadoActualComprobanteAsync(request, cancellationToken);
        estadoAnteriorCodigo = Normalize(estadoAnteriorCodigo);

        if (!CanTransition(estadoAnteriorCodigo, estadoNuevoCodigo))
        {
            throw new InvalidOperationException($"Transición SRI no permitida: {estadoAnteriorCodigo} -> {estadoNuevoCodigo}.");
        }

        var estadoAnterior = await dbContext.SriEstadosComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Codigo == estadoAnteriorCodigo, cancellationToken);

        if (outbox is not null)
        {
            outbox.Estado = estadoNuevoCodigo;
            outbox.Mensaje = request.MensajeRespuesta;
            outbox.UltimoError = estadoNuevoCodigo is SriEstadosComprobante.Devuelta or SriEstadosComprobante.NoAutorizado
                ? request.MensajeRespuesta
                : null;
            outbox.UpdatedAt = DateTimeOffset.UtcNow;
            outbox.ProcessingNode = estadoNuevo.EsEstadoFinal ? null : outbox.ProcessingNode;
            outbox.ProcessingStartedAt = estadoNuevo.EsEstadoFinal ? null : outbox.ProcessingStartedAt;
        }

        await ApplyTransactionalDocumentStateAsync(request, estadoNuevoCodigo, cancellationToken);

        dbContext.SriHistorialEstadosComprobante.Add(new SriHistorialEstadoComprobanteEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = request.EmpresaId,
            ComprobanteId = request.ComprobanteId,
            TipoDocumentoId = request.TipoDocumentoId,
            EstadoAnteriorId = estadoAnterior?.Id,
            EstadoNuevoId = estadoNuevo.Id,
            CodigoErrorSri = request.CodigoErrorSri,
            MensajeRespuesta = request.MensajeRespuesta,
            FechaTransaccion = DateTimeOffset.UtcNow,
            UsuarioId = request.UsuarioId,
            WorkerNode = request.WorkerNode
        });

        logger.LogInformation(
            "Transición SRI aplicada. Empresa={EmpresaId}; Comprobante={ComprobanteId}; Tipo={TipoDocumento}; Estado={EstadoAnterior}->{EstadoNuevo}.",
            request.EmpresaId,
            request.ComprobanteId,
            request.TipoDocumentoId,
            estadoAnteriorCodigo,
            estadoNuevoCodigo);

        return new SriTransitionResult
        {
            Succeeded = true,
            EstadoAnterior = estadoAnteriorCodigo,
            EstadoNuevo = estadoNuevoCodigo,
            Message = $"Estado SRI actualizado a {estadoNuevoCodigo}."
        };
    }

    public async Task<IReadOnlyCollection<SriCatalogoErrorResponse>> GetErroresAsync(
        string? term = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term?.Trim();
        var query = dbContext.SriCatalogoErrores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(current =>
                current.CodigoSri.Contains(normalizedTerm) ||
                current.MensajeSri.Contains(normalizedTerm) ||
                current.SolucionSugerida.Contains(normalizedTerm));
        }

        return await query
            .OrderBy(current => current.CodigoSri)
            .Select(current => MapError(current))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SriCatalogoErrorResponse?> GetErrorByCodigoAsync(
        string codigoSri,
        CancellationToken cancellationToken = default)
    {
        var codigo = codigoSri.Trim();
        return await dbContext.SriCatalogoErrores
            .AsNoTracking()
            .Where(current => current.CodigoSri == codigo)
            .Select(current => MapError(current))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SriEstadoComprobanteResponse>> GetEstadosAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SriEstadosComprobante
            .AsNoTracking()
            .OrderBy(current => current.Id)
            .Select(current => new SriEstadoComprobanteResponse
            {
                Id = current.Id,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Descripcion = current.Descripcion,
                RequiereReenvioRecepcion = current.RequiereReenvioRecepcion,
                RequiereConsultaAutorizacion = current.RequiereConsultaAutorizacion,
                EsEstadoFinal = current.EsEstadoFinal,
                EsEditable = current.EsEditable
            })
            .ToArrayAsync(cancellationToken);
    }

    private async Task<string> ResolveEstadoActualComprobanteAsync(SriTransitionRequest request, CancellationToken cancellationToken)
    {
        if (request.TipoDocumentoId == "01")
        {
            var facturaEstado = await dbContext.Facturas
                .IgnoreQueryFilters()
                .Where(current => current.EmpresaId == request.EmpresaId && current.Id == request.ComprobanteId)
                .Select(current => current.Estado)
                .FirstOrDefaultAsync(cancellationToken);

            return MapFacturaEstadoToSri(facturaEstado);
        }

        var comprobanteEstado = await dbContext.ComprobanteCabecera
            .IgnoreQueryFilters()
            .Where(current => current.EmpresaId == request.EmpresaId && current.Id == request.ComprobanteId)
            .Select(current => current.Estado)
            .FirstOrDefaultAsync(cancellationToken);

        return MapFacturaEstadoToSri(comprobanteEstado);
    }

    private async Task ApplyTransactionalDocumentStateAsync(SriTransitionRequest request, string estadoNuevoCodigo, CancellationToken cancellationToken)
    {
        var facturaEstado = MapSriEstadoToFacturaEstado(estadoNuevoCodigo);
        if (facturaEstado is null)
        {
            return;
        }

        if (request.TipoDocumentoId == "01")
        {
            var factura = await dbContext.Facturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(current => current.EmpresaId == request.EmpresaId && current.Id == request.ComprobanteId, cancellationToken);

            if (factura is not null)
            {
                factura.Estado = facturaEstado.Value;
                factura.MensajeEstado = request.MensajeRespuesta;
                factura.UpdatedAt = DateTimeOffset.UtcNow;
            }

            return;
        }

        var comprobante = await dbContext.ComprobanteCabecera
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.EmpresaId == request.EmpresaId && current.Id == request.ComprobanteId, cancellationToken);

        if (comprobante is not null)
        {
            comprobante.Estado = facturaEstado.Value;
            comprobante.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    private static bool CanTransition(string estadoAnteriorCodigo, string estadoNuevoCodigo)
    {
        return AllowedTransitions.TryGetValue(estadoAnteriorCodigo, out var allowed)
            ? allowed.Contains(estadoNuevoCodigo)
            : string.Equals(estadoAnteriorCodigo, estadoNuevoCodigo, StringComparison.OrdinalIgnoreCase);
    }

    private static FacturaEstado? MapSriEstadoToFacturaEstado(string estadoCodigo) => estadoCodigo switch
    {
        SriEstadosComprobante.Generado or SriEstadosComprobante.Firmado => FacturaEstado.NO_FIRMADO,
        SriEstadosComprobante.EnProceso => FacturaEstado.PENDIENTE,
        SriEstadosComprobante.Autorizado => FacturaEstado.AUTORIZADO,
        SriEstadosComprobante.Devuelta or SriEstadosComprobante.NoAutorizado or SriEstadosComprobante.Anulado => FacturaEstado.RECHAZADO,
        _ => null
    };

    private static string MapFacturaEstadoToSri(FacturaEstado estado) => estado switch
    {
        FacturaEstado.NO_FIRMADO => SriEstadosComprobante.Generado,
        FacturaEstado.PENDIENTE => SriEstadosComprobante.EnProceso,
        FacturaEstado.AUTORIZADO => SriEstadosComprobante.Autorizado,
        FacturaEstado.RECHAZADO => SriEstadosComprobante.NoAutorizado,
        _ => SriEstadosComprobante.Generado
    };

    private static SriCatalogoErrorResponse MapError(SriCatalogoErrorEntity current)
    {
        return new SriCatalogoErrorResponse
        {
            Id = current.Id,
            CodigoSri = current.CodigoSri,
            MensajeSri = current.MensajeSri,
            SolucionSugerida = current.SolucionSugerida,
            TipoError = current.TipoError.ToString()
        };
    }

    private static string Normalize(string? value)
    {
        if (string.Equals(value, SriOutboxEstados.Devuelto, StringComparison.OrdinalIgnoreCase))
        {
            return SriEstadosComprobante.Devuelta;
        }

        return string.IsNullOrWhiteSpace(value)
            ? SriEstadosComprobante.Generado
            : value.Trim().ToUpperInvariant();
    }

    private static IReadOnlySet<string> StateSet(params string[] states)
    {
        return new HashSet<string>(states, StringComparer.OrdinalIgnoreCase);
    }
}
