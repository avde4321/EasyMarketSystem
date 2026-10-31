using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;

namespace TestDeIa.Infrastructure.Persistence;

public sealed class ApplicationTransactionRunner(TestDeIaDbContext db) : IApplicationTransactionRunner
{
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (db.Database.CurrentTransaction is not null || !db.Database.IsRelational()) return await action();
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var result = await action();
        await tx.CommitAsync(cancellationToken);
        return result;
    }
}
