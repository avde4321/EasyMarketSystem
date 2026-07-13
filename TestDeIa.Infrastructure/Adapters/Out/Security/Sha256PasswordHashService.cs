using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TestDeIa.Application.Modules.Security.Ports.Out;
using PasswordVerificationModel = TestDeIa.Application.Modules.Security.Models.PasswordVerificationResult;

namespace TestDeIa.Infrastructure.Adapters.Out.Security;

public sealed class Sha256PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<string> passwordHasher = new();

    public string Hash(string password)
    {
        return passwordHasher.HashPassword(string.Empty, password);
    }

    public PasswordVerificationModel Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return PasswordVerificationModel.Failed;
        }

        var identityResult = passwordHasher.VerifyHashedPassword(string.Empty, passwordHash, password);
        if (identityResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
        {
            return new PasswordVerificationModel(true, false);
        }

        if (identityResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded)
        {
            return new PasswordVerificationModel(true, true);
        }

        var legacyHash = ComputeLegacySha256(password);
        var legacyMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(legacyHash),
            Encoding.UTF8.GetBytes(passwordHash));

        return legacyMatches
            ? new PasswordVerificationModel(true, true)
            : PasswordVerificationModel.Failed;
    }

    private static string ComputeLegacySha256(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
