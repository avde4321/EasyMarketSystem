using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Tesoreria.Ports.In;
using TestDeIa.Domain.Modules.Integraciones.Enums;
using TestDeIa.Domain.Modules.Tesoreria.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Tesoreria;
using TestDeIa.Shared.Responses.Tesoreria;

namespace TestDeIa.Infrastructure.Adapters.Out.Tesoreria;

public sealed class ConciliacionService(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    ILogger<ConciliacionService> logger) : IConciliacionService
{
    private const int ExactConfidence = 100;
    private const int DateRangeConfidence = 90;
    private const int SuggestedConfidence = 75;
    private static readonly Regex TokenRegex = new(@"[A-Z0-9]{4,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<ConciliacionResultadoDto> EjecutarConciliacionAutomaticaAsync(Guid cuentaId, CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        var cuenta = await dbContext.CuentasBancarias
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Id == cuentaId, cancellationToken)
            ?? throw new InvalidOperationException("La cuenta bancaria no existe para la empresa activa.");

        var extractos = await dbContext.ExtractoBancarioDetalles
            .Include(current => current.ExtractoHeader)
            .ThenInclude(current => current.CuentaBancaria)
            .Where(current =>
                current.EmpresaId == empresaId &&
                current.CuentaBancariaId == cuentaId &&
                !current.Conciliado)
            .OrderBy(current => current.FechaTransaccion)
            .Take(500)
            .ToListAsync(cancellationToken);

        if (extractos.Count == 0)
        {
            return new ConciliacionResultadoDto
            {
                CuentaBancariaId = cuentaId,
                SaldoConciliado = cuenta.SaldoConciliado
            };
        }

        var fechaDesde = extractos.Min(current => current.FechaTransaccion).AddDays(-5);
        var fechaHasta = extractos.Max(current => current.FechaTransaccion).AddDays(5);

        var movimientos = await dbContext.MovimientosTesoreria
            .Where(current =>
                current.EmpresaId == empresaId &&
                current.CuentaBancariaId == cuentaId &&
                current.EstadoConciliacion == EstadoConciliacionTesoreria.Pendiente &&
                current.Fecha >= fechaDesde &&
                current.Fecha <= fechaHasta)
            .ToListAsync(cancellationToken);

        var pagosFactura = await dbContext.FacturaPagos
            .AsNoTracking()
            .Include(current => current.Factura)
            .Where(current =>
                current.EmpresaId == empresaId &&
                current.Factura.FechaEmision >= fechaDesde &&
                current.Factura.FechaEmision <= fechaHasta)
            .ToListAsync(cancellationToken);

        var pagosDigitales = await dbContext.TransaccionesPagosDigitales
            .AsNoTracking()
            .Where(current =>
                current.EmpresaId == empresaId &&
                current.EstadoPago == EstadoPagoDigital.Aprobado &&
                current.FechaAprobacion.HasValue &&
                current.FechaAprobacion.Value.UtcDateTime >= fechaDesde &&
                current.FechaAprobacion.Value.UtcDateTime <= fechaHasta)
            .ToListAsync(cancellationToken);

        var matches = new List<ConciliacionMatchDto>();

        foreach (var extracto in extractos)
        {
            var match = FindExactMatch(extracto, movimientos)
                ?? FindDateRangeMatch(extracto, movimientos)
                ?? FindSuggestedTreasuryMatch(extracto, movimientos)
                ?? FindSuggestedFacturaPaymentMatch(extracto, pagosFactura)
                ?? FindSuggestedDigitalPaymentMatch(extracto, pagosDigitales);

            if (match is null)
            {
                continue;
            }

            if (match.Movimiento is not null && match.Confidence >= DateRangeConfidence)
            {
                extracto.Conciliado = true;
                extracto.MovimientoTesoreriaId = match.Movimiento.Id;
                match.Movimiento.EstadoConciliacion = EstadoConciliacionTesoreria.ConciliadoAuto;
                cuenta.SaldoConciliado += ResolveSignedAmount(extracto);

                matches.Add(ToDto(extracto, match, conciliado: true));
                movimientos.Remove(match.Movimiento);
                continue;
            }

            matches.Add(ToDto(extracto, match, conciliado: false));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var diferencia = cuenta.SaldoContable - cuenta.SaldoConciliado;
        logger.LogInformation(
            "Conciliacion bancaria automatica ejecutada. UsuarioId={UsuarioId}; EmpresaId={EmpresaId}; CuentaBancariaId={CuentaBancariaId}; Evaluados={Evaluados}; ConciliadosAuto={ConciliadosAuto}; Sugerencias={Sugerencias}; SaldoFinal={SaldoFinal}; Diferencia={Diferencia}.",
            tenantContextAccessor.UserId,
            empresaId,
            cuentaId,
            extractos.Count,
            matches.Count(current => current.Conciliado),
            matches.Count(current => !current.Conciliado),
            cuenta.SaldoConciliado,
            diferencia);

        if (Math.Abs(diferencia) > 100m)
        {
            logger.LogWarning(
                "Descuadre bancario superior al umbral configurado. EmpresaId={EmpresaId}; CuentaBancariaId={CuentaBancariaId}; SaldoContable={SaldoContable}; SaldoConciliado={SaldoConciliado}; Diferencia={Diferencia}.",
                empresaId,
                cuentaId,
                cuenta.SaldoContable,
                cuenta.SaldoConciliado,
                diferencia);
        }

        return new ConciliacionResultadoDto
        {
            CuentaBancariaId = cuentaId,
            TotalExtractosEvaluados = extractos.Count,
            TotalConciliados = matches.Count(current => current.Conciliado),
            TotalSugerencias = matches.Count(current => !current.Conciliado),
            SaldoConciliado = cuenta.SaldoConciliado,
            Coincidencias = matches
        };
    }

