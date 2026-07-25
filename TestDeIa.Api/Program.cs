using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestDeIa.Api.Configuration;
using TestDeIa.Api.Middleware;
using TestDeIa.Api.Security;
using TestDeIa.Application;
using TestDeIa.Infrastructure;
using TestDeIa.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtSecret = builder.Configuration["Security:Jwt:Secret"]
    ?? "TestDeIa_Crm_Development_Secret_Key_Change_In_Production";
var jwtIssuer = builder.Configuration["Security:Jwt:Issuer"] ?? "TestDeIa";
var jwtAudience = builder.Configuration["Security:Jwt:Audience"] ?? "TestDeIa.Client";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
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

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
