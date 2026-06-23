using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Empleados.Ports.In;
using TestDeIa.Application.Modules.Empleados.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Domain.Modules.Empleados.Entities;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Shared.Requests.Empleados;
using TestDeIa.Shared.Responses.Empleados;

namespace TestDeIa.Application.Modules.Empleados.UseCases;

public sealed class EmpleadoUseCase : IEmpleadoUseCase
{
    private readonly IEmpleadoRepository empleadoRepository;
    private readonly IPersonaRepository personaRepository;
    private readonly ICatalogoRepository catalogoRepository;

    public EmpleadoUseCase(
        IEmpleadoRepository empleadoRepository,
        IPersonaRepository personaRepository,
        ICatalogoRepository catalogoRepository)
    {
        this.empleadoRepository = empleadoRepository;
        this.personaRepository = personaRepository;
        this.catalogoRepository = catalogoRepository;
    }

    public async Task<IReadOnlyCollection<EmpleadoResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var empleados = await empleadoRepository.GetAllAsync(cancellationToken);
        return empleados.Select(MapToResponse).ToArray();
    }

    public async Task<EmpleadoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empleado = await empleadoRepository.GetByIdAsync(id, cancellationToken);
        return empleado is null ? null : MapToResponse(empleado);
    }

    public async Task<EmpleadoResponse> CreateAsync(EmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        var persona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (persona is not null)
        {
            var existingEmpleado = await empleadoRepository.GetByPersonaIdAsync(persona.Id, cancellationToken: cancellationToken);
            if (existingEmpleado is not null)
            {
                throw new InvalidOperationException("La persona ya tiene el rol de empleado. Puedes editarla desde la lista.");
            }

            persona = await personaRepository.UpdateAsync(BuildPersona(persona.Id, persona.CreatedAt, request), cancellationToken)
                ?? throw new InvalidOperationException("No se pudo actualizar la persona base del empleado.");
        }
        else
        {
            persona = await personaRepository.CreateAsync(BuildPersona(Guid.NewGuid(), DateTimeOffset.UtcNow, request), cancellationToken);
        }

        var empleado = new Empleado(
            Guid.NewGuid(),
            persona.Id,
            persona.TipoIdentificacion,
            persona.Identificacion,
            persona.Nombres,
            persona.Apellidos,
            persona.Email,
            persona.Telefono,
            persona.Direccion,
            persona.RolesPersona.Concat(["Empleado"]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapToResponse(await empleadoRepository.CreateAsync(empleado, cancellationToken));
    }

    public async Task<EmpleadoResponse?> UpdateAsync(Guid id, EmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        var current = await empleadoRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var existingPersona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (existingPersona is not null && existingPersona.Id != current.PersonaId)
        {
            throw new InvalidOperationException("La identificacion pertenece a otra persona. Usa esa persona para agregar el rol correspondiente.");
        }

        var currentPersona = await personaRepository.GetByIdAsync(current.PersonaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la persona asociada al empleado.");

        var updatedPersona = await personaRepository.UpdateAsync(BuildPersona(currentPersona.Id, currentPersona.CreatedAt, request), cancellationToken)
            ?? throw new InvalidOperationException("No se pudo actualizar la persona del empleado.");

        var empleado = new Empleado(
            id,
            current.PersonaId,
            updatedPersona.TipoIdentificacion,
            updatedPersona.Identificacion,
            updatedPersona.Nombres,
            updatedPersona.Apellidos,
            updatedPersona.Email,
            updatedPersona.Telefono,
            updatedPersona.Direccion,
            updatedPersona.RolesPersona,
            request.IsActive,
            current.CreatedAt,
            DateTimeOffset.UtcNow);

        var updated = await empleadoRepository.UpdateAsync(empleado, cancellationToken);
        return updated is null ? null : MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return empleadoRepository.DeleteAsync(id, cancellationToken);
    }

    private static Persona BuildPersona(Guid id, DateTimeOffset createdAt, EmpleadoRequest request)
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

    private static EmpleadoResponse MapToResponse(Empleado empleado)
    {
        return new EmpleadoResponse
        {
            Id = empleado.Id,
            PersonaId = empleado.PersonaId,
            TipoIdentificacion = empleado.TipoIdentificacion,
            Identificacion = empleado.Identificacion,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            Email = empleado.Email,
            Telefono = empleado.Telefono,
            Direccion = empleado.Direccion,
            RolesPersona = empleado.RolesPersona,
            IsActive = empleado.IsActive,
            CreatedAt = empleado.CreatedAt,
            UpdatedAt = empleado.UpdatedAt
        };
    }

    private async Task ValidateCatalogValuesAsync(EmpleadoRequest request, CancellationToken cancellationToken)
    {
        if (!await catalogoRepository.ExistsActiveItemAsync("TIPO_IDENTIFICACION", request.TipoIdentificacion.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("El tipo de identificacion del empleado no coincide con los tipos soportados por facturacion electronica.");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateIdentificationByType(string tipoIdentificacion, string identificacion)
    {
        var normalizedType = tipoIdentificacion.Trim().ToUpperInvariant();
        var normalizedIdentification = identificacion.Trim();

        if (string.IsNullOrWhiteSpace(normalizedIdentification))
        {
            throw new InvalidOperationException("La identificacion del empleado es obligatoria.");
        }

        if (normalizedType == "CEDULA" && (normalizedIdentification.Length != 10 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("La cedula del empleado debe tener 10 digitos numericos.");
        }

        if (normalizedType == "RUC" && (normalizedIdentification.Length != 13 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("El RUC del empleado debe tener 13 digitos numericos.");
        }
    }
}