    public async Task<ConciliacionMatchDto> ConciliarManualAsync(ConciliacionManualDto request, CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        var extracto = await dbContext.ExtractoBancarioDetalles
            .Include(current => current.ExtractoHeader)
            .ThenInclude(current => current.CuentaBancaria)
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Id == request.ExtractoDetalleId, cancellationToken)
            ?? throw new InvalidOperationException("El movimiento del extracto no existe.");

        if (extracto.CuentaBancariaId != extracto.ExtractoHeader.CuentaBancariaId)
        {
            throw new InvalidOperationException("El movimiento del extracto tiene una referencia bancaria inconsistente.");
        }

        var movimiento = await dbContext.MovimientosTesoreria
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.Id == request.MovimientoTesoreriaId &&
                current.CuentaBancariaId == extracto.CuentaBancariaId,
                cancellationToken)
            ?? throw new InvalidOperationException("El movimiento de tesorería no existe para la cuenta seleccionada.");

        if (extracto.Conciliado)
        {
            throw new InvalidOperationException("El movimiento del extracto ya se encuentra conciliado.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        extracto.Conciliado = true;
        extracto.MovimientoTesoreriaId = movimiento.Id;
        movimiento.EstadoConciliacion = request.EsAjusteAutomatico
            ? EstadoConciliacionTesoreria.ConciliadoAuto
            : EstadoConciliacionTesoreria.ConciliadoManual;
        extracto.ExtractoHeader.CuentaBancaria.SaldoConciliado += ResolveSignedAmount(extracto);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Conciliacion bancaria manual ejecutada. UsuarioId={UsuarioId}; EmpresaId={EmpresaId}; CuentaBancariaId={CuentaBancariaId}; ExtractoDetalleId={ExtractoDetalleId}; MovimientoTesoreriaId={MovimientoTesoreriaId}; SaldoFinal={SaldoFinal}.",
            tenantContextAccessor.UserId,
            empresaId,
            extracto.ExtractoHeader.CuentaBancariaId,
            extracto.Id,
            movimiento.Id,
            extracto.ExtractoHeader.CuentaBancaria.SaldoConciliado);

        return new ConciliacionMatchDto
        {
            ExtractoDetalleId = extracto.Id,
            MovimientoTesoreriaId = movimiento.Id,
            NumeroDocumentoRef = extracto.NumeroDocumentoRef,
            Monto = extracto.Monto,
            FechaExtracto = extracto.FechaTransaccion,
            FechaMovimiento = movimiento.Fecha,
            Nivel = request.EsAjusteAutomatico ? 3 : 1,
            Confianza = request.EsAjusteAutomatico ? SuggestedConfidence : ExactConfidence,
            Conciliado = true,
            Motivo = request.EsAjusteAutomatico ? "Conciliación manual con ajuste automático." : "Conciliación manual confirmada por usuario."
        };
    }

    private static MatchCandidate? FindExactMatch(ExtractoBancarioDetalleEntity extracto, IReadOnlyCollection<MovimientoTesoreriaEntity> movimientos)
    {
        if (string.IsNullOrWhiteSpace(extracto.NumeroDocumentoRef))
        {
            return null;
        }

        var movimiento = movimientos.FirstOrDefault(current =>
            AmountEquals(current.Monto, extracto.Monto) &&
            ReferenceEquals(extracto.NumeroDocumentoRef, current.Id.ToString("N")) ||
            AmountEquals(current.Monto, extracto.Monto) && TextContainsToken(current.Beneficiario, extracto.NumeroDocumentoRef));

        return movimiento is null ? null : new MatchCandidate(movimiento, 1, ExactConfidence, "Referencia/documento y monto exactos.");
    }

    private static MatchCandidate? FindDateRangeMatch(ExtractoBancarioDetalleEntity extracto, IReadOnlyCollection<MovimientoTesoreriaEntity> movimientos)
    {
        var tokens = ExtractTokens(extracto.ConceptoDescripcion);
        var movimiento = movimientos
            .Where(current => AmountEquals(current.Monto, extracto.Monto) && BusinessDaysBetween(current.Fecha.Date, extracto.FechaTransaccion.Date) <= 3)
            .OrderByDescending(current => tokens.Count(token => current.Beneficiario.Contains(token, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(current => tokens.Count == 0 || tokens.Any(token => current.Beneficiario.Contains(token, StringComparison.OrdinalIgnoreCase)));

        return movimiento is null ? null : new MatchCandidate(movimiento, 2, DateRangeConfidence, "Monto exacto, fecha dentro de +/- 3 días hábiles y coincidencia de concepto.");
    }

    private static MatchCandidate? FindSuggestedTreasuryMatch(ExtractoBancarioDetalleEntity extracto, IReadOnlyCollection<MovimientoTesoreriaEntity> movimientos)
    {
        var tokens = ExtractTokens($"{extracto.NumeroDocumentoRef} {extracto.ConceptoDescripcion}");
        var movimiento = movimientos.FirstOrDefault(current =>
            AmountEquals(current.Monto, extracto.Monto) &&
            tokens.Any(token => current.Beneficiario.Contains(token, StringComparison.OrdinalIgnoreCase)));

        return movimiento is null ? null : new MatchCandidate(movimiento, 3, SuggestedConfidence, "Sugerencia por referencia parcial o concepto similar.");
    }

    private static MatchCandidate? FindSuggestedFacturaPaymentMatch(ExtractoBancarioDetalleEntity extracto, IReadOnlyCollection<FacturaPagoEntity> pagos)
    {
        var payment = pagos.FirstOrDefault(current =>
            AmountEquals(current.Monto, extracto.Monto) &&
            (TextContainsToken(extracto.ConceptoDescripcion, current.NumeroReferencia) ||
             TextContainsToken(extracto.ConceptoDescripcion, current.VoucherNumero) ||
             TextContainsToken(extracto.ConceptoDescripcion, current.LoteNumero) ||
             TextContainsToken(extracto.NumeroDocumentoRef, current.NumeroReferencia)));

        return payment is null
            ? null
            : new MatchCandidate(null, 3, SuggestedConfidence, $"Sugerencia contra pago de factura {payment.FacturaId:N}.");
    }

    private static MatchCandidate? FindSuggestedDigitalPaymentMatch(ExtractoBancarioDetalleEntity extracto, IReadOnlyCollection<TransaccionPagoDigitalEntity> pagosDigitales)
    {
        var payment = pagosDigitales.FirstOrDefault(current =>
            AmountEquals(current.Monto, extracto.Monto) &&
            (TextContainsToken(extracto.ConceptoDescripcion, current.TransactionIdPasarela) ||
             TextContainsToken(extracto.NumeroDocumentoRef, current.TransactionIdPasarela) ||
             extracto.ConceptoDescripcion.Contains(current.Pasarela, StringComparison.OrdinalIgnoreCase)));

        return payment is null
            ? null
            : new MatchCandidate(null, 3, SuggestedConfidence, $"Sugerencia contra lote de cobro digital {payment.Pasarela} / {payment.TransactionIdPasarela}.");
    }

    private Guid ResolveEmpresaId()
    {
        return tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa para conciliación bancaria.");
    }

    private static ConciliacionMatchDto ToDto(ExtractoBancarioDetalleEntity extracto, MatchCandidate match, bool conciliado)
    {
        return new ConciliacionMatchDto
        {
            ExtractoDetalleId = extracto.Id,
            MovimientoTesoreriaId = match.Movimiento?.Id,
            NumeroDocumentoRef = extracto.NumeroDocumentoRef,
            Monto = extracto.Monto,
            FechaExtracto = extracto.FechaTransaccion,
            FechaMovimiento = match.Movimiento?.Fecha,
            Nivel = match.Level,
            Confianza = match.Confidence,
            Conciliado = conciliado,
            Motivo = match.Reason
        };
    }

    private static decimal ResolveSignedAmount(ExtractoBancarioDetalleEntity extracto)
    {
        return extracto.TipoMovimiento == TipoMovimientoBancario.RetiroDebito
            ? -extracto.Monto
            : extracto.Monto;
    }

    private static bool AmountEquals(decimal left, decimal right)
    {
        return decimal.Round(left, 2, MidpointRounding.AwayFromZero) == decimal.Round(right, 2, MidpointRounding.AwayFromZero);
    }

    private static bool ReferenceEquals(string left, string right)
    {
        return !string.IsNullOrWhiteSpace(left) &&
            !string.IsNullOrWhiteSpace(right) &&
            NormalizeReference(left) == NormalizeReference(right);
    }

    private static bool TextContainsToken(string text, string? token)
    {
        return !string.IsNullOrWhiteSpace(text) &&
            !string.IsNullOrWhiteSpace(token) &&
            text.Contains(token, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeReference(string value)
    {
        return Regex.Replace(value, @"[^A-Z0-9]", string.Empty, RegexOptions.IgnoreCase).ToUpperInvariant();
    }

    private static IReadOnlyCollection<string> ExtractTokens(string value)
    {
        return TokenRegex.Matches(value)
            .Select(current => current.Value.ToUpperInvariant())
            .Distinct()
            .ToArray();
    }

    private static int BusinessDaysBetween(DateTime left, DateTime right)
    {
        var start = left <= right ? left : right;
        var end = left <= right ? right : left;
        var days = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                days++;
            }
        }

        return Math.Max(0, days - 1);
    }

    private sealed record MatchCandidate(MovimientoTesoreriaEntity? Movimiento, int Level, int Confidence, string Reason);
}
