using TestDeIa.Application.Modules.Security.Models;

namespace TestDeIa.Application.Modules.Security.Ports.Out;

public interface ISecurityTokenGenerator
{
    GeneratedToken Generate(AuthenticatedUser user);
}
