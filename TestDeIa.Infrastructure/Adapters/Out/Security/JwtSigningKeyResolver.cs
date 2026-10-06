using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TestDeIa.Infrastructure.Adapters.Out.Security;

public sealed record JwtSigningKey(string KeyId, SymmetricSecurityKey SecurityKey);

public static class JwtSigningKeyResolver
{
    private const int MinimumSecretBytes = 32;

    public static IReadOnlyCollection<JwtSigningKey> ResolveSigningKeys(IConfiguration configuration)
    {
        var configuredKeys = configuration
            .GetSection("Security:Jwt:SigningKeys")
            .GetChildren()
            .Select(section =>
            {
                var keyId = section["KeyId"]?.Trim();
                var secret = section["Secret"];
                var isActive = !bool.TryParse(section["IsActive"], out var parsedIsActive) || parsedIsActive;

                return new
                {
                    KeyId = string.IsNullOrWhiteSpace(keyId) ? section.Key : keyId,
                    Secret = secret,
                    IsActive = isActive
                };
            })
            .Where(current => current.IsActive && !string.IsNullOrWhiteSpace(current.Secret))
            .Select(current => CreateSigningKey(current.KeyId, current.Secret!))
            .ToArray();

        if (configuredKeys.Length > 0)
        {
            return configuredKeys;
        }

        var legacySecret = configuration["Security:Jwt:Secret"];
        if (!string.IsNullOrWhiteSpace(legacySecret))
        {
            return [CreateSigningKey("legacy", legacySecret)];
        }

        throw new InvalidOperationException(
            "Configure al menos una clave JWT en Security:Jwt:SigningKeys:0:Secret usando User Secrets, variables de entorno o Key Vault.");
    }

    public static JwtSigningKey ResolvePrimarySigningKey(IConfiguration configuration)
    {
        return ResolveSigningKeys(configuration).First();
    }

    private static JwtSigningKey CreateSigningKey(string keyId, string secret)
    {
        if (Encoding.UTF8.GetByteCount(secret) < MinimumSecretBytes)
        {
            throw new InvalidOperationException("Cada clave JWT debe tener al menos 32 bytes.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        {
            KeyId = keyId
        };

        return new JwtSigningKey(keyId, securityKey);
    }
}
