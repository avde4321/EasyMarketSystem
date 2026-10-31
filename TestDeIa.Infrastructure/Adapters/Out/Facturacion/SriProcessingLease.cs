using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

// Exclusión distribuida sin mantener una transacción SQL abierta durante las llamadas HTTP.
internal sealed class SriProcessingLease(TestDeIaDbContext db, string resource) : IAsyncDisposable
{
    internal static async Task<SriProcessingLease> AcquireAsync(TestDeIaDbContext db, Guid empresa, Guid document, CancellationToken ct)
    {
        var lease = new SriProcessingLease(db, $"SRI-PROCESS:{empresa:N}:{document:N}");
        if (!db.Database.IsSqlServer()) return lease;
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            var name = $"SRI-PROCESS:{empresa:N}:{document:N}";
            await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={name}, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=0; IF @r < 0 THROW 51000, 'Este comprobante ya se esta procesando.', 1;", ct);
            return lease;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 51000)
        {
            await db.Database.CloseConnectionAsync();
            throw new InvalidOperationException("Este comprobante ya se está procesando. Espere antes de reintentar.", ex);
        }
        catch { await db.Database.CloseConnectionAsync(); throw; }
    }
    public async ValueTask DisposeAsync()
    {
        if (!db.Database.IsSqlServer()) return;
        try { await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_releaseapplock @Resource={resource}, @LockOwner='Session';", CancellationToken.None); }
        finally { await db.Database.CloseConnectionAsync(); }
    }
}
