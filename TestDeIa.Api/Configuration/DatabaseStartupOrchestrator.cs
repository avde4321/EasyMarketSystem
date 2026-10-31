using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Api.Configuration;

public static class DatabaseStartupOrchestrator
{
    public static async Task UseDatabaseInitializationAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        try
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            await initializer.InitializeAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo inicializar la base de datos. Verifica la conexion al motor SQL Server y las credenciales configuradas.");
        }
    }
}
