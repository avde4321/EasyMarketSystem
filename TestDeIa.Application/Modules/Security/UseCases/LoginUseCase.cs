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
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByUserNameAsync(request.UserName, cancellationToken);

        if (user is null)
        {
            await userRepository.RecordFailedLoginAsync(request.UserName, ipAddress, cancellationToken);

            return new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = "Usuario o contrasena incorrectos."
            };
        }

        if (!user.IsActive)
        {
            return new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = "La cuenta se encuentra inactiva. Contacta al administrador."
            };
        }

        if (user.BloqueadoManualmente)
        {
            return new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = "La cuenta se encuentra bloqueada por un administrador."
            };
        }

        if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTimeOffset.UtcNow)
        {
            return new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = $"Usuario bloqueado temporalmente hasta {user.BloqueadoHasta.Value.LocalDateTime:dd/MM/yyyy HH:mm}."
            };
        }

        var verification = passwordHashService.Verify(request.Password, user.PasswordHash);
        if (!verification.Succeeded)
        {
            var blocked = await userRepository.RecordFailedLoginAsync(request.UserName, ipAddress, cancellationToken);

            return new LoginResponse
            {
                Succeeded = false,
                ErrorMessage = blocked
                    ? "Usuario bloqueado por 15 minutos tras 5 intentos fallidos."
                    : "Usuario o contrasena incorrectos."
            };
        }

        var replacementHash = verification.RequiresRehash
            ? passwordHashService.Hash(request.Password)
            : null;

        await userRepository.RecordSuccessfulLoginAsync(user.Id, ipAddress, replacementHash, cancellationToken);

        var authenticatedUser = new AuthenticatedUser(
            user.Id,
            user.UserName,
            user.DisplayName,
            user.Identification,
            user.Email,
            user.EmpresasAcceso.FirstOrDefault(current => current.IsDefault)?.EmpresaId ?? user.EmpresasAcceso.FirstOrDefault()?.EmpresaId,
            user.Roles,
            user.Permissions);

        var token = tokenGenerator.Generate(authenticatedUser);

        return new LoginResponse
        {
            Succeeded = true,
            Token = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Roles = user.Roles,
            Permissions = user.Permissions,
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
