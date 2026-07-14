using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Application.Modules.Compras.UseCases;

public sealed class ProveedorUseCase : IProveedorUseCase
{
    private static readonly HashSet<string> AllowedTipoIdentificacion = ["04", "06"];

    private readonly IProveedorRepository proveedorRepository;
    private readonly IPersonaRepository personaRepository;
    private readonly ICatalogoRepository catalogoRepository;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public ProveedorUseCase(
        IProveedorRepository proveedorRepository,
        IPersonaRepository personaRepository,
        ICatalogoRepository catalogoRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.proveedorRepository = proveedorRepository;
        this.personaRepository = personaRepository;
        this.catalogoRepository = catalogoRepository;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyCollection<ProveedorResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var proveedores = await proveedorRepository.GetAllAsync(cancellationToken);
        return proveedores.Select(MapToResponse).ToArray();
    }

    public async Task<PagedResultResponse<ProveedorResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var page = await proveedorRepository.GetPagedAsync(term, skip, take, cancellationToken);
        return new PagedResultResponse<ProveedorResponse>
        {
            Items = page.Items.Select(MapToResponse).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<ProveedorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proveedor = await proveedorRepository.GetByIdAsync(id, cancellationToken);
        return proveedor is null ? null : MapToResponse(proveedor);
    }

