using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Application.Modules.Security.Ports.In;

public interface ILoginUseCase
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
