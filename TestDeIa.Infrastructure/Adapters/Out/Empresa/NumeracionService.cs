using System.Data;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Infrastructure.Adapters.Out.Empresa;

public sealed class NumeracionService(TestDeIaDbContext db, ITenantContextAccessor tenant) : INumeracionService
{
    private Guid Empresa => tenant.EmpresaId ?? throw new UnauthorizedAccessException("Seleccione una empresa.");

    public async Task<IReadOnlyCollection<NumeracionResponse>> ConsultarAsync(string ambiente, CancellationToken ct)
    {
        if (ambiente is not ("1" or "2")) throw new InvalidOperationException("Ambiente inválido.");
        var empresa = Empresa;
        var puntos = await db.EmpresaPuntosEmision.AsNoTracking().Where(x => x.EmpresaEmisoraId == empresa).OrderBy(x => x.Establecimiento).ThenBy(x => x.PuntoEmision).ToListAsync(ct);
        var counters = await db.FacturaSecuenciales.AsNoTracking().Where(x => x.EmpresaId == empresa && x.AmbienteSri == ambiente).ToListAsync(ct);
        var result = new List<NumeracionResponse>();
        foreach (var p in puntos)
        foreach (var tipo in new[] { "01", "03", "04", "07" })
        {
            var contador = counters.SingleOrDefault(x => x.CodigoDocumento == tipo && x.Establecimiento == p.Establecimiento && x.PuntoEmision == p.PuntoEmision)?.UltimoSecuencial ?? 0;
            result.Add(new NumeracionResponse { PuntoEmisionId = p.Id, Establecimiento = p.Establecimiento, PuntoEmision = p.PuntoEmision, AmbienteSri = ambiente, CodigoDocumento = tipo, UltimoSecuencial = Math.Max(contador, await SriNumeracionAllocator.HistoricalMaximumAsync(db, empresa, ambiente, tipo, p.Establecimiento, p.PuntoEmision, ct)) });
        }
        return result;
    }

    public async Task ConfigurarAsync(NumeracionRequest request, CancellationToken ct)
    {
        var empresa = Empresa;
        var user = tenant.UserId ?? throw new UnauthorizedAccessException();
        if (request.AmbienteSri is not ("1" or "2") || request.CodigoDocumento is not ("01" or "03" or "04" or "07") || request.UltimoSecuencial < 0 || request.UltimoSecuencial > 999999998 || request.Motivo.Trim().Length < 5 || request.Motivo.Length > 500)
            throw new InvalidOperationException("Revise los datos y registre el motivo del cambio.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var p = await db.EmpresaPuntosEmision.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.PuntoEmisionId && x.EmpresaEmisoraId == empresa, ct)
            ?? throw new InvalidOperationException("El punto no pertenece a la empresa activa.");
        await SriNumeracionAllocator.LockAsync(db, empresa, request.AmbienteSri, request.CodigoDocumento, p.Establecimiento, p.PuntoEmision, ct);
        var record = await db.FacturaSecuenciales.SingleOrDefaultAsync(x => x.EmpresaId == empresa && x.AmbienteSri == request.AmbienteSri && x.CodigoDocumento == request.CodigoDocumento && x.Establecimiento == p.Establecimiento && x.PuntoEmision == p.PuntoEmision, ct);
        var actual = Math.Max(record?.UltimoSecuencial ?? 0, await SriNumeracionAllocator.HistoricalMaximumAsync(db, empresa, request.AmbienteSri, request.CodigoDocumento, p.Establecimiento, p.PuntoEmision, ct));
        if (actual != request.ValorEsperado) throw new InvalidOperationException("La numeración cambió mientras editaba. Actualice la consulta.");
        if (request.UltimoSecuencial < actual) throw new InvalidOperationException("No se permite retroceder la numeración ya reservada o utilizada.");
        if (record is null)
        {
            record = new FacturaSecuencialEntity { Id = Guid.NewGuid(), EmpresaId = empresa, AmbienteSri = request.AmbienteSri, CodigoDocumento = request.CodigoDocumento, Establecimiento = p.Establecimiento, PuntoEmision = p.PuntoEmision, CreatedAt = DateTimeOffset.UtcNow };
            db.FacturaSecuenciales.Add(record);
        }
        record.UltimoSecuencial = request.UltimoSecuencial;
        record.UpdatedAt = DateTimeOffset.UtcNow;
        db.Set<NumeracionHistorialEntity>().Add(new NumeracionHistorialEntity { Id = Guid.NewGuid(), EmpresaId = empresa, UsuarioId = user, Fecha = DateTimeOffset.UtcNow, Serie = $"{p.Establecimiento}-{p.PuntoEmision}", AmbienteSri = request.AmbienteSri, CodigoDocumento = request.CodigoDocumento, Anterior = actual, Nuevo = request.UltimoSecuencial, Motivo = request.Motivo.Trim() });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyCollection<NumeracionHistorialResponse>> HistorialAsync(CancellationToken ct)
    {
        var empresa = Empresa;
        return await db.Set<NumeracionHistorialEntity>().AsNoTracking().Where(x => x.EmpresaId == empresa).OrderByDescending(x => x.Fecha).Take(50).Select(x => new NumeracionHistorialResponse { Fecha = x.Fecha, UsuarioId = x.UsuarioId, Serie = x.Serie, AmbienteSri = x.AmbienteSri, CodigoDocumento = x.CodigoDocumento, Anterior = x.Anterior, Nuevo = x.Nuevo, Motivo = x.Motivo }).ToArrayAsync(ct);
    }
}
