using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestDeIa.Api.Configuration;
using TestDeIa.Api.Health;
using TestDeIa.Api.Middleware;
using TestDeIa.Api.Reporting;
using TestDeIa.Api.Security;
using TestDeIa.Application;
using TestDeIa.Infrastructure;
using TestDeIa.Infrastructure.Adapters.Out.Security;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSingleton<SimpleRidePdfRenderer>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("sqlserver");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(SecurityRateLimitPolicyNames.Login, limiter =>
    {
        limiter.PermitLimit = builder.Configuration.GetValue("Security:RateLimiting:LoginPermitLimit", 5);
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
    options.AddFixedWindowLimiter(SecurityRateLimitPolicyNames.IntegracionMasiva, limiter =>
    {
        limiter.PermitLimit = builder.Configuration.GetValue("Security:RateLimiting:IntegracionMasivaPermitLimit", 20);
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
    options.AddFixedWindowLimiter(SecurityRateLimitPolicyNames.WebhookPago, limiter =>
    {
        limiter.PermitLimit = builder.Configuration.GetValue("Security:RateLimiting:WebhookPagoPermitLimit", 60);
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var key = !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 240,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (origins.Length == 0 && !builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException("Configure Cors:AllowedOrigins para ambientes productivos.");
        }

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

var jwtIssuer = builder.Configuration["Security:Jwt:Issuer"] ?? "TestDeIa";
var jwtAudience = builder.Configuration["Security:Jwt:Audience"] ?? "TestDeIa.Client";
var jwtSigningKeys = JwtSigningKeyResolver.ResolveSigningKeys(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKeys = jwtSigningKeys.Select(current => current.SecurityKey),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdValue, out var userId))
                {
                    context.Fail("Token invalido.");
                    return;
                }

                var issuedAtValue = context.Principal?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
                _ = long.TryParse(issuedAtValue, out var issuedAtUnix);
                var issuedAt = issuedAtUnix > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(issuedAtUnix)
                    : DateTimeOffset.MinValue;

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<TestDeIaDbContext>();
                var user = await dbContext.SecurityUsers
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(current => current.Id == userId, context.HttpContext.RequestAborted);

                if (user is null || !user.IsActive || user.BloqueadoManualmente)
                {
                    context.Fail("La sesion ya no es valida para este usuario.");
                    return;
                }

                if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTimeOffset.UtcNow)
                {
                    context.Fail("La cuenta se encuentra bloqueada temporalmente.");
                    return;
                }

                if (user.TokensInvalidosDesde.HasValue && issuedAt <= user.TokensInvalidosDesde.Value)
                {
                    context.Fail("La sesion fue revocada. Inicia sesion nuevamente.");
                }
            }
        };
    });

builder.Services.AddApiComposition(builder.Configuration);

var app = builder.Build();

await app.UseDatabaseInitializationAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<SaaSQuotaExceptionMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();
