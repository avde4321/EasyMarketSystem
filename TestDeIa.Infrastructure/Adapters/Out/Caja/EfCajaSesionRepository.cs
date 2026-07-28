using Microsoft.EntityFrameworkCore;
using System.Data;
using TestDeIa.Application.Modules.Caja.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Caja.Entities;
using TestDeIa.Domain.Modules.Contabilidad.Enums;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

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
                current.EstadoCaja == CajaEstado.Abierta,
                cancellationToken);

        if (caja is null)
        {
            return null;
        }

        var (efectivo, tarjeta, transferencia) = await CalcularTotalesVentasAsync(caja.Id, cancellationToken);
        caja.TotalVentasEfectivoCalculado = efectivo;
        caja.TotalVentasTarjetaCalculado = tarjeta;
        caja.TotalVentasTransferenciaCalculado = transferencia;

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
                current.EstadoCaja == CajaEstado.Abierta,
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
        var entity = new CajaSesionEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            UsuarioId = userId,
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
        CancellationToken cancellationToken = default)
    {
        var empresaId = currentUserAccessor.GetRequiredEmpresaId();
        var usuarioId = currentUserAccessor.GetRequiredUserId();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var entity = await dbContext.Set<CajaSesionEntity>()
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.UsuarioId == usuarioId &&
                current.EstadoCaja == CajaEstado.Abierta,
                cancellationToken)
            ?? throw new InvalidOperationException("No tienes una caja abierta para cerrar.");

        var (efectivo, tarjeta, transferencia) = await CalcularTotalesVentasAsync(entity.Id, cancellationToken);
        var efectivoReal = Math.Round(montoFisicoEfectivoReal, 2);
        var tarjetaReal = Math.Round(montoFisicoTarjetaReal, 2);
        var transferenciaReal = Math.Round(montoFisicoTransferenciaReal, 2);
        var diferenciaEfectivo = Math.Round(efectivoReal - (entity.MontoApertura + efectivo), 2);
        var diferenciaTarjeta = Math.Round(tarjetaReal - tarjeta, 2);
        var diferenciaTransferencia = Math.Round(transferenciaReal - transferencia, 2);
        var diferenciaTotal = Math.Round(diferenciaEfectivo + diferenciaTarjeta + diferenciaTransferencia, 2);
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
        entity.FechaCierre = updatedAt;
        entity.EstadoCaja = CajaEstado.Cerrada;
        entity.AsientoContableId = asiento.Id;
        entity.UpdatedAt = updatedAt;
        entity.UsuarioModificacionId = usuarioId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Map(entity);
    }

    private async Task<(decimal Efectivo, decimal Tarjeta, decimal Transferencia)> CalcularTotalesVentasAsync(Guid cajaSesionId, CancellationToken cancellationToken)
    {
        var facturas = await dbContext.Set<FacturaEntity>()
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

        var efectivo = facturas
            .Where(current => string.Equals(current.FormaPagoSriCodigo, "01", StringComparison.Ordinal))
            .Sum(current => current.Total);

        var tarjeta = facturas
            .Where(current => IsCardPayment(current.FormaPagoSriCodigo))
            .Sum(current => current.Total);

        var transferencia = facturas
            .Where(current => !string.Equals(current.FormaPagoSriCodigo, "01", StringComparison.Ordinal) && !IsCardPayment(current.FormaPagoSriCodigo))
            .Sum(current => current.Total);

        return (Math.Round(efectivo, 2), Math.Round(tarjeta, 2), Math.Round(transferencia, 2));
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
            caja.TotalVentasEfectivoCalculado + caja.TotalVentasTarjetaCalculado + caja.TotalVentasTransferenciaCalculado,
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

    private static CajaSesion Map(CajaSesionEntity entity)
    {
        return new CajaSesion
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            UsuarioId = entity.UsuarioId,
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
