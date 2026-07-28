using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Clientes.Ports.In;
using TestDeIa.Application.Modules.Clientes.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Clientes.Entities;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Responses.Clientes;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Application.Modules.Clientes.UseCases;

public sealed class ClienteUseCase : IClienteUseCase
{
    private readonly IClienteRepository clienteRepository;
    private readonly IPersonaRepository personaRepository;
    private readonly ICatalogoRepository catalogoRepository;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public ClienteUseCase(
        IClienteRepository clienteRepository,
        IPersonaRepository personaRepository,
        ICatalogoRepository catalogoRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.clienteRepository = clienteRepository;
        this.personaRepository = personaRepository;
        this.catalogoRepository = catalogoRepository;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyCollection<ClienteResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await clienteRepository.GetAllAsync(cancellationToken);
        return clientes.Select(MapToResponse).ToArray();
    }

    public async Task<PagedResultResponse<ClienteResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var page = await clienteRepository.GetPagedAsync(term, skip, take, cancellationToken);
        return new PagedResultResponse<ClienteResponse>
        {
            Items = page.Items.Select(MapToResponse).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<ClienteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await clienteRepository.GetByIdAsync(id, cancellationToken);
        return cliente is null ? null : MapToResponse(cliente);
    }

    public async Task<ClienteResponse> CreateAsync(ClienteRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        var tipoIdentificacion = SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion)
            ?? throw new InvalidOperationException("El tipo de identificacion del cliente no coincide con los tipos soportados por facturacion electronica.");
        EcuadorIdentificationValidator.EnsureValid(tipoIdentificacion, request.Identificacion, "el cliente");

        var persona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (persona is not null)
        {
            var existingCliente = await clienteRepository.GetByPersonaIdAsync(persona.Id, cancellationToken: cancellationToken);
            if (existingCliente is not null)
            {
                throw new InvalidOperationException("La persona ya tiene el rol de cliente. Puedes editarla desde la lista.");
            }

            persona = await personaRepository.UpdateAsync(BuildPersona(persona.Id, persona.CreatedAt, request, persona.EsPersonaJuridica, persona.EsEmpresa), cancellationToken)
                ?? throw new InvalidOperationException("No se pudo actualizar la persona base del cliente.");
        }
        else
        {
            persona = await personaRepository.CreateAsync(BuildPersona(Guid.NewGuid(), DateTimeOffset.UtcNow, request, request.TipoIdentificacion.Trim() == "04", false), cancellationToken);
        }

        var cliente = new Cliente(
            persona.Id,
            persona.Id,
            currentUserAccessor.GetRequiredUserId(),
            persona.TipoIdentificacion,
            persona.Identificacion,
            persona.RazonSocialONombresCompletos,
            persona.NombreComercial,
            persona.DireccionPrincipal,
            persona.CorreoElectronicoPrincipal,
            persona.TelefonoCelular,
            persona.FechaNacimiento,
            persona.Genero,
            NormalizeOptional(request.CorreoFacturacionElectronica),
            request.TipoCliente.Trim(),
            request.ObligadoContabilidad,
            request.EsContribuyenteEspecial,
            request.PermiteCredito,
            request.LimiteCredito,
            request.DiasCreditoMaximo,
            request.EstadoCredito.Trim(),
            persona.RolesPersona.Concat(["Cliente"]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            request.IsActive,
            DateTimeOffset.UtcNow,
            currentUserAccessor.GetRequiredUserId(),
            null,
            persona.RegionCodigo,
            persona.ProvinciaCodigo,
            persona.CiudadCodigo,
            persona.SectorCodigo);

        return MapToResponse(await clienteRepository.CreateAsync(cliente, cancellationToken));
    }

    public async Task<ClienteResponse?> UpdateAsync(Guid id, ClienteRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        var tipoIdentificacion = SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion)
            ?? throw new InvalidOperationException("El tipo de identificacion del cliente no coincide con los tipos soportados por facturacion electronica.");
        EcuadorIdentificationValidator.EnsureValid(tipoIdentificacion, request.Identificacion, "el cliente");

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

        var updatedPersona = await personaRepository.UpdateAsync(BuildPersona(currentPersona.Id, currentPersona.CreatedAt, request, currentPersona.EsPersonaJuridica, currentPersona.EsEmpresa), cancellationToken)
            ?? throw new InvalidOperationException("No se pudo actualizar la persona del cliente.");

