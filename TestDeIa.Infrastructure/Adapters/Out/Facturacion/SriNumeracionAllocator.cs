using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

// El bloqueo pertenece a la transacción de emisión/configuración, no a la instancia del servidor.
internal static class SriNumeracionAllocator
{
    internal static async Task LockAsync(TestDeIaDbContext db, Guid empresa, string ambiente, string tipo, string estab, string punto, CancellationToken ct)
    {
        if (!db.Database.IsSqlServer()) return;
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("La numeración requiere una transacción activa.");
        var resource = $"SRI-NUM:{empresa:N}:{ambiente}:{tipo}:{estab}:{punto}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; IF @result < 0 THROW 51000, 'No se pudo reservar la numeracion. Intenta nuevamente.', 1;", ct);
    }

    internal static async Task<long> HistoricalMaximumAsync(TestDeIaDbContext db, Guid empresa, string ambiente, string tipo, string estab, string punto, CancellationToken ct)
    {
        if (tipo == "01")
            return await db.Facturas.AsNoTracking().Where(x => x.EmpresaId == empresa && x.AmbienteSri == ambiente && x.Establecimiento == estab && x.PuntoEmision == punto).MaxAsync(x => (long?)x.Secuencial, ct) ?? 0;
        if (tipo == "04")
            return await db.ComprobanteCabecera.AsNoTracking().Where(x => x.EmpresaId == empresa && x.AmbienteSri == ambiente && x.TipoDocumentoId == tipo && x.Establecimiento == estab && x.PuntoEmision == punto).MaxAsync(x => (long?)x.Secuencial, ct) ?? 0;
        if (tipo == "07")
        {
            var last = await db.ComprobantesRetencion.AsNoTracking().Where(x => x.EmpresaId == empresa && (int)x.AmbienteSRI == (ambiente == "2" ? 2 : 1) && x.Establecimiento == estab && x.PuntoEmision == punto).MaxAsync(x => x.Secuencial, ct);
            return long.TryParse(last, out var value) ? value : 0;
        }
        var key = await db.Compras.AsNoTracking().Where(x => x.EmpresaId == empresa && x.TipoDocumentoCodigo == tipo && x.Establecimiento == estab && x.PuntoEmision == punto && x.AmbienteSri == ambiente).MaxAsync(x => x.Secuencial, ct);
        return long.TryParse(key, out var number) ? number : 0;
    }

    internal static async Task<long> ReserveAsync(TestDeIaDbContext db, Guid empresa, string ambiente, string tipo, string estab, string punto, CancellationToken ct)
    {
        if (ambiente is not ("1" or "2")) throw new InvalidOperationException("Ambiente SRI inválido.");
        await LockAsync(db, empresa, ambiente, tipo, estab, punto, ct);
        var record = await db.FacturaSecuenciales.SingleOrDefaultAsync(x => x.EmpresaId == empresa && x.AmbienteSri == ambiente && x.CodigoDocumento == tipo && x.Establecimiento == estab && x.PuntoEmision == punto, ct);
        var last = Math.Max(record?.UltimoSecuencial ?? 0, await HistoricalMaximumAsync(db, empresa, ambiente, tipo, estab, punto, ct));
        if (last >= 999999999) throw new InvalidOperationException("Secuencial agotado. Configure otro punto de emisión.");
        if (record is null)
        {
            record = new FacturaSecuencialEntity { Id = Guid.NewGuid(), EmpresaId = empresa, AmbienteSri = ambiente, CodigoDocumento = tipo, Establecimiento = estab, PuntoEmision = punto, CreatedAt = DateTimeOffset.UtcNow };
            db.FacturaSecuenciales.Add(record);
        }
        record.UltimoSecuencial = last + 1;
        record.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return record.UltimoSecuencial;
    }
}