    public async Task<ProveedorResponse> CreateAsync(ProveedorRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        var tipoIdentificacion = GetValidatedTipoIdentificacion(request.TipoIdentificacion);
        EnsureProveedorIdentification(tipoIdentificacion, request.Identificacion);

        var persona = await personaRepository.FindByIdentificacionAsync(request.Identificacion, cancellationToken);
        if (persona is not null)
        {
            var existingProveedor = await proveedorRepository.GetByPersonaIdAsync(persona.Id, cancellationToken: cancellationToken);
            if (existingProveedor is not null)
            {
                throw new InvalidOperationException("La persona ya tiene el rol de proveedor. Puedes editarla desde la lista.");
            }

            persona = await personaRepository.UpdateAsync(BuildPersona(persona.Id, persona.CreatedAt, request), cancellationToken)
                ?? throw new InvalidOperationException("No se pudo actualizar la persona base del proveedor.");
        }
        else
        {
            persona = await personaRepository.CreateAsync(BuildPersona(Guid.NewGuid(), DateTimeOffset.UtcNow, request), cancellationToken);
        }

        var proveedor = new Proveedor(
            persona.Id,
            persona.Id,
            currentUserAccessor.GetRequiredEmpresaId(),
            persona.TipoIdentificacion,
            persona.Identificacion,
            persona.RazonSocialONombresCompletos,
            persona.NombreComercial,
            persona.DireccionPrincipal,
            persona.CorreoElectronicoPrincipal,
            persona.TelefonoCelular,
            request.CodigoRetencionIvaDefault.Trim(),
            request.CodigoRetencionRentaDefault.Trim(),
            request.PermiteCredito,
            request.DiasCredito,
            ParseEstadoProveedor(request.EstadoProveedor),
            persona.RolesPersona.Concat(["Proveedor"]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            request.IsActive,
            DateTimeOffset.UtcNow,
            currentUserAccessor.GetRequiredUserId(),
            null,
            null);

        return MapToResponse(await proveedorRepository.CreateAsync(proveedor, cancellationToken));
    }

    public async Task<ProveedorResponse?> UpdateAsync(Guid id, ProveedorRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        var tipoIdentificacion = GetValidatedTipoIdentificacion(request.TipoIdentificacion);
        EnsureProveedorIdentification(tipoIdentificacion, request.Identificacion);

        var current = await proveedorRepository.GetByIdAsync(id, cancellationToken);
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
            ?? throw new InvalidOperationException("No se encontro la persona asociada al proveedor.");

        var updatedPersona = await personaRepository.UpdateAsync(BuildPersona(currentPersona.Id, currentPersona.CreatedAt, request), cancellationToken)
            ?? throw new InvalidOperationException("No se pudo actualizar la persona del proveedor.");

        var proveedor = new Proveedor(
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
            request.CodigoRetencionIvaDefault.Trim(),
            request.CodigoRetencionRentaDefault.Trim(),
            request.PermiteCredito,
            request.DiasCredito,
            ParseEstadoProveedor(request.EstadoProveedor),
            updatedPersona.RolesPersona,
            request.IsActive,
            current.CreatedAt,
            current.UsuarioCreacionId,
            DateTimeOffset.UtcNow,
            currentUserAccessor.GetRequiredUserId());

        var updated = await proveedorRepository.UpdateAsync(proveedor, cancellationToken);
        return updated is null ? null : MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return proveedorRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task ValidateCatalogValuesAsync(ProveedorRequest request, CancellationToken cancellationToken)
    {
        var tipoIdentificacion = SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion);
        if (tipoIdentificacion is null ||
            !AllowedTipoIdentificacion.Contains(tipoIdentificacion) ||
            !await catalogoRepository.ExistsActiveItemAsync("TIPO_IDENTIFICACION", tipoIdentificacion, cancellationToken))
        {
            throw new InvalidOperationException("El proveedor solo admite tipo de identificacion RUC (04) o Pasaporte (06).");
        }
    }

    private static string GetValidatedTipoIdentificacion(string tipoIdentificacion)
    {
        var normalized = SriCatalogCodes.NormalizeTipoIdentificacionCode(tipoIdentificacion)
            ?? throw new InvalidOperationException("El proveedor solo admite tipo de identificacion RUC (04) o Pasaporte (06).");

        if (!AllowedTipoIdentificacion.Contains(normalized))
        {
            throw new InvalidOperationException("El proveedor solo admite tipo de identificacion RUC (04) o Pasaporte (06).");
        }

        return normalized;
    }

    private static void EnsureProveedorIdentification(string tipoIdentificacion, string identificacion)
    {
        EcuadorIdentificationValidator.EnsureValid(tipoIdentificacion, identificacion, "el proveedor");

        if (tipoIdentificacion == "04")
        {
            var thirdDigit = identificacion.Trim()[2] - '0';
            if (thirdDigit is not 6 and not 9 and not <= 5)
            {
                throw new InvalidOperationException("El RUC del proveedor no tiene una estructura ecuatoriana valida.");
            }
        }
    }

    private static Persona BuildPersona(Guid id, DateTimeOffset createdAt, ProveedorRequest request)
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
            null,
            null,
            [],
            request.IsActive,
            createdAt,
            DateTimeOffset.UtcNow);
    }

    private static ProveedorResponse MapToResponse(Proveedor proveedor)
    {
        return new ProveedorResponse
        {
            Id = proveedor.Id,
            PersonaId = proveedor.PersonaId,
            TipoIdentificacion = proveedor.TipoIdentificacion,
            Identificacion = proveedor.Identificacion,
            RazonSocialONombresCompletos = proveedor.RazonSocialONombresCompletos,
            NombreComercial = proveedor.NombreComercial,
            DireccionPrincipal = proveedor.DireccionPrincipal,
            CorreoElectronicoPrincipal = proveedor.CorreoElectronicoPrincipal,
            TelefonoCelular = proveedor.TelefonoCelular,
            CodigoRetencionIvaDefault = proveedor.CodigoRetencionIvaDefault,
            CodigoRetencionRentaDefault = proveedor.CodigoRetencionRentaDefault,
            PermiteCredito = proveedor.PermiteCredito,
            DiasCredito = proveedor.DiasCredito,
            EstadoProveedor = proveedor.EstadoProveedor.ToString(),
            RolesPersona = proveedor.RolesPersona,
            IsActive = proveedor.IsActive,
            CreatedAt = proveedor.CreatedAt,
            UsuarioCreacionId = proveedor.UsuarioCreacionId,
            UpdatedAt = proveedor.UpdatedAt,
            UsuarioModificacionId = proveedor.UsuarioModificacionId
        };
    }

    private static EstadoProveedor ParseEstadoProveedor(string value)
    {
        return Enum.TryParse<EstadoProveedor>(value?.Trim(), true, out var parsed)
            ? parsed
            : EstadoProveedor.Activo;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}