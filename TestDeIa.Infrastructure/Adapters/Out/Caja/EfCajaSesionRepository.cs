using Microsoft.EntityFrameworkCore;
using System.Data;
using TestDeIa.Application.Modules.Caja.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Caja.Entities;
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

        var (efectivo, tarjeta) = await CalcularTotalesVentasAsync(caja.Id, cancellationToken);
        caja.TotalVentasEfectivoCalculado = efectivo;
        caja.TotalVentasTarjetaCalculado = tarjeta;

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
            MontoFisicoEfectivoReal = 0,
            MontoFisicoTarjetaReal = 0,
            DiferenciaEfectivo = 0,
            DiferenciaTarjeta = 0,
            EstadoCaja = CajaEstado.Abierta,
            CreatedAt = now,
            UsuarioCreacionId = userId
        };

        dbContext.Set<CajaSesionEntity>().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<CajaSesion> CerrarAsync(decimal montoFisicoEfectivoReal, decimal montoFisicoTarjetaReal, CancellationToken cancellationToken = default)
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

        var (efectivo, tarjeta) = await CalcularTotalesVentasAsync(entity.Id, cancellationToken);
        var efectivoReal = Math.Round(montoFisicoEfectivoReal, 2);
        var tarjetaReal = Math.Round(montoFisicoTarjetaReal, 2);
        var updatedAt = DateTimeOffset.UtcNow;

        entity.TotalVentasEfectivoCalculado = efectivo;
        entity.TotalVentasTarjetaCalculado = tarjeta;
        entity.MontoFisicoEfectivoReal = efectivoReal;
        entity.MontoFisicoTarjetaReal = tarjetaReal;
        entity.DiferenciaEfectivo = Math.Round(efectivoReal - efectivo, 2);
        entity.DiferenciaTarjeta = Math.Round(tarjetaReal - tarjeta, 2);
        entity.FechaCierre = updatedAt;
        entity.EstadoCaja = CajaEstado.Cerrada;
        entity.UpdatedAt = updatedAt;
        entity.UsuarioModificacionId = usuarioId;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Map(entity);
    }

    private async Task<(decimal Efectivo, decimal Tarjeta)> CalcularTotalesVentasAsync(Guid cajaSesionId, CancellationToken cancellationToken)
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
            .Where(current => !string.Equals(current.FormaPagoSriCodigo, "01", StringComparison.Ordinal))
            .Sum(current => current.Total);

        return (Math.Round(efectivo, 2), Math.Round(tarjeta, 2));
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
            MontoFisicoEfectivoReal = entity.MontoFisicoEfectivoReal,
            MontoFisicoTarjetaReal = entity.MontoFisicoTarjetaReal,
            DiferenciaEfectivo = entity.DiferenciaEfectivo,
            DiferenciaTarjeta = entity.DiferenciaTarjeta,
            EstadoCaja = entity.EstadoCaja,
            CreatedAt = entity.CreatedAt,
            UsuarioCreacionId = entity.UsuarioCreacionId,
            UpdatedAt = entity.UpdatedAt,
            UsuarioModificacionId = entity.UsuarioModificacionId
        };
    }
}
