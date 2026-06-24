using TestDeIa.Application.Modules.Security.Models;
using TestDeIa.Application.Modules.Security.Ports.In;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Empresa;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Application.Modules.Security.UseCases;

public sealed class LoginUseCase : ILoginUseCase
{
    private readonly ISecurityUserRepository userRepository;
    private readonly IPasswordHashService passwordHashService;
    private readonly ISecurityTokenGenerator tokenGenerator;

    public LoginUseCase(
        ISecurityUserRepository userRepository,
        IPasswordHashService passwordHashService,
        ISecurityTokenGenerator tokenGenerator)
    {
        this.userRepository = userRepository;
        this.passwordHashService = passwordHashService;
        this.tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByUserNameAsync(request.UserName, cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !passwordHashService.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = "Usuario o contrasena incorrectos."
            };
        }

        var authenticatedUser = new AuthenticatedUser(
            user.Id,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.EmpresasAcceso.FirstOrDefault(current => current.IsDefault)?.EmpresaId ?? user.EmpresasAcceso.FirstOrDefault()?.EmpresaId,
            user.Roles);

        var token = tokenGenerator.Generate(authenticatedUser);

        return new LoginResponse
        {
            Succeeded = true,
            Token = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Roles = user.Roles,
            ActiveEmpresaId = authenticatedUser.DefaultEmpresaId,
            Empresas = user.EmpresasAcceso
                .Select(current => new EmpresaOptionResponse
                {
                    Id = current.EmpresaId,
                    RazonSocial = current.RazonSocial,
                    NombreComercial = current.NombreComercial,
                    Ruc = current.Ruc,
                    IsActive = current.IsActive,
                    IsDefault = current.IsDefault
                })
                .ToArray()
        };
    }
}
