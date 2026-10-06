using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TestDeIa.Shared.Security;
using TestDeIa.Application.Modules.Security.Models;
using TestDeIa.Infrastructure.Adapters.Out.Security;

namespace TestDeIa.Tests.Security;

public sealed class SecurityHardeningTests
{
    private const string PrimarySecret = "Primary_Test_Key_For_SaaS_Hardening_2026_Change_Outside_Code";
    private const string SecondarySecret = "Secondary_Test_Key_For_SaaS_Hardening_2026_Keep_For_Rotation";

    [Fact]
    public void JwtGeneratorUsesPrimarySigningKeyAndSupportsRotation()
    {
        var configuration = BuildJwtConfiguration();
        var generator = new JwtSecurityTokenGenerator(configuration);

        var generated = generator.Generate(new AuthenticatedUser(
            Guid.NewGuid(),
            "admin",
            "Administrador",
            "9999999999",
            "admin@test.local",
            Guid.NewGuid(),
            ["Administrador"],
            ["usuarios.administrar"]));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(generated.AccessToken);

        Assert.Equal("primary", token.Header.Kid);
        Assert.Contains(JwtSigningKeyResolver.ResolveSigningKeys(configuration), current => current.KeyId == "secondary");
    }

    [Fact]
    public void JwtValidationRejectsExpiredTokens()
    {
        var configuration = BuildJwtConfiguration();
        var primaryKey = JwtSigningKeyResolver.ResolvePrimarySigningKey(configuration);
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var expiredToken = new JwtSecurityToken(
            issuer: "TestDeIa",
            audience: "TestDeIa.Client",
            claims: [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
            notBefore: now.AddMinutes(-30),
            expires: now.AddMinutes(-5),
            signingCredentials: new SigningCredentials(primaryKey.SecurityKey, SecurityAlgorithms.HmacSha256));
        expiredToken.Header[JwtHeaderParameterNames.Kid] = primaryKey.KeyId;

        var tokenText = handler.WriteToken(expiredToken);

        Assert.Throws<SecurityTokenExpiredException>(() =>
            handler.ValidateToken(tokenText, BuildStrictValidationParameters(configuration), out _));
    }

    [Fact]
    public void LoginEndpointHasDedicatedRateLimitingPolicy()
    {
        var source = File.ReadAllText(GetRepositoryFile("TestDeIa.Api", "Controllers", "SecurityController.cs"));

        Assert.Contains("[HttpPost(\"login\")]", source, StringComparison.Ordinal);
        Assert.Contains($"[EnableRateLimiting(SecurityRateLimitPolicyNames.{nameof(SecurityRateLimitPolicyNames.Login)})]", source, StringComparison.Ordinal);
    }

    private static IConfiguration BuildJwtConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Jwt:Issuer"] = "TestDeIa",
                ["Security:Jwt:Audience"] = "TestDeIa.Client",
                ["Security:Jwt:ExpirationMinutes"] = "30",
                ["Security:Jwt:SigningKeys:0:KeyId"] = "primary",
                ["Security:Jwt:SigningKeys:0:Secret"] = PrimarySecret,
                ["Security:Jwt:SigningKeys:0:IsActive"] = "true",
                ["Security:Jwt:SigningKeys:1:KeyId"] = "secondary",
                ["Security:Jwt:SigningKeys:1:Secret"] = SecondarySecret,
                ["Security:Jwt:SigningKeys:1:IsActive"] = "true"
            })
            .Build();
    }

    private static TokenValidationParameters BuildStrictValidationParameters(IConfiguration configuration)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidIssuer = configuration["Security:Jwt:Issuer"],
            ValidAudience = configuration["Security:Jwt:Audience"],
            IssuerSigningKeys = JwtSigningKeyResolver.ResolveSigningKeys(configuration).Select(current => current.SecurityKey),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero
        };
    }

    private static string GetRepositoryFile(params string[] relativeParts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TestDeIa.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. relativeParts]);
    }
}

