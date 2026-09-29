using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Tesoreria.Ports.In;
using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Infrastructure.Adapters.Out.Tesoreria;

public sealed class BancoParserTestService(
    IBancoParserService bancoParserService,
    ILogger<BancoParserTestService> logger) : IBancoParserTestService
{
    public async Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default)
    {
        var results = new[]
        {
            await ValidarFormatosMultiBancoAsync(cancellationToken),
            await ValidarParsingArchivoGrandeAsync(cancellationToken)
        };

        return new DiagnosticSuiteResultResponse
        {
            Suite = "Parser Extractos Bancarios",
            Resultados = results
        };
    }

    public async Task<DiagnosticTestResultResponse> ValidarFormatosMultiBancoAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            var bancos = new Dictionary<string, string>
            {
                ["Banco Pichincha"] = "Fecha;Documento;Concepto;Debito;Credito\n28/09/2026;TRX-001; TRANSFERENCIA RECIBIDA  CLIENTE QA ;0,00;1.234,56",
                ["Banco Guayaquil"] = "fecha,referencia,descripcion,tipo,monto\n2026-09-28,LOTE-002,PAGO DATAFAST LOTE 002,CREDITO,1234.56",
                ["Banco del Pacífico"] = "Fecha\tComprobante\tDetalle\tDébito\tCrédito\n28-09-2026\tDEP003\tDEPOSITO EN EFECTIVO\t\t500,75",
                ["Produbanco"] = "date|ref|description|movement|amount\n09/28/2026|REF004|NOTA DE DEBITO COMISION|DEBITO|-12.45"
            };

            var evidencias = new Dictionary<string, string>();
            var errores = new List<string>();
            foreach (var banco in bancos)
            {
                await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(banco.Value));
                var parsed = await bancoParserService.ParsearExtractoBancarioAsync(stream, ".csv", banco.Key, cancellationToken);
                var item = parsed.FirstOrDefault();
                if (item is null || item.Monto <= 0 || item.FechaTransaccion == default || string.IsNullOrWhiteSpace(item.ConceptoDescripcion))
                {
                    errores.Add($"No se parseo correctamente el formato de {banco.Key}.");
                    continue;
                }

                evidencias[banco.Key] = $"{item.FechaTransaccion:yyyy-MM-dd}; {item.NumeroDocumentoRef}; {item.TipoMovimiento}; {item.Monto:0.0000}";
            }

            watch.Stop();
            return Result(
                "ValidarFormatosMultiBanco",
                errores.Count == 0,
                errores.Count == 0 ? "VALIDO" : "OBSERVADO",
                errores.Count == 0 ? "CSV multi-banco parseado y normalizado correctamente." : "Se detectaron observaciones de parsing.",
                watch.Elapsed,
                errores,
                evidencias);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Fallo prueba ValidarFormatosMultiBanco.");
            watch.Stop();
            return Failed("ValidarFormatosMultiBanco", exception.Message, watch.Elapsed);
        }
    }

    public async Task<DiagnosticTestResultResponse> ValidarParsingArchivoGrandeAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            var builder = new StringBuilder("Fecha;Documento;Concepto;Debito;Credito\n");
            for (var index = 1; index <= 10_000; index++)
            {
                builder.Append("28/09/2026;TRX-")
                    .Append(index.ToString("000000"))
                    .Append(";TRANSFERENCIA QA;0;")
                    .Append(index % 100 + 1)
                    .Append(",25\n");
            }

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString()));
            var parsed = await bancoParserService.ParsearExtractoBancarioAsync(stream, ".csv", "Banco Pichincha", cancellationToken);
            watch.Stop();

            var ok = parsed.Count == 10_000 && watch.Elapsed < TimeSpan.FromSeconds(10);
            return Result(
                "ValidarParsingArchivoGrande",
                ok,
                ok ? "VALIDO" : "OBSERVADO",
                ok ? "Archivo grande parseado dentro del umbral esperado." : "El parser no cumplio volumen o tiempo esperado.",
                watch.Elapsed,
                ok ? [] : [$"Registros={parsed.Count}; Duracion={watch.Elapsed.TotalSeconds:0.00}s"],
                new Dictionary<string, string>
                {
                    ["Registros"] = parsed.Count.ToString(),
                    ["DuracionMs"] = watch.ElapsedMilliseconds.ToString()
                });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Fallo prueba ValidarParsingArchivoGrande.");
            watch.Stop();
            return Failed("ValidarParsingArchivoGrande", exception.Message, watch.Elapsed);
        }
    }

    private static DiagnosticTestResultResponse Result(
        string name,
        bool succeeded,
        string status,
        string message,
        TimeSpan duration,
        IReadOnlyCollection<string>? errors = null,
        IReadOnlyDictionary<string, string>? evidences = null)
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
}
