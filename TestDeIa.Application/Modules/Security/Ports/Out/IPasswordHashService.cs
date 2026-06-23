namespace TestDeIa.Application.Modules.Security.Ports.Out;

public interface IPasswordHashService
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
