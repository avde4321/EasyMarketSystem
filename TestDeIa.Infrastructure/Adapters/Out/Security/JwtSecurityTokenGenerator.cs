using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TestDeIa.Application.Modules.Security.Models;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Adapters.Out.Security;

public sealed class JwtSecurityTokenGenerator : ISecurityTokenGenerator
{
    private readonly IConfiguration configuration;

    public JwtSecurityTokenGenerator(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public GeneratedToken Generate(AuthenticatedUser user)
    {
        var issuer = configuration["Security:Jwt:Issuer"] ?? "TestDeIa";
        var audience = configuration["Security:Jwt:Audience"] ?? "TestDeIa.Client";
        var secret = configuration["Security:Jwt:Secret"]
            ?? "TestDeIa_Crm_Development_Secret_Key_Change_In_Production";
        var expirationMinutes = int.TryParse(
            configuration["Security:Jwt:ExpirationMinutes"],
            out var configuredMinutes)
            ? configuredMinutes
            : 60;

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        var issuedAt = DateTimeOffset.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email)
        };

        if (user.DefaultEmpresaId.HasValue)
        {
            claims.Add(new Claim("default_empresa_id", user.DefaultEmpresaId.Value.ToString()));
        }

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(permission => new Claim(SecurityClaimTypes.Permission, permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new GeneratedToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
