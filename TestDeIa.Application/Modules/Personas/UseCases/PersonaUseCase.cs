using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.In;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Application.Common;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Shared.Requests.Personas;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Personas;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Application.Modules.Personas.UseCases;

public sealed class PersonaUseCase : IPersonaUseCase
{
    private readonly IPersonaRepository personaRepository;
    private readonly ICatalogoRepository catalogoRepository;

    public PersonaUseCase(IPersonaRepository personaRepository, ICatalogoRepository catalogoRepository)
    {
        this.personaRepository = personaRepository;
        this.catalogoRepository = catalogoRepository;
    }

    public async Task<IReadOnlyCollection<PersonaResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personas = await personaRepository.GetAllAsync(cancellationToken);
        return personas.Select(MapToResponse).ToArray();
    }

    public async Task<PagedResultResponse<PersonaResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var page = await personaRepository.GetPagedAsync(term, skip, take, cancellationToken);
        return new PagedResultResponse<PersonaResponse>
        {
            Items = page.Items.Select(MapToResponse).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<PersonaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var persona = await personaRepository.GetByIdAsync(id, cancellationToken);
        return persona is null ? null : MapToResponse(persona);
    }

    public async Task<PersonaResponse?> FindByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
        {
            return null;
        }

        var persona = await personaRepository.FindByIdentificacionAsync(identificacion.Trim(), cancellationToken);
        return persona is null ? null : MapToResponse(persona);
    }

    public async Task<PersonaResponse> CreateAsync(PersonaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        var tipoIdentificacion = SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion)
            ?? throw new InvalidOperationException("El tipo de identificacion no coincide con los tipos soportados por facturacion electronica.");
        EcuadorIdentificationValidator.EnsureValid(tipoIdentificacion, request.Identificacion, "la persona");

        if (await personaRepository.ExistsByIdentificacionAsync(request.Identificacion, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una persona con esa identificacion.");
        }

        var persona = new Persona(
            Guid.NewGuid(),
            tipoIdentificacion,
            request.Identificacion.Trim(),
            request.RazonSocialONombresCompletos.Trim(),
            NormalizeOptional(request.NombreComercial),
            request.DireccionPrincipal.Trim(),
            NormalizeOptional(request.TelefonoCelular),
            NormalizeOptional(request.CorreoElectronicoPrincipal),
            request.FechaNacimiento,
            NormalizeOptional(request.Genero),
            request.EsPersonaJuridica,
            request.EsEmpresa,
            [],
            request.IsActive,
            DateTimeOffset.UtcNow,
            null,
            NormalizeOptional(request.RegionCodigo),
            NormalizeOptional(request.ProvinciaCodigo),
            NormalizeOptional(request.CiudadCodigo),
            NormalizeOptional(request.SectorCodigo));

        return MapToResponse(await personaRepository.CreateAsync(persona, cancellationToken));
    }

    public async Task<PersonaResponse?> UpdateAsync(Guid id, PersonaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCatalogValuesAsync(request, cancellationToken);
        var tipoIdentificacion = SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion)
            ?? throw new InvalidOperationException("El tipo de identificacion no coincide con los tipos soportados por facturacion electronica.");
        EcuadorIdentificationValidator.EnsureValid(tipoIdentificacion, request.Identificacion, "la persona");

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
            tipoIdentificacion,
            request.Identificacion.Trim(),
            request.RazonSocialONombresCompletos.Trim(),
            NormalizeOptional(request.NombreComercial),
            request.DireccionPrincipal.Trim(),
            NormalizeOptional(request.TelefonoCelular),
            NormalizeOptional(request.CorreoElectronicoPrincipal),
            request.FechaNacimiento,
            NormalizeOptional(request.Genero),
            request.EsPersonaJuridica,
            request.EsEmpresa,
            current.RolesPersona,
            request.IsActive,
            current.CreatedAt,
            DateTimeOffset.UtcNow,
            NormalizeOptional(request.RegionCodigo),
            NormalizeOptional(request.ProvinciaCodigo),
            NormalizeOptional(request.CiudadCodigo),
            NormalizeOptional(request.SectorCodigo));

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
            RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos,
            NombreComercial = persona.NombreComercial,
            DireccionPrincipal = persona.DireccionPrincipal,
            RegionCodigo = persona.RegionCodigo,
            ProvinciaCodigo = persona.ProvinciaCodigo,
            CiudadCodigo = persona.CiudadCodigo,
            SectorCodigo = persona.SectorCodigo,
            FechaNacimiento = persona.FechaNacimiento,
            CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal,
            TelefonoCelular = persona.TelefonoCelular,
            Genero = persona.Genero,
            EsPersonaJuridica = persona.EsPersonaJuridica,
            EsEmpresa = persona.EsEmpresa,
            RolesPersona = persona.RolesPersona,
            IsActive = persona.IsActive,
            CreatedAt = persona.CreatedAt,
            UpdatedAt = persona.UpdatedAt
        };
    }

    private async Task ValidateCatalogValuesAsync(PersonaRequest request, CancellationToken cancellationToken)
    {
        var tipoIdentificacion = SriCatalogCodes.NormalizeTipoIdentificacionCode(request.TipoIdentificacion);
        if (tipoIdentificacion is null ||
            !await catalogoRepository.ExistsActiveItemAsync("TIPO_IDENTIFICACION", tipoIdentificacion, cancellationToken))
        {
            throw new InvalidOperationException("El tipo de identificacion no coincide con los tipos soportados por facturacion electronica.");
        }

    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
