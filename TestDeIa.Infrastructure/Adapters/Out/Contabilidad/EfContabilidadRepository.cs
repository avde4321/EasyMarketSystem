using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Contabilidad.Entities;
using TestDeIa.Domain.Modules.Contabilidad.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Infrastructure.Adapters.Out.Contabilidad;

public sealed class EfContabilidadRepository(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserAccessor currentUserAccessor) : IContabilidadRepository
{
    private static readonly AccountTemplate[] AccountTemplates =
    [
        new("1", "Activo", 1, TipoCuentaContable.Activo, false),
        new("1.1", "Activo Corriente", 2, TipoCuentaContable.Activo, false),
        new("1.1.01", "Efectivo y Equivalentes", 3, TipoCuentaContable.Activo, false),
        new("1.1.01.01", "Caja General", 4, TipoCuentaContable.Activo, true),
        new("1.1.01.02", "Bancos", 4, TipoCuentaContable.Activo, true),
        new("1.1.02", "Cuentas por Cobrar Tributarias", 3, TipoCuentaContable.Activo, false),
        new("1.1.02.01", "Credito Tributario IVA Compras", 4, TipoCuentaContable.Activo, true),
        new("1.1.03", "Cuentas por Cobrar Comerciales", 3, TipoCuentaContable.Activo, false),
        new("1.1.03.01", "Cuentas por Cobrar Clientes", 4, TipoCuentaContable.Activo, true),
        new("1.1.04", "Inventarios", 3, TipoCuentaContable.Activo, false),
        new("1.1.04.01", "Inventario de Mercaderias", 4, TipoCuentaContable.Activo, true),
        new("2", "Pasivo", 1, TipoCuentaContable.Pasivo, false),
        new("2.1", "Pasivo Corriente", 2, TipoCuentaContable.Pasivo, false),
        new("2.1.01", "Cuentas por Pagar Comerciales", 3, TipoCuentaContable.Pasivo, false),
        new("2.1.01.01", "Cuentas por Pagar Proveedores", 4, TipoCuentaContable.Pasivo, true),
        new("2.1.03", "Impuestos por Pagar", 3, TipoCuentaContable.Pasivo, false),
        new("2.1.03.01", "IVA Ventas por Pagar", 4, TipoCuentaContable.Pasivo, true),
        new("4", "Ingresos", 1, TipoCuentaContable.Ingreso, false),
        new("4.1", "Ingresos Operacionales", 2, TipoCuentaContable.Ingreso, false),
        new("4.1.01", "Ventas", 3, TipoCuentaContable.Ingreso, false),
        new("4.1.01.01", "Ingreso por Ventas", 4, TipoCuentaContable.Ingreso, true),
        new("5", "Gastos", 1, TipoCuentaContable.Gasto, false),
        new("5.1", "Gastos Administrativos", 2, TipoCuentaContable.Gasto, false),
        new("5.1.01", "Gastos Operativos", 3, TipoCuentaContable.Gasto, false),
        new("5.1.01.01", "Gastos por Compras y Servicios", 4, TipoCuentaContable.Gasto, true),
        new("6", "Costos", 1, TipoCuentaContable.Costo, false),
        new("6.1", "Costo de Ventas", 2, TipoCuentaContable.Costo, false),
        new("6.1.01", "Costo de Ventas - Mercaderias", 3, TipoCuentaContable.Costo, true)
    ];

    public async Task<IReadOnlyCollection<CuentaContable>> GetPlanCuentasAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CuentasContables
            .AsNoTracking()
            .OrderBy(current => current.Codigo)
            .Select(current => new CuentaContable(
                current.Id,
                current.EmpresaId,
                current.Codigo,
                current.Nombre,
                current.Nivel,
                current.TipoCuenta,
                current.EsAceptable,
                current.SaldoActual))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CuentaContable>> GetCuentasAceptablesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CuentasContables
            .AsNoTracking()
            .Where(current => current.EsAceptable)
            .OrderBy(current => current.Codigo)
            .Select(current => new CuentaContable(
                current.Id,
                current.EmpresaId,
                current.Codigo,
                current.Nombre,
                current.Nivel,
                current.TipoCuenta,
                current.EsAceptable,
                current.SaldoActual))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<string> GenerarAsientoDesdeOrigenAsync(Guid transaccionId, string moduloOrigen, CancellationToken cancellationToken = default)
    {
        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            throw new InvalidOperationException("No existe una empresa activa para generar el asiento contable.");
        }

