using Microsoft.Extensions.Diagnostics.HealthChecks;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Api.Health;

public sealed class DatabaseHealthCheck(TestDeIaDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("SQL Server disponible.")
                : HealthCheckResult.Unhealthy("SQL Server no responde.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("SQL Server no responde.", exception);
        }
    }
}
