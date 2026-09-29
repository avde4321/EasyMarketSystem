using Microsoft.EntityFrameworkCore;
using System.Data;
using TestDeIa.Application.Modules.Caja.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Caja.Entities;
using TestDeIa.Domain.Modules.Contabilidad.Enums;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Caja;

public sealed class EfCajaSesionRepository(
    TestDeIaDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor) : ICajaSesionRepository
{
    public async Task<CajaSesion?> GetActivaAsync(CancellationToken cancellationToken = default)
    {
        var empresaId = currentUserAccessor.GetRequiredEmpresaId();
        var usuarioId = currentUserAccessor.GetRequiredUserId();

        var caja = await dbContext.Set<CajaSesionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.UsuarioId == usuarioId &&
                (current.EstadoCaja == CajaEstado.Abierta || current.EstadoCaja == CajaEstado.EnArqueo),
                cancellationToken);

        if (caja is null)
        {
            return null;
        }

        var (efectivo, tarjeta, transferencia, otros) = await CalcularTotalesVentasAsync(caja.Id, cancellationToken);
        var movimientos = await CalcularMovimientosCajaAsync(caja.Id, cancellationToken);
        caja.TotalVentasEfectivoCalculado = efectivo;
        caja.TotalVentasTarjetaCalculado = tarjeta;
        caja.TotalVentasTransferenciaCalculado = transferencia;
        caja.MontoCalculadoEfectivo = Math.Round(caja.MontoApertura + efectivo + movimientos.Efectivo, 2);
        caja.MontoCalculadoTarjetas = tarjeta;
        caja.MontoCalculadoTransferencias = transferencia;
        caja.MontoCalculadoOtros = otros;
        caja.MontoCalculadoTotal = Math.Round(caja.MontoCalculadoEfectivo + tarjeta + transferencia + otros, 2);

        return Map(caja);
    }

    public async Task<bool> HasActiveSessionAsync(CancellationToken cancellationToken = default)
    {
        var empresaId = currentUserAccessor.GetRequiredEmpresaId();
        var usuarioId = currentUserAccessor.GetRequiredUserId();

        return await dbContext.Set<CajaSesionEntity>()
            .AsNoTracking()
            .AnyAsync(current =>
                current.EmpresaId == empresaId &&
                current.UsuarioId == usuarioId &&
                (current.EstadoCaja == CajaEstado.Abierta || current.EstadoCaja == CajaEstado.EnArqueo),
                cancellationToken);
    }

    public async Task<CajaSesion> AbrirAsync(decimal montoApertura, CancellationToken cancellationToken = default)
    {
        if (await HasActiveSessionAsync(cancellationToken))
        {
            throw new InvalidOperationException("Ya tienes una caja abierta para la empresa activa.");
        }

        var now = DateTimeOffset.UtcNow;
        var userId = currentUserAccessor.GetRequiredUserId();
        var empresaId = currentUserAccessor.GetRequiredEmpresaId();
        var puntoOperativo = await ResolvePuntoEmisionOperativoAsync(empresaId, userId, cancellationToken);
        var entity = new CajaSesionEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            UsuarioId = userId,
            PuntoEmisionId = puntoOperativo?.Id,
            BodegaId = puntoOperativo?.BodegaId,
            FechaApertura = now,
            MontoApertura = Math.Round(montoApertura, 2),
            TotalVentasEfectivoCalculado = 0,
            TotalVentasTarjetaCalculado = 0,
            TotalVentasTransferenciaCalculado = 0,
            MontoFisicoEfectivoReal = 0,
            MontoFisicoTarjetaReal = 0,
            MontoFisicoTransferenciaReal = 0,
            DiferenciaEfectivo = 0,
            DiferenciaTarjeta = 0,
            DiferenciaTransferencia = 0,
            Diferencia = 0,
            EstadoCaja = CajaEstado.Abierta,
            CreatedAt = now,
            UsuarioCreacionId = userId
        };

        dbContext.Set<CajaSesionEntity>().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<CajaSesion> CerrarAsync(
        decimal montoFisicoEfectivoReal,
        decimal montoFisicoTarjetaReal,
        decimal montoFisicoTransferenciaReal,
        decimal montoFisicoOtrosReal,
        string? observacionesCierre,
        CancellationToken cancellationToken = default)
    {
        var empresaId = currentUserAccessor.GetRequiredEmpresaId();
        var usuarioId = currentUserAccessor.GetRequiredUserId();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var entity = await dbContext.Set<CajaSesionEntity>()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.UsuarioId == usuarioId &&
                (current.EstadoCaja == CajaEstado.Abierta || current.EstadoCaja == CajaEstado.EnArqueo),
                cancellationToken)
            ?? throw new InvalidOperationException("No tienes una caja abierta para cerrar.");

        var (efectivo, tarjeta, transferencia, otros) = await CalcularTotalesVentasAsync(entity.Id, cancellationToken);
        var movimientos = await CalcularMovimientosCajaAsync(entity.Id, cancellationToken);
        var efectivoReal = Math.Round(montoFisicoEfectivoReal, 2);
        var tarjetaReal = Math.Round(montoFisicoTarjetaReal, 2);
        var transferenciaReal = Math.Round(montoFisicoTransferenciaReal, 2);
        var otrosReal = Math.Round(montoFisicoOtrosReal, 2);
        var efectivoCalculado = Math.Round(entity.MontoApertura + efectivo + movimientos.Efectivo, 2);
        var tarjetaCalculada = Math.Round(tarjeta, 2);
        var transferenciaCalculada = Math.Round(transferencia, 2);
        var otrosCalculado = Math.Round(otros, 2);
        var totalCalculado = Math.Round(efectivoCalculado + tarjetaCalculada + transferenciaCalculada + otrosCalculado, 2);
        var totalDeclarado = Math.Round(efectivoReal + tarjetaReal + transferenciaReal + otrosReal, 2);
        var diferenciaEfectivo = Math.Round(efectivoReal - efectivoCalculado, 2);
        var diferenciaTarjeta = Math.Round(tarjetaReal - tarjeta, 2);
        var diferenciaTransferencia = Math.Round(transferenciaReal - transferencia, 2);
        var diferenciaTotal = Math.Round(totalDeclarado - totalCalculado, 2);
        var fechaServidor = DateTime.Today;
        var updatedAt = new DateTimeOffset(fechaServidor);
        var cuentas = await EnsureCajaAccountsAsync(empresaId, cancellationToken);
        var asiento = await CrearAsientoCierreCajaAsync(
            entity,
            cuentas,
            efectivoReal,
            tarjetaReal,
            transferenciaReal,
            diferenciaTotal,
            fechaServidor,
            cancellationToken);

        entity.TotalVentasEfectivoCalculado = efectivo;
        entity.TotalVentasTarjetaCalculado = tarjeta;
        entity.TotalVentasTransferenciaCalculado = transferencia;
        entity.MontoFisicoEfectivoReal = efectivoReal;
        entity.MontoFisicoTarjetaReal = tarjetaReal;
        entity.MontoFisicoTransferenciaReal = transferenciaReal;
        entity.DiferenciaEfectivo = diferenciaEfectivo;
        entity.DiferenciaTarjeta = diferenciaTarjeta;
        entity.DiferenciaTransferencia = diferenciaTransferencia;
        entity.Diferencia = diferenciaTotal;
        entity.MontoDeclaradoEfectivo = efectivoReal;
        entity.MontoDeclaradoTarjetas = tarjetaReal;
        entity.MontoDeclaradoTransferencias = transferenciaReal;
        entity.MontoDeclaradoOtros = otrosReal;
        entity.MontoDeclaradoTotal = totalDeclarado;
        entity.MontoCalculadoEfectivo = efectivoCalculado;
        entity.MontoCalculadoTarjetas = tarjetaCalculada;
        entity.MontoCalculadoTransferencias = transferenciaCalculada;
        entity.MontoCalculadoOtros = otrosCalculado;
        entity.MontoCalculadoTotal = totalCalculado;
        entity.DiferenciaMonto = diferenciaTotal;
        entity.ObservacionesCierre = NormalizeOptional(observacionesCierre);
        entity.FechaCierre = updatedAt;
        entity.EstadoCaja = CajaEstado.Cerrada;
        entity.AsientoContableId = asiento.Id;
        entity.UpdatedAt = updatedAt;
        entity.UsuarioModificacionId = usuarioId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<CajaSesion> RegistrarMovimientoCajaAsync(
        string tipoMovimiento,
        decimal monto,
        string concepto,
        string? comprobanteReferencia,
        CancellationToken cancellationToken = default)
    {
        var normalizedTipo = tipoMovimiento.Trim().ToUpperInvariant();
        if (!TipoMovimientoCaja.IsValid(normalizedTipo))
        {
            throw new InvalidOperationException("El tipo de movimiento de caja no es valido.");
        }

        var empresaId = currentUserAccessor.GetRequiredEmpresaId();
        var usuarioId = currentUserAccessor.GetRequiredUserId();
        var caja = await dbContext.Set<CajaSesionEntity>()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.UsuarioId == usuarioId &&
                current.EstadoCaja == CajaEstado.Abierta,
                cancellationToken)
            ?? throw new InvalidOperationException("Debes tener una caja abierta para registrar movimientos.");

        dbContext.Set<CajaMovimientoEntity>().Add(new CajaMovimientoEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            CajaSesionId = caja.Id,
            TipoMovimiento = normalizedTipo,
            Monto = Math.Round(monto, 2),
            Concepto = concepto.Trim(),
            ComprobanteReferencia = NormalizeOptional(comprobanteReferencia),
            CreadoPorUsuarioId = usuarioId,
            FechaMovimiento = DateTimeOffset.UtcNow
        });

        caja.UpdatedAt = DateTimeOffset.UtcNow;
        caja.UsuarioModificacionId = usuarioId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ReloadWithTotalsAsync(caja.Id, cancellationToken);
    }

    public async Task<CajaSesion> IniciarArqueoCiegoAsync(
        decimal montoDeclaradoEfectivo,
        decimal montoDeclaradoTarjetas,
        decimal montoDeclaradoTransferencias,
        decimal montoDeclaradoOtros,
        string? observacionesCierre,
        CancellationToken cancellationToken = default)
    {
        var empresaId = currentUserAccessor.GetRequiredEmpresaId();
        var usuarioId = currentUserAccessor.GetRequiredUserId();
        var caja = await dbContext.Set<CajaSesionEntity>()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.UsuarioId == usuarioId &&
                current.EstadoCaja == CajaEstado.Abierta,
                cancellationToken)
            ?? throw new InvalidOperationException("Debes tener una caja abierta para iniciar el arqueo.");

        caja.EstadoCaja = CajaEstado.EnArqueo;
        caja.MontoDeclaradoEfectivo = Math.Round(montoDeclaradoEfectivo, 2);
        caja.MontoDeclaradoTarjetas = Math.Round(montoDeclaradoTarjetas, 2);
        caja.MontoDeclaradoTransferencias = Math.Round(montoDeclaradoTransferencias, 2);
        caja.MontoDeclaradoOtros = Math.Round(montoDeclaradoOtros, 2);
        caja.MontoDeclaradoTotal = Math.Round(
            caja.MontoDeclaradoEfectivo.Value +
            caja.MontoDeclaradoTarjetas.Value +
            caja.MontoDeclaradoTransferencias.Value +
            caja.MontoDeclaradoOtros.Value,
            2);
        caja.ObservacionesCierre = NormalizeOptional(observacionesCierre);
        caja.UpdatedAt = DateTimeOffset.UtcNow;
        caja.UsuarioModificacionId = usuarioId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ReloadWithTotalsAsync(caja.Id, cancellationToken);
    }

    private async Task<CajaSesion> ReloadWithTotalsAsync(Guid cajaSesionId, CancellationToken cancellationToken)
    {
        var caja = await dbContext.Set<CajaSesionEntity>()
            .AsNoTracking()
            .FirstAsync(current => current.Id == cajaSesionId, cancellationToken);
        var (efectivo, tarjeta, transferencia, otros) = await CalcularTotalesVentasAsync(caja.Id, cancellationToken);
        var movimientos = await CalcularMovimientosCajaAsync(caja.Id, cancellationToken);
        caja.TotalVentasEfectivoCalculado = efectivo;
        caja.TotalVentasTarjetaCalculado = tarjeta;
        caja.TotalVentasTransferenciaCalculado = transferencia;
        caja.MontoCalculadoEfectivo = Math.Round(caja.MontoApertura + efectivo + movimientos.Efectivo, 2);
        caja.MontoCalculadoTarjetas = tarjeta;
        caja.MontoCalculadoTransferencias = transferencia;
        caja.MontoCalculadoOtros = otros;
        caja.MontoCalculadoTotal = Math.Round(caja.MontoCalculadoEfectivo + tarjeta + transferencia + otros, 2);
        return Map(caja);
    }

    private async Task<(decimal Efectivo, decimal Tarjeta, decimal Transferencia, decimal Otros)> CalcularTotalesVentasAsync(Guid cajaSesionId, CancellationToken cancellationToken)
    {
        var pagos = await dbContext.Set<FacturaPagoEntity>()
            .AsNoTracking()
            .Where(current => current.CajaSesionId == cajaSesionId)
            .Select(current => new
            {
                FormaPagoSriCodigo = current.FormaPagoCodigo,
                Total = current.Monto
            })
            .ToListAsync(cancellationToken);

        if (pagos.Count == 0)
        {
            pagos = await dbContext.Set<FacturaEntity>()
                .AsNoTracking()
                .Where(current =>
                    current.CajaSesionId == cajaSesionId &&
                    current.Estado != FacturaEstado.RECHAZADO)
                .Select(current => new
                {
                    current.FormaPagoSriCodigo,
                    current.Total
                })
                .ToListAsync(cancellationToken);
        }

        var efectivo = pagos
            .Where(current => string.Equals(current.FormaPagoSriCodigo, SriCatalogCodes.FormaPagoEfectivo, StringComparison.Ordinal))
            .Sum(current => current.Total);

        var tarjeta = pagos
            .Where(current => IsCardPayment(current.FormaPagoSriCodigo))
            .Sum(current => current.Total);

        var transferencia = pagos
            .Where(current => string.Equals(current.FormaPagoSriCodigo, SriCatalogCodes.FormaPagoTransferencia, StringComparison.Ordinal))
            .Sum(current => current.Total);

        var otros = pagos
            .Where(current =>
                !string.Equals(current.FormaPagoSriCodigo, SriCatalogCodes.FormaPagoEfectivo, StringComparison.Ordinal) &&
                !string.Equals(current.FormaPagoSriCodigo, SriCatalogCodes.FormaPagoTransferencia, StringComparison.Ordinal) &&
                !IsCardPayment(current.FormaPagoSriCodigo))
            .Sum(current => current.Total);

        return (Math.Round(efectivo, 2), Math.Round(tarjeta, 2), Math.Round(transferencia, 2), Math.Round(otros, 2));
    }

    private async Task<(decimal Efectivo, decimal Otros)> CalcularMovimientosCajaAsync(Guid cajaSesionId, CancellationToken cancellationToken)
    {
        var movimientos = await dbContext.Set<CajaMovimientoEntity>()
            .AsNoTracking()
            .Where(current => current.CajaSesionId == cajaSesionId)
            .Select(current => new { current.TipoMovimiento, current.Monto })
            .ToListAsync(cancellationToken);

        var ingresos = movimientos
            .Where(current => current.TipoMovimiento == TipoMovimientoCaja.IngresoManual)
            .Sum(current => current.Monto);
        var egresos = movimientos
            .Where(current => current.TipoMovimiento is TipoMovimientoCaja.EgresoGasto or TipoMovimientoCaja.RetiroSeguridad)
            .Sum(current => current.Monto);

        return (Math.Round(ingresos - egresos, 2), 0m);
    }

    private async Task<CajaAccounts> EnsureCajaAccountsAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        var templates = new[]
        {
            new AccountTemplate("1.1.01.01", "Caja General", 4, TipoCuentaContable.Activo, true),
            new AccountTemplate("1.1.01.02", "Bancos", 4, TipoCuentaContable.Activo, true),
            new AccountTemplate("4.1.01.01", "Ingreso por Ventas", 4, TipoCuentaContable.Ingreso, true),
            new AccountTemplate("4.1.02", "Otros Ingresos", 3, TipoCuentaContable.Ingreso, false),
            new AccountTemplate("4.1.02.01", "Otros Ingresos / Sobrantes", 4, TipoCuentaContable.Ingreso, true),
            new AccountTemplate("5.1.01.02", "Gasto por Faltante de Caja", 4, TipoCuentaContable.Gasto, true)
        };

        var codes = templates.Select(current => current.Codigo).ToArray();
        var cuentas = await dbContext.CuentasContables
            .Where(current => current.EmpresaId == empresaId && codes.Contains(current.Codigo))
            .ToDictionaryAsync(current => current.Codigo, cancellationToken);

        foreach (var template in templates.Where(current => !cuentas.ContainsKey(current.Codigo)).OrderBy(current => current.Nivel))
        {
            var cuenta = new CuentaContableEntity
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

            dbContext.CuentasContables.Add(cuenta);
            cuentas[template.Codigo] = cuenta;
        }

        return new CajaAccounts(
            cuentas["1.1.01.01"],
            cuentas["1.1.01.02"],
            cuentas["4.1.01.01"],
            cuentas["4.1.02.01"],
            cuentas["5.1.01.02"]);
    }

    private async Task<AsientoContableEntity> CrearAsientoCierreCajaAsync(
        CajaSesionEntity caja,
        CajaAccounts cuentas,
        decimal efectivoReal,
        decimal tarjetaReal,
        decimal transferenciaReal,
        decimal diferenciaTotal,
        DateTime fechaServidor,
        CancellationToken cancellationToken)
    {
        var detalles = new List<AsientoDetalleEntity>();
        AddDetalle(detalles, cuentas.CajaGeneral, efectivoReal, 0m);
        AddDetalle(detalles, cuentas.Bancos, tarjetaReal + transferenciaReal, 0m);

        if (diferenciaTotal < 0m)
        {
            AddDetalle(detalles, cuentas.GastoFaltanteCaja, Math.Abs(diferenciaTotal), 0m);
        }

        if (caja.MontoApertura > 0m)
        {
            AddDetalle(detalles, cuentas.CajaGeneral, 0m, caja.MontoApertura);
        }

        var totalVentas = Math.Round(
            caja.TotalVentasEfectivoCalculado +
            caja.TotalVentasTarjetaCalculado +
            caja.TotalVentasTransferenciaCalculado +
            caja.MontoCalculadoOtros,
            2);
        AddDetalle(detalles, cuentas.IngresoPorVentas, 0m, totalVentas);

        if (diferenciaTotal > 0m)
        {
            AddDetalle(detalles, cuentas.OtrosIngresosSobrantes, 0m, diferenciaTotal);
        }

        var totalDebe = detalles.Sum(current => current.Debe);
        var totalHaber = detalles.Sum(current => current.Haber);
        var ajuste = Math.Round(totalDebe - totalHaber, 2);
        if (ajuste > 0m)
        {
            AddDetalle(detalles, cuentas.OtrosIngresosSobrantes, 0m, ajuste);
        }
        else if (ajuste < 0m)
        {
            AddDetalle(detalles, cuentas.GastoFaltanteCaja, Math.Abs(ajuste), 0m);
        }

        var nextNumber = await dbContext.AsientosContables.CountAsync(cancellationToken) + 1;
        var asiento = new AsientoContableEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = caja.EmpresaId,
            NumeroAsiento = nextNumber.ToString("D8"),
            FechaContable = fechaServidor,
            Concepto = $"POS / Cierre de Caja | CAJA:{caja.Id:N}",
            ModuloOrigen = ModuloOrigenContable.PosCierreCaja,
            DocumentoSoporte = null,
            Estado = EstadoAsientoContable.Posteado,
            Detalles = detalles.Where(current => current.Debe > 0m || current.Haber > 0m).ToList()
        };

        foreach (var detalle in asiento.Detalles)
        {
            var cuenta = detalle.CuentaContable;
            cuenta.SaldoActual = ApplySaldo(cuenta.TipoCuenta, cuenta.SaldoActual, detalle.Debe, detalle.Haber);
            detalle.CuentaContable = null!;
        }

        dbContext.AsientosContables.Add(asiento);
        return asiento;
    }

    private static void AddDetalle(List<AsientoDetalleEntity> detalles, CuentaContableEntity cuenta, decimal debe, decimal haber)
    {
        detalles.Add(new AsientoDetalleEntity
        {
            Id = Guid.NewGuid(),
            CuentaContableId = cuenta.Id,
            CuentaContable = cuenta,
            Debe = Math.Round(debe, 2),
            Haber = Math.Round(haber, 2)
        });
    }

    private static bool IsCardPayment(string formaPagoSriCodigo) =>
        formaPagoSriCodigo is "16" or "18" or "19";

    private static decimal ApplySaldo(TipoCuentaContable tipoCuenta, decimal saldoActual, decimal debe, decimal haber)
    {
        return tipoCuenta switch
        {
            TipoCuentaContable.Activo or TipoCuentaContable.Gasto or TipoCuentaContable.Costo => saldoActual + debe - haber,
            TipoCuentaContable.Pasivo or TipoCuentaContable.Patrimonio or TipoCuentaContable.Ingreso => saldoActual - debe + haber,
            _ => saldoActual + debe - haber
        };
    }

    private async Task<EmpresaPuntoEmisionEntity?> ResolvePuntoEmisionOperativoAsync(
        Guid empresaId,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var puntoPermitidoId = await dbContext.Set<SecurityUserPuntoEmisionEntity>()
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId && current.SecurityUserId == usuarioId)
            .OrderBy(current => current.CreatedAt)
            .Select(current => current.EmpresaPuntoEmisionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (puntoPermitidoId != Guid.Empty)
        {
            return await dbContext.EmpresaPuntosEmision
                .AsNoTracking()
                .FirstOrDefaultAsync(current => current.Id == puntoPermitidoId, cancellationToken);
        }

        return await dbContext.EmpresaPuntosEmision
            .AsNoTracking()
            .Where(current => current.EmpresaEmisoraId == empresaId)
            .OrderByDescending(current => current.IsDefault)
            .ThenBy(current => current.Establecimiento)
            .ThenBy(current => current.PuntoEmision)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static CajaSesion Map(CajaSesionEntity entity)
    {
        return new CajaSesion
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            UsuarioId = entity.UsuarioId,
            PuntoEmisionId = entity.PuntoEmisionId,
            BodegaId = entity.BodegaId,
            FechaApertura = entity.FechaApertura,
            FechaCierre = entity.FechaCierre,
            MontoApertura = entity.MontoApertura,
            TotalVentasEfectivoCalculado = entity.TotalVentasEfectivoCalculado,
            TotalVentasTarjetaCalculado = entity.TotalVentasTarjetaCalculado,
            TotalVentasTransferenciaCalculado = entity.TotalVentasTransferenciaCalculado,
            MontoFisicoEfectivoReal = entity.MontoFisicoEfectivoReal,
            MontoFisicoTarjetaReal = entity.MontoFisicoTarjetaReal,
            MontoFisicoTransferenciaReal = entity.MontoFisicoTransferenciaReal,
            DiferenciaEfectivo = entity.DiferenciaEfectivo,
            DiferenciaTarjeta = entity.DiferenciaTarjeta,
            DiferenciaTransferencia = entity.DiferenciaTransferencia,
            Diferencia = entity.Diferencia,
            MontoDeclaradoEfectivo = entity.MontoDeclaradoEfectivo,
            MontoDeclaradoTarjetas = entity.MontoDeclaradoTarjetas,
            MontoDeclaradoTransferencias = entity.MontoDeclaradoTransferencias,
            MontoDeclaradoOtros = entity.MontoDeclaradoOtros,
            MontoDeclaradoTotal = entity.MontoDeclaradoTotal,
            MontoCalculadoEfectivo = entity.MontoCalculadoEfectivo,
            MontoCalculadoTarjetas = entity.MontoCalculadoTarjetas,
            MontoCalculadoTransferencias = entity.MontoCalculadoTransferencias,
            MontoCalculadoOtros = entity.MontoCalculadoOtros,
            MontoCalculadoTotal = entity.MontoCalculadoTotal,
            DiferenciaMonto = entity.DiferenciaMonto,
            ObservacionesCierre = entity.ObservacionesCierre,
            EstadoCaja = entity.EstadoCaja,
            AsientoContableId = entity.AsientoContableId,
            CreatedAt = entity.CreatedAt,
            UsuarioCreacionId = entity.UsuarioCreacionId,
            UpdatedAt = entity.UpdatedAt,
            UsuarioModificacionId = entity.UsuarioModificacionId
        };
    }

    private sealed record AccountTemplate(string Codigo, string Nombre, int Nivel, TipoCuentaContable TipoCuenta, bool EsAceptable);

    private sealed record CajaAccounts(
        CuentaContableEntity CajaGeneral,
        CuentaContableEntity Bancos,
        CuentaContableEntity IngresoPorVentas,
        CuentaContableEntity OtrosIngresosSobrantes,
        CuentaContableEntity GastoFaltanteCaja);
}
