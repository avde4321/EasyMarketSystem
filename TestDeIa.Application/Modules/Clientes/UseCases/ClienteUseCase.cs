using TestDeIa.Application.Modules.Clientes.Ports.In;
using TestDeIa.Application.Modules.Clientes.Ports.Out;
using TestDeIa.Domain.Modules.Clientes.Entities;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Clientes;

namespace TestDeIa.Application.Modules.Clientes.UseCases;

public sealed class ClienteUseCase : IClienteUseCase
{
    private readonly IClienteRepository clienteRepository;

    public ClienteUseCase(IClienteRepository clienteRepository)
    {
        this.clienteRepository = clienteRepository;
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
        ValidateTipoIdentificacion(request.TipoIdentificacion);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        if (await clienteRepository.ExistsByIdentificacionAsync(request.Identificacion, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un cliente con esa identificacion.");
        }

        var cliente = new Cliente(
            Guid.NewGuid(),
            Guid.NewGuid(),
            request.TipoIdentificacion.Trim(),
            request.Identificacion.Trim(),
            request.Nombres.Trim(),
            request.Apellidos.Trim(),
            NormalizeOptional(request.Email),
            NormalizeOptional(request.Telefono),
            NormalizeOptional(request.Direccion),
            request.IsActive,
            DateTimeOffset.UtcNow,
            null);

        return MapToResponse(await clienteRepository.CreateAsync(cliente, cancellationToken));
    }

    public async Task<ClienteResponse?> UpdateAsync(Guid id, ClienteRequest request, CancellationToken cancellationToken = default)
    {
        ValidateTipoIdentificacion(request.TipoIdentificacion);
        ValidateIdentificationByType(request.TipoIdentificacion, request.Identificacion);

        if (await clienteRepository.ExistsByIdentificacionAsync(request.Identificacion, id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro cliente con esa identificacion.");
        }

        var current = await clienteRepository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        if (await clienteRepository.ExistsPersonaByIdentificacionAsync(request.Identificacion, current.PersonaId, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otra persona con esa identificacion.");
        }

        var cliente = new Cliente(
            id,
            current.PersonaId,
            request.TipoIdentificacion.Trim(),
            request.Identificacion.Trim(),
            request.Nombres.Trim(),
            request.Apellidos.Trim(),
            NormalizeOptional(request.Email),
            NormalizeOptional(request.Telefono),
            NormalizeOptional(request.Direccion),
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
            IsActive = cliente.IsActive,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt
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
            throw new InvalidOperationException("El tipo de identificacion del cliente no coincide con los tipos soportados por facturacion electronica.");
        }
    }

    private static void ValidateIdentificationByType(string tipoIdentificacion, string identificacion)
    {
        var normalizedType = tipoIdentificacion.Trim().ToUpperInvariant();
        var normalizedIdentification = identificacion.Trim();

        if (string.IsNullOrWhiteSpace(normalizedIdentification))
        {
            throw new InvalidOperationException("La identificacion del cliente es obligatoria.");
        }

        if (normalizedType is "CEDULA" or "CÉDULA" && (normalizedIdentification.Length != 10 || !normalizedIdentification.All(char.IsDigit)))
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