        return moduloOrigen.Trim().ToUpperInvariant() switch
        {
            "POS" => await GenerarAsientoDesdePosAsync(transaccionId, cancellationToken),
            "COMPRAS" => await GenerarAsientoDesdeCompraAsync(transaccionId, cancellationToken),
            _ => throw new InvalidOperationException($"El módulo de origen {moduloOrigen} no está soportado para contabilización automática.")
        };
    }

    public async Task<string> CrearAsientoAsync(CrearAsientoRequest request, CancellationToken cancellationToken = default)
    {
        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            throw new InvalidOperationException("No existe una empresa activa para registrar el asiento.");
        }

        var empresaId = tenantContextAccessor.EmpresaId.Value;
        var fechaContable = request.FechaContable.Date;
        var periodo = await dbContext.PeriodosContables
            .FirstOrDefaultAsync(current => current.Anio == fechaContable.Year && current.Mes == fechaContable.Month, cancellationToken);

        if (periodo?.EstaCerrado == true)
        {
            throw new InvalidOperationException($"El período {fechaContable:MM/yyyy} se encuentra cerrado y no admite cambios.");
        }

        var cuentaIds = request.Detalles
            .Select(current => current.CuentaContableId)
            .Distinct()
            .ToArray();

        var cuentas = await dbContext.CuentasContables
            .Where(current => cuentaIds.Contains(current.Id))
            .ToDictionaryAsync(current => current.Id, cancellationToken);

        if (cuentas.Count != cuentaIds.Length || cuentas.Values.Any(current => !current.EsAceptable))
        {
            throw new InvalidOperationException("Todas las cuentas del asiento deben existir y ser aceptables para movimiento directo.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var nextNumber = await dbContext.AsientosContables.CountAsync(cancellationToken) + 1;
        var numeroAsiento = nextNumber.ToString("D8");

        var modulo = Enum.TryParse<ModuloOrigenContable>(request.ModuloOrigen, true, out var parsedModulo)
            ? parsedModulo
            : ModuloOrigenContable.Diario;

        var estado = Enum.TryParse<EstadoAsientoContable>(request.Estado, true, out var parsedEstado)
            ? parsedEstado
            : EstadoAsientoContable.Posteado;

        var asiento = new AsientoContableEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            NumeroAsiento = numeroAsiento,
            FechaContable = fechaContable,
            Concepto = request.Concepto.Trim(),
            ModuloOrigen = modulo,
            DocumentoSoporte = string.IsNullOrWhiteSpace(request.DocumentoSoporte) ? null : request.DocumentoSoporte.Trim(),
            Estado = estado,
            Detalles = request.Detalles
                .Select(current => new AsientoDetalleEntity
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = current.CuentaContableId,
                    Debe = decimal.Round(current.Debe, 2, MidpointRounding.AwayFromZero),
                    Haber = decimal.Round(current.Haber, 2, MidpointRounding.AwayFromZero)
                })
                .ToList()
        };

        if (estado == EstadoAsientoContable.Posteado)
        {
            foreach (var detalle in asiento.Detalles)
            {
                var cuenta = cuentas[detalle.CuentaContableId];
                cuenta.SaldoActual = ApplySaldo(cuenta.TipoCuenta, cuenta.SaldoActual, detalle.Debe, detalle.Haber);
            }
        }

        dbContext.AsientosContables.Add(asiento);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return numeroAsiento;
    }

    public async Task<IReadOnlyCollection<AsientoContableResponse>> GetLibroDiarioAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.AsientosContables
            .AsNoTracking()
            .Include(current => current.Detalles)
            .ThenInclude(current => current.CuentaContable)
            .OrderByDescending(current => current.FechaContable)
            .ThenByDescending(current => current.NumeroAsiento)
            .Take(50)
            .Select(current => new AsientoContableResponse
            {
                Id = current.Id,
                NumeroAsiento = current.NumeroAsiento,
                FechaContable = current.FechaContable,
                Concepto = current.Concepto,
                ModuloOrigen = current.ModuloOrigen.ToString(),
                DocumentoSoporte = current.DocumentoSoporte,
                Estado = current.Estado.ToString(),
                TotalDebe = current.Detalles.Sum(detail => detail.Debe),
                TotalHaber = current.Detalles.Sum(detail => detail.Haber),
                Detalles = current.Detalles
                    .OrderBy(detail => detail.CuentaContable.Codigo)
                    .Select(detail => new AsientoDetalleResponse
                    {
                        Id = detail.Id,
                        CuentaContableId = detail.CuentaContableId,
                        CuentaCodigo = detail.CuentaContable.Codigo,
                        CuentaNombre = detail.CuentaContable.Nombre,
                        Debe = detail.Debe,
                        Haber = detail.Haber
                    })
                    .ToArray()
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LibroDiarioLineaResponse>> GetLibroDiarioAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var fechaDesde = desde.Date;
        var fechaHasta = hasta.Date;

        return await dbContext.AsientosContables
            .AsNoTracking()
            .Where(current =>
                current.Estado == EstadoAsientoContable.Posteado &&
                current.FechaContable >= fechaDesde &&
                current.FechaContable <= fechaHasta)
            .SelectMany(current => current.Detalles.Select(detail => new LibroDiarioLineaResponse
            {
                FechaContable = current.FechaContable,
                NumeroAsiento = current.NumeroAsiento,
                CuentaCodigo = detail.CuentaContable.Codigo,
                CuentaNombre = detail.CuentaContable.Nombre,
                Concepto = current.Concepto,
                DocumentoSoporte = current.DocumentoSoporte,
                Debe = detail.Debe,
                Haber = detail.Haber
            }))
            .OrderBy(current => current.FechaContable)
            .ThenBy(current => current.NumeroAsiento)
            .ThenBy(current => current.CuentaCodigo)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<LibroMayorResponse> GetLibroMayorAsync(
        Guid cuentaContableId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var fechaDesde = desde.Date;
        var fechaHasta = hasta.Date;
        var cuenta = await dbContext.CuentasContables
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == cuentaContableId, cancellationToken)
            ?? throw new InvalidOperationException("La cuenta contable seleccionada no existe para la empresa activa.");

        var saldosPrevios = await dbContext.AsientosDetalle
            .AsNoTracking()
            .Where(detail =>
                detail.CuentaContableId == cuentaContableId &&
                detail.AsientoContable.Estado == EstadoAsientoContable.Posteado &&
                detail.AsientoContable.FechaContable < fechaDesde)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Debe = group.Sum(current => current.Debe),
                Haber = group.Sum(current => current.Haber)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var saldoInicial = ApplySaldo(cuenta.TipoCuenta, 0m, saldosPrevios?.Debe ?? 0m, saldosPrevios?.Haber ?? 0m);

        var movimientosBase = await dbContext.AsientosDetalle
            .AsNoTracking()
            .Where(detail =>
                detail.CuentaContableId == cuentaContableId &&
                detail.AsientoContable.Estado == EstadoAsientoContable.Posteado &&
                detail.AsientoContable.FechaContable >= fechaDesde &&
                detail.AsientoContable.FechaContable <= fechaHasta)
            .OrderBy(detail => detail.AsientoContable.FechaContable)
            .ThenBy(detail => detail.AsientoContable.NumeroAsiento)
            .Select(detail => new
            {
                detail.AsientoContable.FechaContable,
                detail.AsientoContable.NumeroAsiento,
                detail.AsientoContable.Concepto,
                detail.AsientoContable.DocumentoSoporte,
                detail.Debe,
                detail.Haber
            })
            .ToArrayAsync(cancellationToken);

        var saldoAcumulado = saldoInicial;
        var movimientos = movimientosBase
            .Select(current =>
            {
                saldoAcumulado = ApplySaldo(cuenta.TipoCuenta, saldoAcumulado, current.Debe, current.Haber);
                return new LibroMayorMovimientoResponse
                {
                    FechaContable = current.FechaContable,
                    NumeroAsiento = current.NumeroAsiento,
                    Concepto = current.Concepto,
                    DocumentoSoporte = current.DocumentoSoporte,
                    Debe = current.Debe,
                    Haber = current.Haber,
                    SaldoAcumulado = saldoAcumulado
                };
            })
            .ToArray();

        return new LibroMayorResponse
        {
            CuentaContableId = cuenta.Id,
            CuentaCodigo = cuenta.Codigo,
            CuentaNombre = cuenta.Nombre,
            Desde = fechaDesde,
            Hasta = fechaHasta,
            SaldoInicial = saldoInicial,
            TotalDebe = movimientos.Sum(current => current.Debe),
            TotalHaber = movimientos.Sum(current => current.Haber),
            SaldoFinal = saldoAcumulado,
            Movimientos = movimientos
        };
    }

    public async Task<IReadOnlyCollection<CuentaContable>> GetCuentasParaEstadosFinancierosAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CuentasContables
            .AsNoTracking()
            .OrderBy(current => current.Codigo)
            .Select(current => new CuentaContable(
                current.Id,
                current.EmpresaId,
                current.Codigo,
                current.Nombre,
                current.Nivel,
                current.TipoCuenta,
                current.EsAceptable,
                current.SaldoActual))
            .ToArrayAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<PeriodoContableResponse>> GetPeriodosAsync(int anio, CancellationToken cancellationToken = default)
    {
        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            throw new InvalidOperationException("No existe una empresa activa para consultar periodos fiscales.");
        }

        var empresaId = tenantContextAccessor.EmpresaId.Value;
        var existing = await dbContext.PeriodosContables
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId && current.Anio == anio)
            .ToDictionaryAsync(current => current.Mes, cancellationToken);

        return Enumerable.Range(1, 12)
            .Select(mes =>
            {
                existing.TryGetValue(mes, out var periodo);
                return new PeriodoContableResponse
                {
                    Id = periodo?.Id ?? Guid.Empty,
                    Anio = anio,
                    Mes = mes,
                    EstaCerrado = periodo?.EstaCerrado ?? false,
                    FechaCierre = periodo?.FechaCierre,
                    UsuarioCierreId = periodo?.UsuarioCierreId
                };
            })
            .ToArray();
    }

    public async Task<PeriodoContableResponse> CerrarPeriodoFiscalAsync(
        CerrarPeriodoFiscalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            throw new InvalidOperationException("No existe una empresa activa para cerrar el periodo fiscal.");
        }

        var empresaId = tenantContextAccessor.EmpresaId.Value;
        var hasDrafts = await dbContext.AsientosContables
            .AsNoTracking()
            .AnyAsync(current =>
                current.EmpresaId == empresaId &&
                current.Estado == EstadoAsientoContable.Borrador &&
                current.FechaContable.Year == request.Anio &&
                current.FechaContable.Month == request.Mes,
                cancellationToken);

        if (hasDrafts)
        {
            throw new InvalidOperationException("No se puede cerrar el periodo fiscal porque existen asientos en borrador.");
        }

        var periodo = await dbContext.PeriodosContables
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.Anio == request.Anio &&
                current.Mes == request.Mes,
                cancellationToken);

        if (periodo?.EstaCerrado == true)
        {
            return MapPeriodo(periodo);
        }

        if (periodo is null)
        {
            periodo = new PeriodoContableEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                Anio = request.Anio,
                Mes = request.Mes,
                EstaCerrado = false
            };
            dbContext.PeriodosContables.Add(periodo);
        }

        periodo.EstaCerrado = true;
        periodo.FechaCierre = DateTimeOffset.UtcNow;
        periodo.UsuarioCierreId = currentUserAccessor.GetRequiredUserId();

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapPeriodo(periodo);
    }

    public async Task<AjusteInventarioContableResponse> AjustarInventarioContableAsync(
        bool generarAsiento,
        CancellationToken cancellationToken = default)
    {
        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            throw new InvalidOperationException("No existe una empresa activa para reconciliar inventario.");
        }

        var empresaId = tenantContextAccessor.EmpresaId.Value;
        var movimientos = await dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId)
            .Select(current => new
            {
                current.ProductoId,
                current.BodegaId,
                current.FechaMovimiento,
                current.SaldoValor
            })
            .ToArrayAsync(cancellationToken);

        var valorKardex = Math.Round(movimientos
            .GroupBy(current => new { current.ProductoId, current.BodegaId })
            .Select(group => group.OrderByDescending(current => current.FechaMovimiento).First().SaldoValor)
            .Sum(), 2, MidpointRounding.AwayFromZero);

        var cuentas = await EnsureInventoryAdjustmentAccountsAsync(empresaId, cancellationToken);
        var saldoCuenta = Math.Round(cuentas.InventarioMercaderias.SaldoActual, 2, MidpointRounding.AwayFromZero);
        var diferencia = Math.Round(saldoCuenta - valorKardex, 2, MidpointRounding.AwayFromZero);

        if (!generarAsiento || diferencia <= 0m)
        {
            return new AjusteInventarioContableResponse
            {
                ValorKardex = valorKardex,
                SaldoCuentaInventario = saldoCuenta,
                Diferencia = diferencia,
                AsientoGenerado = false
            };
        }

        var request = new CrearAsientoRequest
        {
            FechaContable = DateTime.Today,
            Concepto = $"Ajuste automatico por merma/deterioro de inventario | Kardex {valorKardex:N2} vs cuenta {saldoCuenta:N2}",
            ModuloOrigen = ModuloOrigenContable.Diario.ToString(),
            Estado = EstadoAsientoContable.Posteado.ToString(),
            Detalles =
            [
                CreateDetalle(cuentas.GastoMermasInventario.Id, diferencia, 0m),
                CreateDetalle(cuentas.InventarioMercaderias.Id, 0m, diferencia)
            ]
        };

        var numeroAsiento = await CrearAsientoAsync(request, cancellationToken);
        return new AjusteInventarioContableResponse
        {
            ValorKardex = valorKardex,
            SaldoCuentaInventario = saldoCuenta,
            Diferencia = diferencia,
            AsientoGenerado = true,
            NumeroAsiento = numeroAsiento
        };
    }
    private async Task<string> GenerarAsientoDesdePosAsync(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await dbContext.Facturas
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken)
            ?? throw new InvalidOperationException("La factura de origen no existe para generar el asiento contable.");

        var originMarker = BuildOriginMarker(facturaId);
        var existingNumber = await FindExistingNumeroAsientoAsync(ModuloOrigenContable.Pos, originMarker, cancellationToken);
        if (!string.IsNullOrWhiteSpace(existingNumber))
        {
            return existingNumber;
        }

        var cuentas = await EnsureOperationalAccountsAsync(cancellationToken);
        var productoIds = factura.Detalles.Select(current => current.ProductoId).Distinct().ToArray();
        var productos = await dbContext.Productos
            .AsNoTracking()
            .Where(current => productoIds.Contains(current.Id))
            .ToDictionaryAsync(current => current.Id, cancellationToken);

        var totalCostoVentas = Math.Round(
            factura.Detalles
                .Where(detail => productos.TryGetValue(detail.ProductoId, out var producto) && producto.ControlaStock)
                .Sum(detail => detail.Cantidad * productos[detail.ProductoId].CostoPromedio),
            2,
            MidpointRounding.AwayFromZero);

        var request = new CrearAsientoRequest
        {
            FechaContable = factura.FechaEmision.UtcDateTime,
            Concepto = $"Venta POS {factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000} | {originMarker}",
            ModuloOrigen = ModuloOrigenContable.Pos.ToString(),
            DocumentoSoporte = NormalizeDocumentoSoporte(factura.ClaveAcceso, $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}"),
            Estado = EstadoAsientoContable.Posteado.ToString(),
            Detalles = BuildPosDetalles(factura, cuentas, totalCostoVentas)
        };

        return await CrearAsientoAsync(request, cancellationToken);
    }

    private async Task<string> GenerarAsientoDesdeCompraAsync(Guid compraId, CancellationToken cancellationToken)
    {
        var compra = await dbContext.Compras
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == compraId, cancellationToken)
            ?? throw new InvalidOperationException("La compra de origen no existe para generar el asiento contable.");

        var originMarker = BuildOriginMarker(compraId);
        var existingNumber = await FindExistingNumeroAsientoAsync(ModuloOrigenContable.Compras, originMarker, cancellationToken);
        if (!string.IsNullOrWhiteSpace(existingNumber))
        {
            return existingNumber;
        }

        var cuentas = await EnsureOperationalAccountsAsync(cancellationToken);
        var productoIds = compra.Detalles.Select(current => current.ProductoId).Distinct().ToArray();
        var productos = await dbContext.Productos
            .AsNoTracking()
            .Where(current => productoIds.Contains(current.Id))
            .ToDictionaryAsync(current => current.Id, cancellationToken);

        var hasCuentaPorPagar = await dbContext.CuentasPorPagar
            .AsNoTracking()
            .AnyAsync(current => current.CompraId == compra.Id, cancellationToken);

        var totalInventario = Math.Round(
            compra.Detalles
                .Where(detail => productos.TryGetValue(detail.ProductoId, out var producto) && producto.ControlaStock)
                .Sum(detail => detail.CostoTotalSinImpuesto),
            2,
            MidpointRounding.AwayFromZero);

        var totalGasto = Math.Round(
            compra.Detalles
                .Where(detail => !productos.TryGetValue(detail.ProductoId, out var producto) || !producto.ControlaStock)
                .Sum(detail => detail.CostoTotalSinImpuesto),
            2,
            MidpointRounding.AwayFromZero);

        var detalles = new List<CrearAsientoDetalleRequest>();
        if (totalInventario > 0m)
        {
            detalles.Add(CreateDetalle(cuentas.InventarioMercaderias.Id, totalInventario, 0m));
        }

        if (totalGasto > 0m)
        {
            detalles.Add(CreateDetalle(cuentas.GastoComprasServicios.Id, totalGasto, 0m));
        }

        if (compra.TotalImpuestos > 0m)
        {
            detalles.Add(CreateDetalle(cuentas.CreditoTributarioIvaCompras.Id, compra.TotalImpuestos, 0m));
        }

        detalles.Add(CreateDetalle(
            hasCuentaPorPagar ? cuentas.CuentasPorPagarProveedores.Id : cuentas.Bancos.Id,
            0m,
            compra.ImporteTotal));

        var request = new CrearAsientoRequest
        {
            FechaContable = compra.FechaEmision.UtcDateTime,
            Concepto = $"Compra {compra.Establecimiento}-{compra.PuntoEmision}-{compra.Secuencial} | {originMarker}",
            ModuloOrigen = ModuloOrigenContable.Compras.ToString(),
            DocumentoSoporte = NormalizeDocumentoSoporte(compra.ClaveAccesoProveedor, compra.ClaveAccesoGenerada, $"{compra.Establecimiento}-{compra.PuntoEmision}-{compra.Secuencial}"),
            Estado = EstadoAsientoContable.Posteado.ToString(),
            Detalles = detalles
        };

        return await CrearAsientoAsync(request, cancellationToken);
    }

    private List<CrearAsientoDetalleRequest> BuildPosDetalles(FacturaEntity factura, OperationalAccounts cuentas, decimal totalCostoVentas)
    {
        var detalles = new List<CrearAsientoDetalleRequest>
        {
            CreateDetalle(
                IsCreditPayment(factura.FormaPagoSriCodigo, factura.FormaPago) ? cuentas.CuentasPorCobrarClientes.Id : cuentas.CajaGeneral.Id,
                factura.Total,
                0m),
            CreateDetalle(cuentas.IngresoPorVentas.Id, 0m, factura.Subtotal),
            CreateDetalle(cuentas.IvaVentasPorPagar.Id, 0m, factura.IvaTotal)
        };

        if (totalCostoVentas > 0m)
        {
            detalles.Add(CreateDetalle(cuentas.CostoVentasMercaderias.Id, totalCostoVentas, 0m));
            detalles.Add(CreateDetalle(cuentas.InventarioMercaderias.Id, 0m, totalCostoVentas));
        }

        return detalles
            .Where(detail => detail.Debe > 0m || detail.Haber > 0m)
            .ToList();
    }

    private async Task<OperationalAccounts> EnsureOperationalAccountsAsync(CancellationToken cancellationToken)
    {
        var requiredCodes = AccountTemplates.Select(current => current.Codigo).ToArray();
        var cuentas = await dbContext.CuentasContables
            .Where(current => requiredCodes.Contains(current.Codigo))
            .ToDictionaryAsync(current => current.Codigo, cancellationToken);

        var missingTemplates = AccountTemplates
            .Where(current => !cuentas.ContainsKey(current.Codigo))
            .OrderBy(current => current.Nivel)
            .ToArray();

        if (missingTemplates.Length > 0)
        {
            var empresaId = tenantContextAccessor.EmpresaId!.Value;
            foreach (var template in missingTemplates)
            {
                var entity = new CuentaContableEntity
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    Codigo = template.Codigo,
                    Nombre = template.Nombre,
                    Nivel = template.Nivel,
                    TipoCuenta = template.TipoCuenta,
                    EsAceptable = template.EsAceptable,
                    SaldoActual = 0m
                };

                dbContext.CuentasContables.Add(entity);
                cuentas[template.Codigo] = entity;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new OperationalAccounts(
            cuentas["1.1.01.01"],
            cuentas["1.1.01.02"],
            cuentas["1.1.02.01"],
            cuentas["1.1.03.01"],
            cuentas["1.1.04.01"],
            cuentas["2.1.01.01"],
            cuentas["2.1.03.01"],
            cuentas["4.1.01.01"],
            cuentas["5.1.01.01"],
            cuentas["6.1.01"]);
    }

    private async Task<InventoryAdjustmentAccounts> EnsureInventoryAdjustmentAccountsAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var templates = new[]
        {
            new AccountTemplate("1.1.04.01", "Inventario de Mercaderias", 4, TipoCuentaContable.Activo, true),
            new AccountTemplate("5.1.02", "Gastos por Inventario", 3, TipoCuentaContable.Gasto, false),
            new AccountTemplate("5.1.02.01", "Gasto por Mermas/Deterioro de Inventario", 4, TipoCuentaContable.Gasto, true)
        };

        var codes = templates.Select(current => current.Codigo).ToArray();
        var cuentas = await dbContext.CuentasContables
            .Where(current => current.EmpresaId == empresaId && codes.Contains(current.Codigo))
            .ToDictionaryAsync(current => current.Codigo, cancellationToken);

        foreach (var template in templates.Where(current => !cuentas.ContainsKey(current.Codigo)).OrderBy(current => current.Nivel))
        {
            var entity = new CuentaContableEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                Codigo = template.Codigo,
                Nombre = template.Nombre,
                Nivel = template.Nivel,
                TipoCuenta = template.TipoCuenta,
                EsAceptable = template.EsAceptable,
                SaldoActual = 0m
            };

            dbContext.CuentasContables.Add(entity);
            cuentas[template.Codigo] = entity;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new InventoryAdjustmentAccounts(
            cuentas["1.1.04.01"],
            cuentas["5.1.02.01"]);
    }

    private async Task<string?> FindExistingNumeroAsientoAsync(ModuloOrigenContable moduloOrigen, string originMarker, CancellationToken cancellationToken)
    {
        return await dbContext.AsientosContables
            .AsNoTracking()
            .Where(current => current.ModuloOrigen == moduloOrigen && current.Concepto.Contains(originMarker))
            .OrderByDescending(current => current.FechaContable)
            .Select(current => current.NumeroAsiento)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static CrearAsientoDetalleRequest CreateDetalle(Guid cuentaContableId, decimal debe, decimal haber)
    {
        return new CrearAsientoDetalleRequest
        {
            CuentaContableId = cuentaContableId,
            Debe = Math.Round(debe, 2, MidpointRounding.AwayFromZero),
            Haber = Math.Round(haber, 2, MidpointRounding.AwayFromZero)
        };
    }

    private static PeriodoContableResponse MapPeriodo(PeriodoContableEntity periodo)
    {
        return new PeriodoContableResponse
        {
            Id = periodo.Id,
            Anio = periodo.Anio,
            Mes = periodo.Mes,
            EstaCerrado = periodo.EstaCerrado,
            FechaCierre = periodo.FechaCierre,
            UsuarioCierreId = periodo.UsuarioCierreId
        };
    }

    private static string BuildOriginMarker(Guid transaccionId) => $"ORIGEN:{transaccionId:N}";

    private static string? NormalizeDocumentoSoporte(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static bool IsCreditPayment(string formaPagoSriCodigo, string? formaPagoNombre)
    {
        var normalizedName = formaPagoNombre?.Trim() ?? string.Empty;
        return formaPagoSriCodigo.Equals("19", StringComparison.OrdinalIgnoreCase) ||
               normalizedName.Contains("credito", StringComparison.OrdinalIgnoreCase) ||
               normalizedName.Contains("plazo", StringComparison.OrdinalIgnoreCase) ||
               normalizedName.Contains("cuenta por cobrar", StringComparison.OrdinalIgnoreCase);
    }

    private static decimal ApplySaldo(TipoCuentaContable tipoCuenta, decimal saldoActual, decimal debe, decimal haber)
    {
        return tipoCuenta switch
        {
            TipoCuentaContable.Activo or TipoCuentaContable.Gasto or TipoCuentaContable.Costo => saldoActual + debe - haber,
            TipoCuentaContable.Pasivo or TipoCuentaContable.Patrimonio or TipoCuentaContable.Ingreso => saldoActual - debe + haber,
            _ => saldoActual + debe - haber
        };
    }

    private sealed record AccountTemplate(string Codigo, string Nombre, int Nivel, TipoCuentaContable TipoCuenta, bool EsAceptable);

    private sealed record OperationalAccounts(
        CuentaContableEntity CajaGeneral,
        CuentaContableEntity Bancos,
        CuentaContableEntity CreditoTributarioIvaCompras,
        CuentaContableEntity CuentasPorCobrarClientes,
        CuentaContableEntity InventarioMercaderias,
        CuentaContableEntity CuentasPorPagarProveedores,
        CuentaContableEntity IvaVentasPorPagar,
        CuentaContableEntity IngresoPorVentas,
        CuentaContableEntity GastoComprasServicios,
        CuentaContableEntity CostoVentasMercaderias);

    private sealed record InventoryAdjustmentAccounts(
        CuentaContableEntity InventarioMercaderias,
        CuentaContableEntity GastoMermasInventario);
}




