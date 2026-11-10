using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TestDeIa.Infrastructure.Adapters.Out.Security;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Tests.Security;

public sealed class LegacyPasswordMigrationTests
{
    private const string Password = "Correct-Horse-Battery-42!";

    [Fact]
    public void NewHashesUseTheNativeIdentityPasswordHasher()
    {
        IPasswordHasher<SecurityUserEntity> identityHasher = new PasswordHasher<SecurityUserEntity>();
        var service = new Sha256PasswordHashService(identityHasher);

        var hash = service.Hash(Password);

        Assert.True(identityHasher.VerifyHashedPassword(new SecurityUserEntity(), hash, Password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded);
        Assert.True(service.Verify(Password, hash).Succeeded);
        Assert.False(service.Verify(Password, hash).RequiresRehash);
    }

    [Fact]
    public void ValidLegacySha256PasswordIsAcceptedAndMarkedForRehash()
    {
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Password))).ToLowerInvariant();
        var service = new Sha256PasswordHashService();

        var verification = service.Verify(Password, legacyHash);

        Assert.True(verification.Succeeded);
        Assert.True(verification.RequiresRehash);

        var migratedHash = service.Hash(Password);
        var migratedVerification = service.Verify(Password, migratedHash);
        Assert.True(migratedVerification.Succeeded);
        Assert.False(migratedVerification.RequiresRehash);
        Assert.NotEqual(legacyHash, migratedHash);
    }

    [Fact]
    public void InvalidLegacyHashIsRejected()
    {
        var service = new Sha256PasswordHashService();

        Assert.False(service.Verify(Password, new string('Z', 64)).Succeeded);
        Assert.False(service.Verify("wrong-password", Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Password)))).Succeeded);
    }
}
