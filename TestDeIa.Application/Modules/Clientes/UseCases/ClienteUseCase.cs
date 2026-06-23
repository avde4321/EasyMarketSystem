using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Clientes.Ports.In;
using TestDeIa.Application.Modules.Clientes.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Domain.Modules.Clientes.Entities;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Clientes;

namespace TestDeIa.Application.Modules.Clientes.UseCases;

public sealed class ClienteUseCase : IClienteUseCase
{
    private readonly IClienteRepository clienteRepository;
    private readonly IPersonaRepository personaRepository;
    private readonly ICatalogoRepository catalogoRepository;

    public ClienteUseCase(
        IClienteRepository clienteRepository,
        IPersonaRepository personaRepository,
        ICatalogoRepository catalogoRepository)
    {
        this.clienteRepository = clienteRepository;
        this.personaRepository = personaRepository;
        this.catalogoRepository = catalogoRepository;
    }

    public async Task<IReadOnlyCollection<ClienteResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await clienteRepository.GetAllAsync(cancellationToken);
        return clientes.Select(MapToResponse).ToArray();
    }

    public async Task<ClienteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await clienteRepository.GetByIdAsync(id, cancellationToken);
        return cliente is null ? null : MapToResponse(cliente);
    }

    public async Task<ClienteResponse> CreateAsync(ClienteRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        var persona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (persona is not null)
        {
            var existingCliente = await clienteRepository.GetByPersonaIdAsync(persona.Id, cancellationToken: cancellationToken);
            if (existingCliente is not null)
            {
                throw new InvalidOperationException("La persona ya tiene el rol de cliente. Puedes editarla desde la lista.");
            }

            persona = await personaRepository.UpdateAsync(BuildPersona(persona.Id, persona.CreatedAt, request), cancellationToken)
                ?? throw new InvalidOperationException("No se pudo actualizar la persona base del cliente.");
        }
        else
        {
            persona = await personaRepository.CreateAsync(BuildPersona(Guid.NewGuid(), DateTimeOffset.UtcNow, request), cancellationToken);
        }

        var cliente = new Cliente(
            Guid.NewGuid(),
            persona.Id,
            persona.TipoIdentificacion,
            persona.Identificacion,
            persona.Nombres,
            persona.Apellidos,
            persona.Email,
            persona.Telefono,
            persona.Direccion,
            persona.RolesPersona.Concat(["Cliente"]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapToResponse(await clienteRepository.CreateAsync(cliente, cancellationToken));
    }

    public async Task<ClienteResponse?> UpdateAsync(Guid id, ClienteRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        var current = await clienteRepository.GetByIdAsync(id, cancellationToken);
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
            ?? throw new InvalidOperationException("No se encontro la persona asociada al cliente.");

        var updatedPersona = await personaRepository.UpdateAsync(BuildPersona(currentPersona.Id, currentPersona.CreatedAt, request), cancellationToken)
            ?? throw new InvalidOperationException("No se pudo actualizar la persona del cliente.");

        var cliente = new Cliente(
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

        var updated = await clienteRepository.UpdateAsync(cliente, cancellationToken);
        return updated is null ? null : MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return clienteRepository.DeleteAsync(id, cancellationToken);
    }

    private static Persona BuildPersona(Guid id, DateTimeOffset createdAt, ClienteRequest request)
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

    private static ClienteResponse MapToResponse(Cliente cliente)
    {
        return new ClienteResponse
        {
            Id = cliente.Id,
            PersonaId = cliente.PersonaId,
            TipoIdentificacion = cliente.TipoIdentificacion,
            Identificacion = cliente.Identificacion,
            Nombres = cliente.Nombres,
            Apellidos = cliente.Apellidos,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Direccion = cliente.Direccion,
            RolesPersona = cliente.RolesPersona,
            IsActive = cliente.IsActive,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt
        };
    }

    private async Task ValidateCatalogValuesAsync(ClienteRequest request, CancellationToken cancellationToken)
    {
        if (!await catalogoRepository.ExistsActiveItemAsync("TIPO_IDENTIFICACION", request.TipoIdentificacion.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("El tipo de identificacion del cliente no coincide con los tipos soportados por facturacion electronica.");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateIdentificationByType(string tipoIdentificacion, string identificacion)
    {
        var normalizedType = tipoIdentificacion.Trim().ToUpperInvariant();
        var normalizedIdentification = identificacion.Trim();

        if (string.IsNullOrWhiteSpace(normalizedIdentification))
        {
            throw new InvalidOperationException("La identificacion del cliente es obligatoria.");
        }

        if (normalizedType == "CEDULA" && (normalizedIdentification.Length != 10 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("La cedula del cliente debe tener 10 digitos numericos.");
        }

        if (normalizedType == "RUC" && (normalizedIdentification.Length != 13 || !normalizedIdentification.All(char.IsDigit)))
        {
            throw new InvalidOperationException("El RUC del cliente debe tener 13 digitos numericos.");
        }

        if (normalizedType == "CONSUMIDOR FINAL" && normalizedIdentification != "9999999999999")
        {
            throw new InvalidOperationException("Para consumidor final se debe usar la identificacion 9999999999999.");
        }
    }
}
