using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Tesoreria.Ports.In;
using TestDeIa.Domain.Modules.Contabilidad.Enums;
using TestDeIa.Domain.Modules.Tesoreria.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Infrastructure.Adapters.Out.Tesoreria;

public sealed class ConciliacionTestService(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    ITesoreriaService tesoreriaService,
    IConciliacionService conciliacionService,
    ILogger<ConciliacionTestService> logger) : IConciliacionTestService
{
    public async Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default)
    {
        var results = new[]
        {
            await ValidarMatchingExactoMontoYReferenciaAsync(cancellationToken),
            await ValidarMatchingPorRangoDeFechasAsync(cancellationToken),
            await ValidarManejoComisionesYBancosAsync(cancellationToken),
            await EvitarDuplicadosExtractoTestAsync(cancellationToken)
        };

        return new DiagnosticSuiteResultResponse
        {
            Suite = "Conciliación Bancaria",
            Resultados = results
        };
    }

    public async Task<DiagnosticTestResultResponse> ValidarMatchingExactoMontoYReferenciaAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var context = await EnsureContextAsync(cancellationToken);
            var referencia = "TRX-EXACT-001";
            var movimiento = AddMovimiento(context, referencia, 125.50m, DateTime.Today, "Cliente QA TRX-EXACT-001");
            AddExtracto(context, referencia, 125.50m, DateTime.Today, "TRANSFERENCIA TRX-EXACT-001");
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = await conciliacionService.EjecutarConciliacionAutomaticaAsync(context.Cuenta.Id, cancellationToken);
            await transaction.RollbackAsync(cancellationToken);
            watch.Stop();

            var match = result.Coincidencias.FirstOrDefault(current => current.MovimientoTesoreriaId == movimiento.Id);
            var ok = match is { Conciliado: true, Confianza: 100 };
            return Result(
                "ValidarMatchingExactoMontoYReferencia",
                ok,
                ok ? "VALIDO" : "OBSERVADO",
                ok ? "El match exacto por monto/referencia fue conciliado automaticamente al 100%." : "No se obtuvo match exacto esperado.",
                watch.Elapsed,
                ok ? [] : ["La transferencia no fue conciliada al 100%."],
                new Dictionary<string, string>
                {
                    ["Referencia"] = referencia,
                    ["Conciliados"] = result.TotalConciliados.ToString(),
                    ["Confianza"] = match?.Confianza.ToString() ?? "N/A"
                });
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(exception, "Fallo prueba ValidarMatchingExactoMontoYReferencia.");
            watch.Stop();
            return Failed("ValidarMatchingExactoMontoYReferencia", exception.Message, watch.Elapsed);
        }
    }

    public async Task<DiagnosticTestResultResponse> ValidarMatchingPorRangoDeFechasAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var context = await EnsureContextAsync(cancellationToken);
            var fechaLibro = DateTime.Today.AddDays(-2);
            var fechaBanco = DateTime.Today;
            var movimiento = AddMovimiento(context, "SIN-REF-RANGO", 240.75m, fechaLibro, "Cliente Mayorista QA");
            AddExtracto(context, string.Empty, 240.75m, fechaBanco, "ACREDITACION CLIENTE MAYORISTA QA");
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = await conciliacionService.EjecutarConciliacionAutomaticaAsync(context.Cuenta.Id, cancellationToken);
            await transaction.RollbackAsync(cancellationToken);
            watch.Stop();

            var match = result.Coincidencias.FirstOrDefault(current => current.MovimientoTesoreriaId == movimiento.Id);
            var ok = match is { Conciliado: true, Confianza: 90 };
            return Result(
                "ValidarMatchingPorRangoDeFechas",
                ok,
                ok ? "VALIDO" : "OBSERVADO",
                ok ? "El match por monto, rango de fechas y concepto se concilio con 90%." : "No se obtuvo match por rango esperado.",
                watch.Elapsed,
                ok ? [] : ["La acreditacion no fue detectada por rango de fechas/concepto."],
                new Dictionary<string, string>
                {
                    ["FechaLibro"] = fechaLibro.ToString("yyyy-MM-dd"),
                    ["FechaBanco"] = fechaBanco.ToString("yyyy-MM-dd"),
                    ["Confianza"] = match?.Confianza.ToString() ?? "N/A"
                });
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(exception, "Fallo prueba ValidarMatchingPorRangoDeFechas.");
            watch.Stop();
            return Failed("ValidarMatchingPorRangoDeFechas", exception.Message, watch.Elapsed);
        }
    }

    public async Task<DiagnosticTestResultResponse> ValidarManejoComisionesYBancosAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var context = await EnsureContextAsync(cancellationToken);
            AddExtracto(context, "COMISION-001", 3.25m, DateTime.Today, "NOTA DE DEBITO COMISION BANCARIA", TipoMovimientoBancario.RetiroDebito);
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = await conciliacionService.EjecutarConciliacionAutomaticaAsync(context.Cuenta.Id, cancellationToken);
            var asientoGenerado = await dbContext.AsientosContables.AnyAsync(current => current.EmpresaId == context.EmpresaId && current.Concepto.Contains("COMISION"), cancellationToken);
            await transaction.RollbackAsync(cancellationToken);
            watch.Stop();

            var ok = result.TotalConciliados == 0 && !asientoGenerado;
            return Result(
                "ValidarManejoComisionesYBancos",
                ok,
                ok ? "VALIDO_CON_OBSERVACION" : "OBSERVADO",
                "El motor detecta comisiones bancarias como extractos no conciliados. La generación de asiento automático queda bloqueada hasta parametrizar cuenta de gasto bancaria.",
                watch.Elapsed,
                ok ? [] : ["Se genero un asiento sin parametrizacion explicita de cuenta de gasto bancaria."],
                new Dictionary<string, string>
                {
                    ["AsientoAutomaticoGenerado"] = asientoGenerado.ToString(),
                    ["Recomendacion"] = "Agregar ParametrosTesoreria: CuentaGastoComisionBancariaId y CuentaBancoContrapartidaId antes de automatizar NIIF."
                });
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(exception, "Fallo prueba ValidarManejoComisionesYBancos.");
            watch.Stop();
            return Failed("ValidarManejoComisionesYBancos", exception.Message, watch.Elapsed);
        }
    }

    public async Task<DiagnosticTestResultResponse> EvitarDuplicadosExtractoTestAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var context = await EnsureContextAsync(cancellationToken);
            var csv = "Fecha;Documento;Concepto;Debito;Credito\n28/09/2026;DUP-001;TRANSFERENCIA DUPLICADA;0;88,10";
            await using var first = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            await using var second = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var firstHeader = await tesoreriaService.ImportarExtractoAsync(context.Cuenta.Id, first, "duplicado.csv", ".csv", cancellationToken);
            var secondHeader = await tesoreriaService.ImportarExtractoAsync(context.Cuenta.Id, second, "duplicado.csv", ".csv", cancellationToken);
            var count = await dbContext.ExtractoBancarioDetalles.CountAsync(current => current.ExtractoHeader.CuentaBancariaId == context.Cuenta.Id && current.NumeroDocumentoRef == "DUP-001", cancellationToken);

            await transaction.RollbackAsync(cancellationToken);
            watch.Stop();

            var ok = firstHeader.TotalRegistros == 1 && secondHeader.TotalRegistros == 0 && count == 1;
            return Result(
                "EvitarDuplicadosExtractoTest",
                ok,
                ok ? "VALIDO" : "OBSERVADO",
                ok ? "La segunda importacion omitio filas duplicadas por fecha/referencia/monto." : "Se detecto duplicacion de extractos.",
                watch.Elapsed,
                ok ? [] : [$"Primera={firstHeader.TotalRegistros}; Segunda={secondHeader.TotalRegistros}; Count={count}"],
                new Dictionary<string, string>
                {
                    ["PrimeraImportacion"] = firstHeader.TotalRegistros.ToString(),
                    ["SegundaImportacion"] = secondHeader.TotalRegistros.ToString(),
                    ["DetallesPersistidosAntesRollback"] = count.ToString()
                });
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(exception, "Fallo prueba EvitarDuplicadosExtractoTest.");
            watch.Stop();
            return Failed("EvitarDuplicadosExtractoTest", exception.Message, watch.Elapsed);
        }
    }

    private async Task<TestContext> EnsureContextAsync(CancellationToken cancellationToken)
    {
        var empresaId = tenantContextAccessor.EmpresaId
            ?? await dbContext.EmpresasEmisoras.IgnoreQueryFilters().OrderBy(current => current.RazonSocial).Select(current => current.Id).FirstAsync(cancellationToken);

        var cuentaContable = await dbContext.CuentasContables.IgnoreQueryFilters().FirstOrDefaultAsync(current => current.EmpresaId == empresaId, cancellationToken)
            ?? new CuentaContableEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                Codigo = $"QA-{Guid.NewGuid():N}"[..20],
                Nombre = "Cuenta QA Tesoreria",
                Nivel = 1,
                TipoCuenta = TipoCuentaContable.Activo,
                EsAceptable = true,
                SaldoActual = 0m
            };

        if (dbContext.Entry(cuentaContable).State == EntityState.Detached)
        {
            dbContext.CuentasContables.Add(cuentaContable);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var cuenta = new CuentaBancariaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            BancoNombre = "Banco QA",
            TipoCuenta = TipoCuentaBancaria.Corriente,
            NumeroCuenta = $"QA-{Guid.NewGuid():N}"[..20],
            SaldoContable = 1000m,
            SaldoConciliado = 0m,
            Moneda = "USD",
            CuentaContableId = cuentaContable.Id,
            Activa = true
        };

        dbContext.CuentasBancarias.Add(cuenta);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new TestContext(empresaId, cuenta);
    }

    private MovimientoTesoreriaEntity AddMovimiento(TestContext context, string referencia, decimal monto, DateTime fecha, string beneficiario)
    {
        var entity = new MovimientoTesoreriaEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = context.EmpresaId,
            CuentaBancariaId = context.Cuenta.Id,
            Fecha = fecha.Date,
            Tipo = TipoMovimientoTesoreria.Ingreso,
            Monto = monto,
            Beneficiario = $"{beneficiario} {referencia}",
            EstadoConciliacion = EstadoConciliacionTesoreria.Pendiente
        };
        dbContext.MovimientosTesoreria.Add(entity);
        return entity;
    }

    private void AddExtracto(TestContext context, string referencia, decimal monto, DateTime fecha, string concepto, TipoMovimientoBancario tipo = TipoMovimientoBancario.DepositoCredito)
    {
        var header = new ExtractoBancarioHeaderEntity
        {
            Id = Guid.NewGuid(),
            CuentaBancariaId = context.Cuenta.Id,
            FechaImportacion = DateTime.UtcNow,
            FechaDesde = fecha.Date,
            FechaHasta = fecha.Date,
            NombreArchivoOriginal = "qa.csv",
            TotalRegistros = 1
        };
        header.Detalles.Add(new ExtractoBancarioDetalleEntity
        {
            Id = Guid.NewGuid(),
            ExtractoHeaderId = header.Id,
            FechaTransaccion = fecha.Date,
            NumeroDocumentoRef = referencia,
            ConceptoDescripcion = concepto,
            TipoMovimiento = tipo,
            Monto = monto,
            Conciliado = false
        });
        dbContext.ExtractoBancarioHeaders.Add(header);
    }

    private static DiagnosticTestResultResponse Result(string name, bool succeeded, string status, string message, TimeSpan duration, IReadOnlyCollection<string>? errors = null, IReadOnlyDictionary<string, string>? evidences = null)
    {
        return new DiagnosticTestResultResponse
        {
            NombrePrueba = name,
            Succeeded = succeeded,
            Estado = status,
            Mensaje = message,
            Duracion = duration,
            Errores = errors ?? Array.Empty<string>(),
            Evidencias = evidences ?? new Dictionary<string, string>()
        };
    }

    private static DiagnosticTestResultResponse Failed(string name, string message, TimeSpan duration)
    {
        return Result(name, false, "ERROR", message, duration, [message]);
    }

    private sealed record TestContext(Guid EmpresaId, CuentaBancariaEntity Cuenta);
}
