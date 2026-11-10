using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TestDeIa.Shared.Security;
using TestDeIa.Application.Modules.Security.Models;
using TestDeIa.Api.Configuration;
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
    public void SuperAdministratorReceivesStandardErpPermissionsEvenWhenLegacyRoleLinksAreMissing()
    {
        var permissions = EfSecurityUserRepository.ResolvePermissions(
            [SecurityRoleNames.SuperAdministrador],
            Array.Empty<string>());

        Assert.Equal(SecurityPermissionCatalog.RoleMappings[SecurityRoleNames.SuperAdministrador].Count, permissions.Length);
        Assert.Contains(SecurityPermissions.DashboardView, permissions);
        Assert.Contains(SecurityPermissions.PosFacturar, permissions);
        Assert.Contains(SecurityPermissions.EmpresaConfigurar, permissions);
        Assert.Contains(SecurityPermissions.ReporteriaVentas, permissions);
    }

    [Fact]
    public void SuperAdministratorCanPassEveryErpPermissionPolicyWithoutChangingOtherRoles()
    {
        var owner = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, SecurityRoleNames.SuperAdministrador)], "test"));
        var tenantUser = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Role, SecurityRoleNames.Cajero),
                new Claim(SecurityClaimTypes.Permission, SecurityPermissions.PosFacturar)
            ], "test"));

        foreach (var policyPermissions in SecurityPolicyCatalog.Policies.Values)
        {
            Assert.True(SecurityAuthorizationRules.HasAccessToAnyPermission(owner, policyPermissions));
        }

        Assert.False(SecurityAuthorizationRules.HasAccessToAnyPermission(
            tenantUser, [SecurityPermissions.InventarioBodegas]));
        Assert.True(SecurityAuthorizationRules.HasAccessToAnyPermission(
            tenantUser, [SecurityPermissions.PosFacturar]));
    }

    [Fact]
    public void LoginEndpointHasDedicatedRateLimitingPolicy()
    {
        var source = File.ReadAllText(GetRepositoryFile("TestDeIa.Api", "Controllers", "SecurityController.cs"));

        Assert.Contains("[HttpPost(\"login\")]", source, StringComparison.Ordinal);
        Assert.Contains($"[EnableRateLimiting(SecurityRateLimitPolicyNames.{nameof(SecurityRateLimitPolicyNames.Login)})]", source, StringComparison.Ordinal);
    }

    [Fact]
    public void JwtSecretKeyRequiresAtLeast256BitsAndCanUseCanonicalSetting()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:Jwt:SecretKey"] = new string('k', 32)
            })
            .Build();

        Assert.Single(JwtSigningKeyResolver.ResolveSigningKeys(configuration));
    }

    [Fact]
    public void JwtSigningFailsWhenSecretIsMissingOrWeak()
    {
        var missing = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var weak = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SecretKey"] = new string('x', 31) })
            .Build();

        Assert.Throws<InvalidOperationException>(() => JwtSigningKeyResolver.ResolveSigningKeys(missing));
        Assert.Throws<InvalidOperationException>(() => JwtSigningKeyResolver.ResolveSigningKeys(weak));
    }

    [Fact]
    public void DevelopmentCorsAllowsLoopbackOriginsOnly()
    {
        Assert.True(CorsPolicyConfiguration.IsLoopbackOrigin("http://localhost:5159"));
        Assert.True(CorsPolicyConfiguration.IsLoopbackOrigin("https://127.0.0.1:7031"));
        Assert.False(CorsPolicyConfiguration.IsLoopbackOrigin("https://easy-market.example"));
        Assert.False(CorsPolicyConfiguration.IsLoopbackOrigin("https://localhost.example"));
    }

    [Fact]
    public void ProductionCorsRequiresExplicitHttpsOriginsWithoutWildcards()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://app.easymarket.example"
            })
            .Build();

        Assert.Equal("https://app.easymarket.example", Assert.Single(CorsPolicyConfiguration.GetProductionOrigins(configuration)));

        var wildcard = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cors:AllowedOrigins:0"] = "https://*.example" })
            .Build();
        var http = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cors:AllowedOrigins:0"] = "http://app.example" })
            .Build();
        var missing = new ConfigurationBuilder().AddInMemoryCollection().Build();

        Assert.Throws<InvalidOperationException>(() => CorsPolicyConfiguration.GetProductionOrigins(wildcard));
        Assert.Throws<InvalidOperationException>(() => CorsPolicyConfiguration.GetProductionOrigins(http));
        Assert.Throws<InvalidOperationException>(() => CorsPolicyConfiguration.GetProductionOrigins(missing));
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

