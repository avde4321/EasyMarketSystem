using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class EfCuentaPorPagarRepository : ICuentaPorPagarRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfCuentaPorPagarRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<CuentaPorPagar> CreateAsync(CuentaPorPagar cuentaPorPagar, CancellationToken cancellationToken = default)
    {
        var entity = new CuentaPorPagarEntity
        {
            Id = cuentaPorPagar.Id,
            EmpresaId = cuentaPorPagar.EmpresaId,
            CompraId = cuentaPorPagar.CompraId,
            ProveedorId = cuentaPorPagar.ProveedorId,
            FechaEmision = cuentaPorPagar.FechaEmision,
            FechaVence = cuentaPorPagar.FechaVence,
            MontoOriginal = cuentaPorPagar.MontoOriginal,
            SaldoActual = cuentaPorPagar.SaldoActual,
            EstadoDeuda = cuentaPorPagar.EstadoDeuda.ToString(),
            CreatedAt = cuentaPorPagar.CreatedAt,
            UsuarioCreacionId = cuentaPorPagar.UsuarioCreacionId,
            UpdatedAt = cuentaPorPagar.UpdatedAt,
            UsuarioModificacionId = cuentaPorPagar.UsuarioModificacionId
        };

        dbContext.CuentasPorPagar.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequiredAsync(entity.Id, cancellationToken);
    }

    public async Task<PagedResultResponse<CuentaPorPagar>> GetPagedAsync(string? term, Guid? proveedorId, int skip, int take, CancellationToken cancellationToken = default)
    {
        await NormalizeEstadosVencidosAsync(cancellationToken);

        var query = BaseQuery();
        if (proveedorId.HasValue && proveedorId.Value != Guid.Empty)
        {
            query = query.Where(current => current.ProveedorId == proveedorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.Trim();
            query = query.Where(current =>
                current.Proveedor.Persona.Identificacion.Contains(normalized) ||
                current.Proveedor.Persona.RazonSocialONombresCompletos.Contains(normalized) ||
                (current.Compra != null && (
                    current.Compra.Establecimiento.Contains(normalized) ||
                    current.Compra.PuntoEmision.Contains(normalized) ||
                    current.Compra.Secuencial.Contains(normalized))));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(current => current.EstadoDeuda == EstadoDeuda.Vencida.ToString())
            .ThenBy(current => current.FechaVence)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<CuentaPorPagar>
        {
            Items = items.Select(Map).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<CuentasPorPagarResumen> GetResumenAsync(CancellationToken cancellationToken = default)
    {
        await NormalizeEstadosVencidosAsync(cancellationToken);

        var now = DateTimeOffset.Now.Date;
        var cuentas = await BaseQuery()
            .Where(current => current.EstadoDeuda != EstadoDeuda.Pagada.ToString())
            .ToListAsync(cancellationToken);

        return new CuentasPorPagarResumen
        {
            TotalPorVencer = cuentas.Where(current => current.FechaVence.Date >= now).Sum(current => current.SaldoActual),
            TotalVencido = cuentas.Where(current => current.FechaVence.Date < now).Sum(current => current.SaldoActual),
            TotalPendientes = cuentas.Count(current => current.FechaVence.Date >= now),
            TotalVencidas = cuentas.Count(current => current.FechaVence.Date < now)
        };
    }

    public async Task<CuentaPorPagar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await NormalizeEstadosVencidosAsync(cancellationToken);
        var entity = await BaseQuery().FirstOrDefaultAsync(current => current.Id == id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<CuentaPorPagar> RegistrarAbonoAsync(PagoCxP pago, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        await NormalizeEstadosVencidosAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var cuenta = await dbContext.CuentasPorPagar
            .Include(current => current.Pagos)
            .Include(current => current.Proveedor)
            .ThenInclude(current => current.Persona)
            .Include(current => current.Compra)
            .FirstOrDefaultAsync(current => current.Id == pago.CuentaPorPagarId, cancellationToken)
            ?? throw new InvalidOperationException("La cuenta por pagar no existe en la empresa activa.");

        if (cuenta.EstadoDeuda == EstadoDeuda.Pagada.ToString())
        {
            throw new InvalidOperationException("La cuenta por pagar ya se encuentra pagada.");
        }

        if (pago.MontoPagado <= 0)
        {
            throw new InvalidOperationException("El abono debe ser mayor a cero.");
        }

        if (pago.MontoPagado > cuenta.SaldoActual)
        {
            throw new InvalidOperationException("El abono no puede superar el saldo pendiente.");
        }

        var pagoEntity = new PagoCxPEntity
        {
            Id = pago.Id,
            CuentaPorPagarId = cuenta.Id,
            FechaPago = pago.FechaPago,
            MontoPagado = pago.MontoPagado,
            FormaPago = pago.FormaPago,
            ReferenciaTransaccion = pago.ReferenciaTransaccion,
            CreatedAt = pago.CreatedAt,
            UsuarioCreacionId = pago.UsuarioCreacionId
        };

        dbContext.PagosCxP.Add(pagoEntity);

        cuenta.SaldoActual = Math.Round(cuenta.SaldoActual - pago.MontoPagado, 2, MidpointRounding.AwayFromZero);
        cuenta.EstadoDeuda = ResolveEstadoDeuda(cuenta.MontoOriginal, cuenta.SaldoActual, cuenta.FechaVence, DateTimeOffset.Now).ToString();
        cuenta.UpdatedAt = DateTimeOffset.UtcNow;
        cuenta.UsuarioModificacionId = usuarioId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Map(cuenta);
    }

    private IQueryable<CuentaPorPagarEntity> BaseQuery()
    {
        return dbContext.CuentasPorPagar
            .AsNoTracking()
            .Include(current => current.Pagos.OrderByDescending(payment => payment.FechaPago))
            .Include(current => current.Proveedor)
            .ThenInclude(current => current.Persona)
            .Include(current => current.Compra);
    }

    private async Task NormalizeEstadosVencidosAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        await dbContext.CuentasPorPagar
            .Where(current =>
                current.EstadoDeuda != EstadoDeuda.Pagada.ToString() &&
                current.SaldoActual > 0 &&
                current.FechaVence < now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(current => current.EstadoDeuda, EstadoDeuda.Vencida.ToString()),
                cancellationToken);

        await dbContext.CuentasPorPagar
            .Where(current =>
                current.EstadoDeuda == EstadoDeuda.Vencida.ToString() &&
                current.SaldoActual > 0 &&
                current.FechaVence >= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(current => current.EstadoDeuda, EstadoDeuda.Pendiente.ToString()),
                cancellationToken);
    }

    private async Task<CuentaPorPagar> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await BaseQuery().FirstAsync(current => current.Id == id, cancellationToken);
        return Map(entity);
    }

    private static EstadoDeuda ResolveEstadoDeuda(decimal montoOriginal, decimal saldoActual, DateTimeOffset fechaVence, DateTimeOffset now)
    {
        if (saldoActual <= 0)
        {
            return EstadoDeuda.Pagada;
        }

        if (fechaVence.Date < now.Date)
        {
            return EstadoDeuda.Vencida;
        }

        return saldoActual < montoOriginal ? EstadoDeuda.Abonada : EstadoDeuda.Pendiente;
    }

    private static CuentaPorPagar Map(CuentaPorPagarEntity entity)
    {
        var proveedorNombre = entity.Proveedor.Persona.RazonSocialONombresCompletos;
        var proveedorIdentificacion = entity.Proveedor.Persona.Identificacion;

        return new CuentaPorPagar(
            entity.Id,
            entity.EmpresaId,
            entity.CompraId,
            entity.ProveedorId,
            proveedorIdentificacion,
            proveedorNombre,
            entity.Compra is null ? string.Empty : $"{entity.Compra.Establecimiento}-{entity.Compra.PuntoEmision}-{entity.Compra.Secuencial}",
            entity.FechaEmision,
            entity.FechaVence,
            entity.MontoOriginal,
            entity.SaldoActual,
            Enum.TryParse<EstadoDeuda>(entity.EstadoDeuda, true, out var estado) ? estado : EstadoDeuda.Pendiente,
            entity.CreatedAt,
            entity.UsuarioCreacionId,
            entity.UpdatedAt,
            entity.UsuarioModificacionId,
            entity.Pagos
                .OrderByDescending(current => current.FechaPago)
                .Select(current => new PagoCxP(
                    current.Id,
                    current.CuentaPorPagarId,
                    current.FechaPago,
                    current.MontoPagado,
                    current.FormaPago,
                    current.ReferenciaTransaccion,
                    current.CreatedAt,
                    current.UsuarioCreacionId))
                .ToArray());
    }
}
