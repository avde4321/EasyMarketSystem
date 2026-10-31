using TestDeIa.Application.Modules.Security.Models;

namespace TestDeIa.Application.Modules.Security.Ports.Out;

public interface IPasswordHashService
{
    string Hash(string password);

    PasswordVerificationResult Verify(string password, string passwordHash);
}