        var cliente = new Cliente(
            id,
            current.PersonaId,
            current.EmpresaId,
            updatedPersona.TipoIdentificacion,
            updatedPersona.Identificacion,
            updatedPersona.RazonSocialONombresCompletos,
            updatedPersona.NombreComercial,
            updatedPersona.DireccionPrincipal,
            updatedPersona.CorreoElectronicoPrincipal,
            updatedPersona.TelefonoCelular,
            updatedPersona.FechaNacimiento,
            updatedPersona.Genero,
            NormalizeOptional(request.CorreoFacturacionElectronica),
            request.TipoCliente.Trim(),
            request.ObligadoContabilidad,
            request.EsContribuyenteEspecial,
            request.PermiteCredito,
            request.LimiteCredito,
            request.DiasCreditoMaximo,
            request.EstadoCredito.Trim(),
            updatedPersona.RolesPersona,
            request.IsActive,
            current.CreatedAt,
            current.UsuarioCreacionId,
            DateTimeOffset.UtcNow,
            updatedPersona.RegionCodigo,
            updatedPersona.ProvinciaCodigo,
            updatedPersona.CiudadCodigo,
            updatedPersona.SectorCodigo);

        var updated = await clienteRepository.UpdateAsync(cliente, cancellationToken);
        return updated is null ? null : MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return clienteRepository.DeleteAsync(id, cancellationToken);
    }

    private static Persona BuildPersona(Guid id, DateTimeOffset createdAt, ClienteRequest request, bool esPersonaJuridica, bool esEmpresa)
    {
        return new Persona(
            id,
            SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion) ?? request.TipoIdentificacion.Trim(),
            request.Identificacion.Trim(),
            request.RazonSocialONombresCompletos.Trim(),
            NormalizeOptional(request.NombreComercial),
            request.DireccionPrincipal.Trim(),
            NormalizeOptional(request.TelefonoCelular),
            NormalizeOptional(request.CorreoElectronicoPrincipal),
            request.FechaNacimiento,
            NormalizeOptional(request.Genero),
            esPersonaJuridica,
            esEmpresa,
            [],
            request.IsActive,
            createdAt,
            DateTimeOffset.UtcNow,
            NormalizeOptional(request.RegionCodigo),
            NormalizeOptional(request.ProvinciaCodigo),
            NormalizeOptional(request.CiudadCodigo),
            NormalizeOptional(request.SectorCodigo));
    }

    private static ClienteResponse MapToResponse(Cliente cliente)
    {
        return new ClienteResponse
        {
            Id = cliente.Id,
            PersonaId = cliente.PersonaId,
            TipoIdentificacion = cliente.TipoIdentificacion,
            Identificacion = cliente.Identificacion,
            RazonSocialONombresCompletos = cliente.RazonSocialONombresCompletos,
            NombreComercial = cliente.NombreComercial,
            DireccionPrincipal = cliente.DireccionPrincipal,
            RegionCodigo = cliente.RegionCodigo,
            ProvinciaCodigo = cliente.ProvinciaCodigo,
            CiudadCodigo = cliente.CiudadCodigo,
            SectorCodigo = cliente.SectorCodigo,
            CorreoElectronicoPrincipal = cliente.CorreoElectronicoPrincipal,
            TelefonoCelular = cliente.TelefonoCelular,
            FechaNacimiento = cliente.FechaNacimiento,
            Genero = cliente.Genero,
            CorreoFacturacionElectronica = cliente.CorreoFacturacionElectronica,
            TipoCliente = cliente.TipoCliente,
            ObligadoContabilidad = cliente.ObligadoContabilidad,
            EsContribuyenteEspecial = cliente.EsContribuyenteEspecial,
            PermiteCredito = cliente.PermiteCredito,
            LimiteCredito = cliente.LimiteCredito,
            DiasCreditoMaximo = cliente.DiasCreditoMaximo,
            EstadoCredito = cliente.EstadoCredito,
            RolesPersona = cliente.RolesPersona,
            IsActive = cliente.IsActive,
            CreatedAt = cliente.CreatedAt,
            UsuarioCreacionId = cliente.UsuarioCreacionId,
            UpdatedAt = cliente.UpdatedAt,
            UsuarioModificacionId = cliente.UsuarioModificacionId
        };
    }

    private async Task ValidateCatalogValuesAsync(ClienteRequest request, CancellationToken cancellationToken)
    {
        var tipoIdentificacion = SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion);
        if (tipoIdentificacion is null ||
            !await catalogoRepository.ExistsActiveItemAsync("TIPO_IDENTIFICACION", tipoIdentificacion, cancellationToken))
        {
            throw new InvalidOperationException("El tipo de identificacion del cliente no coincide con los tipos soportados por facturacion electronica.");
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
