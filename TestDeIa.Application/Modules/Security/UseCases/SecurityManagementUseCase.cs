using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.In;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Domain.Modules.Security.Entities;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Security;

namespace TestDeIa.Application.Modules.Security.UseCases;

public sealed class SecurityManagementUseCase : ISecurityManagementUseCase
{
    private readonly ISecurityUserRepository securityUserRepository;
    private readonly IPersonaRepository personaRepository;
    private readonly IPasswordHashService passwordHashService;
    private readonly ICatalogoRepository catalogoRepository;

    public SecurityManagementUseCase(
        ISecurityUserRepository securityUserRepository,
        IPersonaRepository personaRepository,
        IPasswordHashService passwordHashService,
        ICatalogoRepository catalogoRepository)
    {
        this.securityUserRepository = securityUserRepository;
        this.personaRepository = personaRepository;
        this.passwordHashService = passwordHashService;
        this.catalogoRepository = catalogoRepository;
    }

    public async Task<IReadOnlyCollection<SecurityUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await securityUserRepository.GetAllAsync(cancellationToken);
        var personas = await personaRepository.GetAllAsync(cancellationToken);
        return users.Select(user => MapUser(user, personas)).ToArray();
    }

    public async Task<IReadOnlyCollection<SecurityRoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await securityUserRepository.GetRolesAsync(cancellationToken);
        return roles
            .Where(role => role.IsActive)
            .OrderBy(role => role.Name)
            .Select(role => new SecurityRoleResponse { Name = role.Name })
            .ToArray();
    }

    public async Task<SecurityUserResponse> CreateUserAsync(SecurityUserRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);
        ValidateRequest(request, true);

        var persona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (persona is not null)
        {
            var existingUser = await securityUserRepository.GetByPersonaIdAsync(persona.Id, cancellationToken: cancellationToken);
            if (existingUser is not null)
            {
                throw new InvalidOperationException("La persona ya tiene un usuario asignado. Puedes editarlo desde la lista.");
            }

            persona = await personaRepository.UpdateAsync(BuildPersona(persona.Id, persona.CreatedAt, request), cancellationToken)
                ?? throw new InvalidOperationException("No se pudo actualizar la persona base del usuario.");
        }
        else
        {
            persona = await personaRepository.CreateAsync(BuildPersona(Guid.NewGuid(), DateTimeOffset.UtcNow, request), cancellationToken);
        }

        if (await securityUserRepository.ExistsByUserNameAsync(request.UserName.Trim(), cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un usuario con ese nombre.");
        }

        await ValidateRolesAsync(request.Roles, cancellationToken);

        var user = new SecurityUser(
            Guid.NewGuid(),
            persona.Id,
            request.UserName.Trim(),
            $"{persona.Nombres} {persona.Apellidos}".Trim(),
            persona.Email!.Trim(),
            passwordHashService.Hash(request.Password!.Trim()),
            request.Roles.Select(role => role.Trim()).ToArray(),
            persona.RolesPersona.Concat(["Usuario"]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            request.IsActive,
            DateTimeOffset.UtcNow);

        var created = await securityUserRepository.CreateAsync(user, cancellationToken);
        return MapUser(created, [persona]);
    }

    public async Task<SecurityUserResponse?> UpdateUserAsync(Guid id, SecurityUserRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);
        ValidateRequest(request, false);

        var current = await securityUserRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        if (await securityUserRepository.ExistsByUserNameAsync(request.UserName.Trim(), id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un usuario con ese nombre.");
        }

        var existingPersona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (existingPersona is not null && existingPersona.Id != current.PersonaId)
        {
            throw new InvalidOperationException("La identificacion pertenece a otra persona. Usa esa persona para agregar el rol de usuario.");
        }

        var currentPersona = await personaRepository.GetByIdAsync(current.PersonaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la persona asociada al usuario.");

        var updatedPersona = await personaRepository.UpdateAsync(BuildPersona(currentPersona.Id, currentPersona.CreatedAt, request), cancellationToken)
            ?? throw new InvalidOperationException("No se pudo actualizar la persona del usuario.");

        await ValidateRolesAsync(request.Roles, cancellationToken);

        var passwordHash = string.IsNullOrWhiteSpace(request.Password)
            ? current.PasswordHash
            : passwordHashService.Hash(request.Password.Trim());

        var user = new SecurityUser(
            current.Id,
            current.PersonaId,
            request.UserName.Trim(),
            $"{updatedPersona.Nombres} {updatedPersona.Apellidos}".Trim(),
            updatedPersona.Email!.Trim(),
            passwordHash,
            request.Roles.Select(role => role.Trim()).ToArray(),
            updatedPersona.RolesPersona,
            request.IsActive,
            current.CreatedAt);

        var updated = await securityUserRepository.UpdateAsync(user, cancellationToken);
        return updated is null ? null : MapUser(updated, [updatedPersona]);
    }

    private async Task ValidateCatalogValuesAsync(SecurityUserRequest request, CancellationToken cancellationToken)
    {
        if (!await catalogoRepository.ExistsActiveItemAsync("TIPO_IDENTIFICACION", request.TipoIdentificacion.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("El tipo de identificacion del usuario no coincide con el catalogo parametrizado.");
        }
    }

    private async Task ValidateRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken)
    {
        var activeRoleNames = (await securityUserRepository.GetRolesAsync(cancellationToken))
            .Where(role => role.IsActive)
            .Select(role => role.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roleNames.Count == 0 || roleNames.Any(role => !activeRoleNames.Contains(role.Trim())))
        {
            throw new InvalidOperationException("Uno o varios roles seleccionados no estan disponibles.");
        }
    }

    private static Persona BuildPersona(Guid id, DateTimeOffset createdAt, SecurityUserRequest request)
    {
        return new Persona(
            id,
            request.TipoIdentificacion.Trim(),
            request.Identificacion.Trim(),
            request.Nombres.Trim(),
            request.Apellidos.Trim(),
            null,
            null,
            NormalizeOptional(request.Email),
            NormalizeOptional(request.Telefono),
            NormalizeOptional(request.Direccion),
            [],
            request.IsActive,
            createdAt,
            DateTimeOffset.UtcNow);
    }

    private static void ValidateRequest(SecurityUserRequest request, bool requirePassword)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new InvalidOperationException("El usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("El correo es obligatorio para el usuario.");
        }

        if (requirePassword && string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("La clave es obligatoria para crear el usuario.");
        }

        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Trim().Length < 6)
        {
            throw new InvalidOperationException("La clave debe tener al menos 6 caracteres.");
        }
    }

    private static SecurityUserResponse MapUser(SecurityUser user, IReadOnlyCollection<Persona> personas)
    {
        var persona = personas.FirstOrDefault(current => current.Id == user.PersonaId);

        return new SecurityUserResponse
        {
            Id = user.Id,
            PersonaId = user.PersonaId,
            PersonaIdentificacion = persona?.Identificacion ?? user.UserName,
            PersonaNombre = persona is null ? user.DisplayName : $"{persona.Nombres} {persona.Apellidos}".Trim(),
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Roles = user.Roles,
            RolesPersona = user.RolesPersona,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateIdentificationByType(string tipoIdentificacion, string identificacion)
    {
        var normalizedType = tipoIdentificacion.Trim().ToUpperInvariant();
        var normalizedIdentification = identificacion.Trim();

        if (string.IsNullOrWhiteSpace(normalizedIdentification))
        {
            throw new InvalidOperationException("La identificacion de la persona es obligatoria.");
        }

        if (normalizedType == "CEDULA" && (normalizedIdentification.Length != 10 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("La cedula debe tener 10 digitos numericos.");
        }

        if (normalizedType == "RUC" && (normalizedIdentification.Length != 13 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("El RUC debe tener 13 digitos numericos.");
        }
    }
}
