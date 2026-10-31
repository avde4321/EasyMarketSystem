using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.XmlSri.Ports.In;
using TestDeIa.Domain.Modules.XmlSri.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Compras;
using TestDeIa.Shared.Requests.XmlSri;
using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Infrastructure.Adapters.Out.XmlSri;

public sealed class XmlComprasTestService(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    IXmlSriParserService xmlSriParserService,
    ILogger<XmlComprasTestService> logger) : IXmlComprasTestService
{
    public async Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default)
    {
        var results = new[]
        {
            await ValidarEstructuraXmlSriTestAsync(cancellationToken),
            await EvitarDuplicadosXmlTestAsync(cancellationToken),
            await ConversionACompraEInventarioTestAsync(cancellationToken)
        };

        return new DiagnosticSuiteResultResponse
        {
            Suite = "Importador XML Compras SRI",
            Resultados = results
        };
    }

    public Task<DiagnosticTestResultResponse> ValidarEstructuraXmlSriTestAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var xmls = new[] { BuildFacturaXml("1.0.0", BuildClaveAcceso("1000000000000000000000000000000000000000000000001")), BuildFacturaXml("1.1.0", BuildClaveAcceso("1000000000000000000000000000000000000000000000002")) };
            var parsed = xmls.Select(xmlSriParserService.ParsearXmlFacturaSri).ToArray();
            var errors = new List<string>();

            foreach (var item in parsed)
            {
                if (item.RucEmisor.Length != 13)
                {
                    errors.Add("RUC emisor no fue extraido correctamente.");
                }

                if (item.ClaveAcceso.Length != 49)
                {
                    errors.Add("Clave de acceso no tiene 49 digitos.");
                }

                if (item.Detalles.Count == 0 || item.Detalles.Any(detail => detail.Impuestos.Count == 0))
                {
                    errors.Add("Detalle o tarifa IVA no fue extraido correctamente.");
                }

                if (item.TotalSinImpuestos <= 0 || item.ImporteTotal <= 0)
                {
                    errors.Add("Totales SRI no fueron extraidos correctamente.");
                }
            }

            watch.Stop();
            return Task.FromResult(Result(
                "ValidarEstructuraXmlSriTest",
                errors.Count == 0,
                errors.Count == 0 ? "VALIDO" : "OBSERVADO",
                errors.Count == 0 ? "XML SRI v1.0.0 y v1.1.0 deserializados correctamente." : "Se encontraron observaciones al parsear XML SRI.",
                watch.Elapsed,
                errors,
                new Dictionary<string, string>
                {
                    ["Versiones"] = "1.0.0, 1.1.0",
                    ["RucEmisor"] = parsed[0].RucEmisor,
                    ["Secuencial"] = parsed[0].EstabPuntoEmiSecuencial,
                    ["BaseImponible"] = parsed[0].TotalSinImpuestos.ToString("0.0000"),
                    ["Iva"] = parsed[0].TotalConImpuestos.Sum(current => current.Valor).ToString("0.0000"),
                    ["Total"] = parsed[0].ImporteTotal.ToString("0.0000")
                }));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Fallo prueba ValidarEstructuraXmlSriTest.");
            watch.Stop();
            return Task.FromResult(Failed("ValidarEstructuraXmlSriTest", exception.Message, watch.Elapsed));
        }
    }

    public async Task<DiagnosticTestResultResponse> EvitarDuplicadosXmlTestAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var empresaId = await ResolveEmpresaIdAsync(cancellationToken);
            var clave = BuildClaveAcceso("2000000000000000000000000000000000000000000000001");
            var xml = BuildFacturaXml("1.1.0", clave);

            var first = await xmlSriParserService.ProcesarArchivosXmlMasivosAsync([xml], empresaId, cancellationToken);
            var second = await xmlSriParserService.ProcesarArchivosXmlMasivosAsync([xml], empresaId, cancellationToken);
            await transaction.RollbackAsync(cancellationToken);
            watch.Stop();

            var ok = first.Insertados == 1 && second.Duplicados + second.Actualizados >= 1 && second.Errores == 0;
            return Result(
                "EvitarDuplicadosXmlTest",
                ok,
                ok ? "VALIDO" : "OBSERVADO",
                ok ? "La segunda carga del mismo XML no lanzo excepcion no controlada." : "La validacion de duplicados no tuvo el resultado esperado.",
                watch.Elapsed,
                ok ? [] : [$"Primera carga: insertados={first.Insertados}, errores={first.Errores}. Segunda: duplicados={second.Duplicados}, actualizados={second.Actualizados}, errores={second.Errores}."],
                new Dictionary<string, string>
                {
                    ["ClaveAcceso"] = clave,
                    ["PrimeraCarga"] = $"Insertados={first.Insertados}; Errores={first.Errores}",
                    ["SegundaCarga"] = $"Duplicados={second.Duplicados}; Actualizados={second.Actualizados}; Errores={second.Errores}"
                });
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(exception, "Fallo prueba EvitarDuplicadosXmlTest.");
            watch.Stop();
            return Failed("EvitarDuplicadosXmlTest", exception.Message, watch.Elapsed);
        }
    }

    public async Task<DiagnosticTestResultResponse> ConversionACompraEInventarioTestAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var empresaId = await ResolveEmpresaIdAsync(cancellationToken);
            var bodega = await dbContext.Bodegas.AsNoTracking().FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.IsActive, cancellationToken);
            var producto = await dbContext.Productos.AsNoTracking().FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.IsActive && current.ControlaStock, cancellationToken);

            if (bodega is null || producto is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                watch.Stop();
                return Result(
                    "ConversionACompraEInventarioTest",
                    true,
                    "OMITIDA",
                    "No hay bodega o producto inventariable activo para simular la conversion sin preparar datos adicionales.",
                    watch.Elapsed);
            }

            var clave = BuildClaveAcceso("3000000000000000000000000000000000000000000000001");
            var xml = BuildFacturaXml("1.1.0", clave, producto.Codigo, producto.Nombre);
            await xmlSriParserService.ProcesarArchivosXmlMasivosAsync([xml], empresaId, cancellationToken);

            var log = await dbContext.FacturaCompraXmlLogs
                .IgnoreQueryFilters()
                .FirstAsync(current => current.EmpresaId == empresaId && current.ClaveAcceso == clave, cancellationToken);

            var stockInicial = await dbContext.ProductosBodega
                .IgnoreQueryFilters()
                .Where(current => current.EmpresaId == empresaId && current.BodegaId == bodega.Id && current.ProductoId == producto.Id)
                .Select(current => (decimal?)current.StockActual)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            var compra = await xmlSriParserService.ConvertirXmlACompraAsync(
                new ConvertirXmlACompraRequest
                {
                    FacturaCompraXmlLogId = log.Id,
                    BodegaId = bodega.Id,
                    FormaPagoCompra = FormaPagoCompra.CreditoProveedores,
                    CrearProveedorSiNoExiste = true,
                    MapeosProductos =
                    [
                        new MapeoProductoProveedorDto
                        {
                            CodigoProductoProveedor = producto.Codigo,
                            NombreProductoProveedor = producto.Nombre,
                            ProductoId = producto.Id
                        }
                    ]
                },
                cancellationToken);

            var stockFinal = await dbContext.ProductosBodega
                .IgnoreQueryFilters()
                .Where(current => current.EmpresaId == empresaId && current.BodegaId == bodega.Id && current.ProductoId == producto.Id)
                .Select(current => (decimal?)current.StockActual)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            var cxpCreada = await dbContext.CuentasPorPagar
                .IgnoreQueryFilters()
                .AnyAsync(current => current.CompraId == compra.Id, cancellationToken);

            await transaction.RollbackAsync(cancellationToken);
            watch.Stop();

            var ok = stockFinal > stockInicial && cxpCreada;
            return Result(
                "ConversionACompraEInventarioTest",
                ok,
                ok ? "VALIDO" : "OBSERVADO",
                ok ? "La conversion XML incremento stock y preparo CXP dentro de una transaccion de prueba con rollback." : "La conversion no evidencio stock/CXP esperado.",
                watch.Elapsed,
                ok ? [] : ["No se pudo verificar incremento de stock y/o creacion de CXP."],
                new Dictionary<string, string>
                {
                    ["CompraIdRollback"] = compra.Id.ToString(),
                    ["Producto"] = producto.Codigo,
                    ["Bodega"] = bodega.Nombre,
                    ["StockInicial"] = stockInicial.ToString("0.####"),
                    ["StockFinalAntesRollback"] = stockFinal.ToString("0.####"),
                    ["CxpCreada"] = cxpCreada.ToString()
                });
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(exception, "Fallo prueba ConversionACompraEInventarioTest.");
            watch.Stop();
            return Failed("ConversionACompraEInventarioTest", exception.Message, watch.Elapsed);
        }
    }

    private async Task<Guid> ResolveEmpresaIdAsync(CancellationToken cancellationToken)
    {
        if (tenantContextAccessor.EmpresaId.HasValue)
        {
            return tenantContextAccessor.EmpresaId.Value;
        }

        return await dbContext.EmpresasEmisoras
            .IgnoreQueryFilters()
            .OrderBy(current => current.RazonSocial)
            .Select(current => current.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string BuildFacturaXml(string version, string claveAcceso, string codigo = "TEST-XML-001", string descripcion = "Producto XML prueba")
    {
        return $$"""
        <?xml version="1.0" encoding="UTF-8"?>
        <factura id="comprobante" version="{{version}}">
          <infoTributaria>
            <ambiente>1</ambiente>
            <tipoEmision>1</tipoEmision>
            <razonSocial>PROVEEDOR XML QA S.A.</razonSocial>
            <nombreComercial>PROVEEDOR XML QA</nombreComercial>
            <ruc>1790012345001</ruc>
            <claveAcceso>{{claveAcceso}}</claveAcceso>
            <codDoc>01</codDoc>
            <estab>001</estab>
            <ptoEmi>001</ptoEmi>
            <secuencial>000000001</secuencial>
            <dirMatriz>Quito</dirMatriz>
          </infoTributaria>
          <infoFactura>
            <fechaEmision>24/08/2026</fechaEmision>
            <dirEstablecimiento>Quito</dirEstablecimiento>
            <tipoIdentificacionComprador>04</tipoIdentificacionComprador>
            <razonSocialComprador>EMPRESA QA</razonSocialComprador>
            <identificacionComprador>0999999999001</identificacionComprador>
            <totalSinImpuestos>10.00</totalSinImpuestos>
            <totalDescuento>0.00</totalDescuento>
            <totalConImpuestos>
              <totalImpuesto>
                <codigo>2</codigo>
                <codigoPorcentaje>4</codigoPorcentaje>
                <baseImponible>10.00</baseImponible>
                <valor>1.50</valor>
              </totalImpuesto>
            </totalConImpuestos>
            <propina>0.00</propina>
            <importeTotal>11.50</importeTotal>
            <moneda>DOLAR</moneda>
          </infoFactura>
          <detalles>
            <detalle>
              <codigoPrincipal>{{codigo}}</codigoPrincipal>
              <descripcion>{{descripcion}}</descripcion>
              <cantidad>2.0000</cantidad>
              <precioUnitario>5.0000</precioUnitario>
              <descuento>0.00</descuento>
              <precioTotalSinImpuesto>10.00</precioTotalSinImpuesto>
              <impuestos>
                <impuesto>
                  <codigo>2</codigo>
                  <codigoPorcentaje>4</codigoPorcentaje>
                  <tarifa>15.00</tarifa>
                  <baseImponible>10.00</baseImponible>
                  <valor>1.50</valor>
                </impuesto>
              </impuestos>
            </detalle>
          </detalles>
        </factura>
        """;
    }

    private static string BuildClaveAcceso(string seed)
    {
        return new string(seed.Where(char.IsDigit).Take(49).ToArray()).PadRight(49, '0')[..49];
    }

    private static DiagnosticTestResultResponse Failed(string name, string message, TimeSpan duration)
    {
        return Result(name, false, "FALLIDA", message, duration, [message]);
    }

    private static DiagnosticTestResultResponse Result(
        string name,
        bool succeeded,
        string state,
        string message,
        TimeSpan duration,
        IReadOnlyCollection<string>? errors = null,
        IReadOnlyDictionary<string, string>? evidence = null)
    {
        return new DiagnosticTestResultResponse
        {
            NombrePrueba = name,
            Succeeded = succeeded,
            Estado = state,
            Mensaje = message,
            Duracion = duration,
            Errores = errors ?? Array.Empty<string>(),
            Evidencias = evidence ?? new Dictionary<string, string>()
        };
    }
}
