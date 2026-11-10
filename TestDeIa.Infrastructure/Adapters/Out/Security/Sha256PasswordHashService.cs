using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Infrastructure.Persistence.Entities;
using PasswordVerificationModel = TestDeIa.Application.Modules.Security.Models.PasswordVerificationResult;

namespace TestDeIa.Infrastructure.Adapters.Out.Security;

public sealed class Sha256PasswordHashService : IPasswordHashService
{
    private readonly IPasswordHasher<SecurityUserEntity> passwordHasher;

    public Sha256PasswordHashService(IPasswordHasher<SecurityUserEntity>? passwordHasher = null)
    {
        this.passwordHasher = passwordHasher ?? new PasswordHasher<SecurityUserEntity>();
    }

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return passwordHasher.HashPassword(new SecurityUserEntity(), password);
    }

    public PasswordVerificationModel Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return PasswordVerificationModel.Failed;
        }

        var identityResult = passwordHasher.VerifyHashedPassword(new SecurityUserEntity(), passwordHash, password);
        if (identityResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
        {
            return new PasswordVerificationModel(true, false);
        }

        if (identityResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded)
        {
            return new PasswordVerificationModel(true, true);
        }

        var legacyMatches = VerifyLegacySha256(password, passwordHash);

        return legacyMatches
            ? new PasswordVerificationModel(true, true)
            : PasswordVerificationModel.Failed;
    }

    private static bool VerifyLegacySha256(string password, string passwordHash)
    {
        if (passwordHash.Length != 64)
        {
            return false;
        }

        try
        {
            var storedHash = Convert.FromHexString(passwordHash);
            var computedHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
