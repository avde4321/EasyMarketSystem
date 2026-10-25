using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TestDeIa.Api.Middleware;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Infrastructure.Persistence.Tenancy;

namespace TestDeIa.Tests.Saas;

public sealed class RequireActiveSubscriptionMiddlewareTests
{
    [Theory]
    [InlineData("/api/facturacion/monitor")]
    [InlineData("/api/inventario/productos")]
    public async Task InvokeAsync_ReturnsPaymentRequired_WhenSubscriptionIsMissingOrExpired(string path)
    {
        var empresaId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var nextWasCalled = false;
        var context = CreateAuthenticatedContext(path, userId);
        var tenantContext = new TenantContextAccessor
        {
            EmpresaId = empresaId,
            UserId = userId
        };
        var middleware = new RequireActiveSubscriptionMiddleware(
            _ =>
            {
                nextWasCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<RequireActiveSubscriptionMiddleware>.Instance);

        await middleware.InvokeAsync(
            context,
            tenantContext,
            new BlockingQuotaValidationService());

        Assert.False(nextWasCalled);
        Assert.Equal(StatusCodes.Status402PaymentRequired, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/api/saas/plans")]
    [InlineData("/api/saas/subscription-status")]
    [InlineData("/api/empresa")]
    [InlineData("/api/empresa/paged")]
    public async Task InvokeAsync_SkipsPublicPlansEndpoint(string path)
    {
        var nextWasCalled = false;
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var middleware = new RequireActiveSubscriptionMiddleware(
            _ =>
            {
                nextWasCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<RequireActiveSubscriptionMiddleware>.Instance);

        await middleware.InvokeAsync(
            context,
            new TenantContextAccessor(),
            new BlockingQuotaValidationService());

        Assert.True(nextWasCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static DefaultHttpContext CreateAuthenticatedContext(string path, Guid userId)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: "Test"));
        return context;
    }

    private sealed class BlockingQuotaValidationService : ISaaSQuotaValidationService
    {
        public Task EnsureActiveSubscriptionAsync(Guid empresaId, CancellationToken cancellationToken = default)
        {
            throw new SubscriptionRequiredException("Su empresa no cuenta con un plan activo o su periodo de suscripcion ha expirado.");
        }

        public Task CanEmitirFacturaAsync(Guid empresaId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task CanCrearUsuarioAsync(Guid empresaId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task CanCrearSucursalAsync(Guid empresaId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task IncrementarFacturasEmitidasMesActualAsync(Guid empresaId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
