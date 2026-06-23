using System.Security.Cryptography;
using System.Text;
using TestDeIa.Application.Modules.Security.Ports.Out;

namespace TestDeIa.Infrastructure.Adapters.Out.Security;

public sealed class Sha256PasswordHashService : IPasswordHashService
{
    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string password, string passwordHash)
    {
        var hash = Hash(password);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(passwordHash));
    }
}
