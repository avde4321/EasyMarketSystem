using TestDeIa.Application.Modules.Personas.Ports.In;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Shared.Requests.Personas;
using TestDeIa.Shared.Responses.Personas;

namespace TestDeIa.Application.Modules.Personas.UseCases;

public sealed class PersonaUseCase : IPersonaUseCase
{
    private readonly IPersonaRepository personaRepository;

    public PersonaUseCase(IPersonaRepository personaRepository)
    {
        this.personaRepository = personaRepository;
    }

    public async Task<IReadOnlyCollection<PersonaResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personas = await personaRepository.GetAllAsync(cancellationToken);
        return personas.Select(MapToResponse).ToArray();
    }

    public async Task<PersonaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var persona = await personaRepository.GetByIdAsync(id, cancellationToken);
        return persona is null ? null : MapToResponse(persona);
    }

    public async Task<PersonaResponse> CreateAsync(PersonaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateTipoIdentificacion(request.TipoIdentificacion);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        if (await personaRepository.ExistsByIdentificacionAsync(request.Identificacion, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una persona con esa identificacion.");
        }

        var persona = new Persona(
            Guid.NewGuid(),
            request.TipoIdentificacion.Trim(),
            request.Identificacion.Trim(),
            request.Nombres.Trim(),
            request.Apellidos.Trim(),
            request.FechaNacimiento,
            NormalizeOptional(request.Email),
            NormalizeOptional(request.Telefono),
            NormalizeOptional(request.Direccion),
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapToResponse(await personaRepository.CreateAsync(persona, cancellationToken));
    }

    public async Task<PersonaResponse?> UpdateAsync(Guid id, PersonaRequest request, CancellationToken cancellationToken = default)
    {
        ValidateTipoIdentificacion(request.TipoIdentificacion);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        if (await personaRepository.ExistsByIdentificacionAsync(request.Identificacion, id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otra persona con esa identificacion.");
        }

        var current = await personaRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var persona = new Persona(
            id,
            request.TipoIdentificacion.Trim(),
            request.Identificacion.Trim(),
            request.Nombres.Trim(),
            request.Apellidos.Trim(),
            request.FechaNacimiento,
            NormalizeOptional(request.Email),
            NormalizeOptional(request.Telefono),
            NormalizeOptional(request.Direccion),
            request.IsActive,
            current.CreatedAt,
            DateTimeOffset.UtcNow);

        var updated = await personaRepository.UpdateAsync(persona, cancellationToken);
        return updated is null ? null : MapToResponse(updated);
    }

    private static PersonaResponse MapToResponse(Persona persona)
    {
        return new PersonaResponse
        {
            Id = persona.Id,
            TipoIdentificacion = persona.TipoIdentificacion,
            Identificacion = persona.Identificacion,
            Nombres = persona.Nombres,
            Apellidos = persona.Apellidos,
            FechaNacimiento = persona.FechaNacimiento,
            Email = persona.Email,
            Telefono = persona.Telefono,
            Direccion = persona.Direccion,
            IsActive = persona.IsActive,
            CreatedAt = persona.CreatedAt,
            UpdatedAt = persona.UpdatedAt
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateTipoIdentificacion(string tipoIdentificacion)
    {
        var normalized = tipoIdentificacion.Trim().ToUpperInvariant();
        var valid = normalized is
            "RUC" or
            "CEDULA" or
            "CÉDULA" or
            "PASAPORTE" or
            "CONSUMIDOR FINAL" or
            "IDENTIFICACION DEL EXTERIOR" or
            "IDENTIFICACIÓN DEL EXTERIOR" or
            "PLACA";

        if (!valid)
        {
            throw new InvalidOperationException("El tipo de identificacion no coincide con los tipos soportados por facturacion electronica.");
        }
    }

    private static void ValidateIdentificationByType(string tipoIdentificacion, string identificacion)
    {
        var normalizedType = tipoIdentificacion.Trim().ToUpperInvariant();
        var normalizedIdentification = identificacion.Trim();

        if (string.IsNullOrWhiteSpace(normalizedIdentification))
        {
            throw new InvalidOperationException("La identificacion de la persona es obligatoria.");
        }

        if (normalizedType is "CEDULA" or "CÉDULA" && (normalizedIdentification.Length != 10 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("La cedula de la persona debe tener 10 digitos numericos.");
        }

        if (normalizedType == "RUC" && (normalizedIdentification.Length != 13 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("El RUC de la persona debe tener 13 digitos numericos.");
        }

        if (normalizedType == "CONSUMIDOR FINAL" && normalizedIdentification != "9999999999999")
        {
            throw new InvalidOperationException("Para consumidor final se debe usar la identificacion 9999999999999.");
        }
    }
}
