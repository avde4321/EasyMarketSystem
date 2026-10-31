using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Tesoreria.Ports.In;
using TestDeIa.Domain.Modules.Tesoreria.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Tesoreria;
using TestDeIa.Shared.Responses.Tesoreria;

namespace TestDeIa.Infrastructure.Adapters.Out.Tesoreria;

public sealed class TesoreriaService(
    TestDeIaDbContext dbContext,
    ITenantContextAccessor tenantContextAccessor,
    IBancoParserService bancoParserService,
    ILogger<TesoreriaService> logger) : ITesoreriaService
{
    public async Task<IReadOnlyCollection<CuentaBancariaDto>> GetCuentasBancariasAsync(CancellationToken cancellationToken = default)
    {
        var cuentas = await dbContext.CuentasBancarias
            .AsNoTracking()
            .Include(current => current.CuentaContable)
            .OrderBy(current => current.BancoNombre)
            .ThenBy(current => current.NumeroCuenta)
            .ToArrayAsync(cancellationToken);

        return cuentas.Select(MapCuenta).ToArray();
    }

    public async Task<CuentaBancariaDto> SaveCuentaBancariaAsync(Guid? id, CuentaBancariaRequest request, CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        var cuentaContableExists = await dbContext.CuentasContables
            .AnyAsync(current => current.EmpresaId == empresaId && current.Id == request.CuentaContableId, cancellationToken);

        if (!cuentaContableExists)
        {
            throw new InvalidOperationException("La cuenta contable seleccionada no existe para la empresa activa.");
        }

        CuentaBancariaEntity entity;
        if (id.HasValue && id.Value != Guid.Empty)
        {
            entity = await dbContext.CuentasBancarias
                .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Id == id.Value, cancellationToken)
                ?? throw new InvalidOperationException("La cuenta bancaria no existe o no pertenece a la empresa activa.");
        }
        else
        {
            entity = new CuentaBancariaEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId
            };
            dbContext.CuentasBancarias.Add(entity);
        }

        entity.BancoNombre = request.BancoNombre.Trim();
        entity.TipoCuenta = (TipoCuentaBancaria)request.TipoCuenta;
        entity.NumeroCuenta = request.NumeroCuenta.Trim();
        entity.SaldoContable = request.SaldoContable;
        entity.SaldoConciliado = request.SaldoConciliado;
        entity.Moneda = string.IsNullOrWhiteSpace(request.Moneda) ? "USD" : request.Moneda.Trim().ToUpperInvariant();
        entity.CuentaContableId = request.CuentaContableId;
        entity.Activa = request.Activa;

        await dbContext.SaveChangesAsync(cancellationToken);

        var cuenta = await dbContext.CuentasBancarias
            .AsNoTracking()
            .Include(current => current.CuentaContable)
            .Where(current => current.Id == entity.Id)
            .FirstAsync(cancellationToken);

        return MapCuenta(cuenta);
    }

    public async Task<ExtractoBancarioHeaderDto> ImportarExtractoAsync(
        Guid cuentaBancariaId,
        Stream fileStream,
        string fileName,
        string? formato,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        var cuenta = await dbContext.CuentasBancarias
            .FirstOrDefaultAsync(current => current.EmpresaId == empresaId && current.Id == cuentaBancariaId && current.Activa, cancellationToken)
            ?? throw new InvalidOperationException("La cuenta bancaria no existe, está inactiva o no pertenece a la empresa activa.");

        var extension = !string.IsNullOrWhiteSpace(formato)
            ? formato
            : Path.GetExtension(fileName);

        var detalles = await bancoParserService.ParsearExtractoBancarioAsync(fileStream, extension, cuenta.BancoNombre, cancellationToken);
        if (detalles.Count == 0)
        {
            throw new InvalidOperationException("No se encontraron movimientos válidos en el extracto bancario.");
        }

        var fechaDesde = detalles.Min(current => current.FechaTransaccion);
        var fechaHasta = detalles.Max(current => current.FechaTransaccion);
        var existingKeys = await dbContext.ExtractoBancarioDetalles
            .AsNoTracking()
            .Where(current =>
                current.EmpresaId == empresaId &&
                current.CuentaBancariaId == cuenta.Id &&
                current.FechaTransaccion >= fechaDesde.Date &&
                current.FechaTransaccion < fechaHasta.Date.AddDays(1))
            .Select(current => new
            {
                current.FechaTransaccion,
                current.NumeroDocumentoRef,
                current.Monto,
                current.TipoMovimiento
            })
            .ToArrayAsync(cancellationToken);

        var uniqueDetails = detalles
            .Where(detail => !existingKeys.Any(existing =>
                existing.FechaTransaccion.Date == detail.FechaTransaccion.Date &&
                existing.NumeroDocumentoRef == Truncate(detail.NumeroDocumentoRef, 50) &&
                existing.TipoMovimiento == detail.TipoMovimiento &&
                decimal.Round(existing.Monto, 4, MidpointRounding.AwayFromZero) == decimal.Round(detail.Monto, 4, MidpointRounding.AwayFromZero)))
            .ToArray();

        var header = new ExtractoBancarioHeaderEntity
        {
            Id = Guid.NewGuid(),
            CuentaBancariaId = cuenta.Id,
            FechaImportacion = DateTime.UtcNow,
            FechaDesde = fechaDesde.Date,
            FechaHasta = fechaHasta.Date,
            NombreArchivoOriginal = Path.GetFileName(fileName),
            TotalRegistros = uniqueDetails.Length,
            Observaciones = uniqueDetails.Length == detalles.Count
                ? $"Importado automáticamente desde {cuenta.BancoNombre}."
                : $"Importado automáticamente desde {cuenta.BancoNombre}. Se omitieron {detalles.Count - uniqueDetails.Length} registros duplicados."
        };

        foreach (var detail in uniqueDetails)
        {
            header.Detalles.Add(new ExtractoBancarioDetalleEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                CuentaBancariaId = cuenta.Id,
                ExtractoHeaderId = header.Id,
                FechaTransaccion = detail.FechaTransaccion.Date,
                NumeroDocumentoRef = Truncate(detail.NumeroDocumentoRef, 50),
                ConceptoDescripcion = Truncate(detail.ConceptoDescripcion, 500),
                TipoMovimiento = detail.TipoMovimiento,
                Monto = detail.Monto,
                Conciliado = false
            });
        }

        dbContext.ExtractoBancarioHeaders.Add(header);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Extracto bancario importado. EmpresaId={EmpresaId}; CuentaBancariaId={CuentaBancariaId}; Archivo={Archivo}; Registros={Registros}; DuplicadosOmitidos={DuplicadosOmitidos}.",
            empresaId,
            cuenta.Id,
            fileName,
            uniqueDetails.Length,
            detalles.Count - uniqueDetails.Length);

        return new ExtractoBancarioHeaderDto
        {
            Id = header.Id,
            CuentaBancariaId = cuenta.Id,
            BancoNombre = cuenta.BancoNombre,
            NumeroCuenta = cuenta.NumeroCuenta,
            FechaImportacion = header.FechaImportacion,
            FechaDesde = header.FechaDesde,
            FechaHasta = header.FechaHasta,
            NombreArchivoOriginal = header.NombreArchivoOriginal,
            TotalRegistros = header.TotalRegistros,
            Observaciones = header.Observaciones
        };
    }

    public async Task<IReadOnlyCollection<MovimientoTesoreriaDto>> GetMovimientosPendientesAsync(
        Guid cuentaBancariaId,
        DateTime? desde,
        DateTime? hasta,
        string? term,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        var query = dbContext.MovimientosTesoreria
            .AsNoTracking()
            .Include(current => current.CuentaBancaria)
            .Where(current =>
                current.EmpresaId == empresaId &&
                current.CuentaBancariaId == cuentaBancariaId &&
                current.EstadoConciliacion == EstadoConciliacionTesoreria.Pendiente);

        if (desde.HasValue)
        {
            query = query.Where(current => current.Fecha >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            query = query.Where(current => current.Fecha < hasta.Value.Date.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.Trim();
            query = query.Where(current => current.Beneficiario.Contains(normalized));
        }

        var items = await query
            .OrderByDescending(current => current.Fecha)
            .Take(300)
            .ToArrayAsync(cancellationToken);

        return items.Select(current => new MovimientoTesoreriaDto
        {
            Id = current.Id,
            EmpresaId = current.EmpresaId,
            CuentaBancariaId = current.CuentaBancariaId,
            BancoNombre = current.CuentaBancaria.BancoNombre,
            NumeroCuenta = current.CuentaBancaria.NumeroCuenta,
            Fecha = current.Fecha,
            Tipo = (byte)current.Tipo,
            TipoNombre = current.Tipo.ToString(),
            Monto = current.Monto,
            Beneficiario = current.Beneficiario,
            FacturaVentaId = current.FacturaVentaId,
            CompraId = current.CompraId,
            AsientoContableId = current.AsientoContableId,
            EstadoConciliacion = (byte)current.EstadoConciliacion,
            EstadoConciliacionNombre = current.EstadoConciliacion.ToString()
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<ExtractoBancarioDetalleDto>> GetExtractosPendientesAsync(
        Guid cuentaBancariaId,
        DateTime? desde,
        DateTime? hasta,
        byte? tipoMovimiento,
        string? term,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ResolveEmpresaId();
        var query = dbContext.ExtractoBancarioDetalles
            .AsNoTracking()
            .Include(current => current.ExtractoHeader)
            .ThenInclude(current => current.CuentaBancaria)
            .Where(current =>
                current.EmpresaId == empresaId &&
                current.CuentaBancariaId == cuentaBancariaId &&
                !current.Conciliado);

        if (desde.HasValue)
        {
            query = query.Where(current => current.FechaTransaccion >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            query = query.Where(current => current.FechaTransaccion < hasta.Value.Date.AddDays(1));
        }

        if (tipoMovimiento.HasValue && Enum.IsDefined(typeof(TipoMovimientoBancario), tipoMovimiento.Value))
        {
            var tipo = (TipoMovimientoBancario)tipoMovimiento.Value;
            query = query.Where(current => current.TipoMovimiento == tipo);
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.Trim();
            query = query.Where(current =>
                current.NumeroDocumentoRef.Contains(normalized) ||
                current.ConceptoDescripcion.Contains(normalized));
        }

        var items = await query
            .OrderByDescending(current => current.FechaTransaccion)
            .Take(300)
            .ToArrayAsync(cancellationToken);

        return items.Select(current => new ExtractoBancarioDetalleDto
        {
            Id = current.Id,
            ExtractoHeaderId = current.ExtractoHeaderId,
            FechaTransaccion = current.FechaTransaccion,
            NumeroDocumentoRef = current.NumeroDocumentoRef,
            ConceptoDescripcion = current.ConceptoDescripcion,
            TipoMovimiento = (byte)current.TipoMovimiento,
            TipoMovimientoNombre = current.TipoMovimiento.ToString(),
            Monto = current.Monto,
            Conciliado = current.Conciliado,
            MovimientoTesoreriaId = current.MovimientoTesoreriaId
        }).ToArray();
    }

    private Guid ResolveEmpresaId()
    {
        return tenantContextAccessor.EmpresaId
            ?? throw new InvalidOperationException("No existe una empresa activa para tesorería.");
    }

    private static CuentaBancariaDto MapCuenta(CuentaBancariaEntity entity)
    {
        return new CuentaBancariaDto
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            BancoNombre = entity.BancoNombre,
            TipoCuenta = (byte)entity.TipoCuenta,
            TipoCuentaNombre = entity.TipoCuenta.ToString(),
            NumeroCuenta = entity.NumeroCuenta,
            SaldoContable = entity.SaldoContable,
            SaldoConciliado = entity.SaldoConciliado,
            Moneda = entity.Moneda,
            CuentaContableId = entity.CuentaContableId,
            CuentaContableCodigo = entity.CuentaContable.Codigo,
            CuentaContableNombre = entity.CuentaContable.Nombre,
            Activa = entity.Activa
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
