using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Infrastructure.Adapters.Out.Personas;

public sealed class EfPersonaRepository : IPersonaRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfPersonaRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<IReadOnlyCollection<Persona>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personas = await BaseQuery()
            .Where(persona => !persona.IsSystemRecord)
            .OrderBy(persona => persona.RazonSocialONombresCompletos)
            .ToListAsync(cancellationToken);

        return personas.Select(MapToDomain).ToArray();
    }

    public async Task<PagedResultResponse<Persona>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term?.Trim();
        var query = BaseQuery()
            .Where(persona => !persona.IsSystemRecord);

        if (!string.IsNullOrWhiteSpace(normalizedTerm))
        {
            query = query.Where(persona =>
                persona.TipoIdentificacion.Contains(normalizedTerm) ||
                persona.Identificacion.Contains(normalizedTerm) ||
                persona.RazonSocialONombresCompletos.Contains(normalizedTerm) ||
                (persona.NombreComercial != null && persona.NombreComercial.Contains(normalizedTerm)) ||
                (persona.CorreoElectronicoPrincipal != null && persona.CorreoElectronicoPrincipal.Contains(normalizedTerm)) ||
                (persona.TelefonoCelular != null && persona.TelefonoCelular.Contains(normalizedTerm)) ||
                (persona.Cliente != null && "Cliente".Contains(normalizedTerm)) ||
                (persona.Empleado != null && "Empleado".Contains(normalizedTerm)) ||
                (persona.Proveedor != null && "Proveedor".Contains(normalizedTerm)) ||
                (persona.SecurityUser != null && "Usuario".Contains(normalizedTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var personas = await query
            .OrderBy(persona => persona.RazonSocialONombresCompletos)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<Persona>
        {
            Items = personas.Select(MapToDomain).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<Persona?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var persona = await BaseQuery()
            .FirstOrDefaultAsync(current => current.Id == id && !current.IsSystemRecord, cancellationToken);

        return persona is null ? null : MapToDomain(persona);
    }

    public async Task<Persona?> FindByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
    {
        var normalizedIdentificacion = identificacion.Trim();
        var persona = await BaseQuery()
            .FirstOrDefaultAsync(
                current => !current.IsSystemRecord && current.Identificacion == normalizedIdentificacion,
                cancellationToken);

        return persona is null ? null : MapToDomain(persona);
    }

    public Task<bool> ExistsByIdentificacionAsync(
        string identificacion,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedIdentificacion = identificacion.Trim();

        return dbContext.Personas.AnyAsync(
            persona =>
                !persona.IsSystemRecord &&
                persona.Identificacion == normalizedIdentificacion &&
                (!excludedId.HasValue || persona.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task<Persona> CreateAsync(Persona persona, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(persona);
        dbContext.Personas.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken) ?? MapToDomain(entity);
    }

    public async Task<Persona?> UpdateAsync(Persona persona, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Personas
            .FirstOrDefaultAsync(current => current.Id == persona.Id && !current.IsSystemRecord, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.TipoIdentificacion = persona.TipoIdentificacion;
        entity.Identificacion = persona.Identificacion;
        entity.RazonSocialONombresCompletos = persona.RazonSocialONombresCompletos;
        entity.NombreComercial = persona.NombreComercial;
        entity.DireccionPrincipal = persona.DireccionPrincipal;
        entity.RegionCodigo = persona.RegionCodigo;
        entity.ProvinciaCodigo = persona.ProvinciaCodigo;
        entity.CiudadCodigo = persona.CiudadCodigo;
        entity.SectorCodigo = persona.SectorCodigo;
        entity.FechaNacimiento = persona.FechaNacimiento;
        entity.CorreoElectronicoPrincipal = persona.CorreoElectronicoPrincipal;
        entity.TelefonoCelular = persona.TelefonoCelular;
        entity.Genero = persona.Genero;
        entity.EsPersonaJuridica = persona.EsPersonaJuridica;
        entity.EsEmpresa = persona.EsEmpresa;
        entity.IsActive = persona.IsActive;
        entity.UpdatedAt = persona.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    private IQueryable<PersonaEntity> BaseQuery()
    {
        return dbContext.Personas
            .AsNoTracking()
            .Include(persona => persona.Cliente)
            .Include(persona => persona.Empleado)
            .Include(persona => persona.Proveedor)
            .Include(persona => persona.SecurityUser);
    }

    private static Persona MapToDomain(PersonaEntity entity)
    {
        return new Persona(
            entity.Id,
            entity.TipoIdentificacion,
            entity.Identificacion,
            entity.RazonSocialONombresCompletos,
            entity.NombreComercial,
            entity.DireccionPrincipal,
            entity.TelefonoCelular,
            entity.CorreoElectronicoPrincipal,
            entity.FechaNacimiento,
            entity.Genero,
            entity.EsPersonaJuridica,
            entity.EsEmpresa,
            ResolvePersonaRoles(entity),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.RegionCodigo,
            entity.ProvinciaCodigo,
            entity.CiudadCodigo,
            entity.SectorCodigo);
    }

    private PersonaEntity MapToEntity(Persona persona)
    {
        return new PersonaEntity
        {
            Id = persona.Id,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para la persona."),
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
            IsActive = persona.IsActive,
            CreatedAt = persona.CreatedAt,
            UpdatedAt = persona.UpdatedAt
        };
    }

    private static string[] ResolvePersonaRoles(PersonaEntity entity)
    {
        var roles = new List<string>();

        if (entity.Cliente is not null)
        {
            roles.Add("Cliente");
        }

        if (entity.Empleado is not null)
        {
            roles.Add("Empleado");
        }

        if (entity.Proveedor is not null)
        {
            roles.Add("Proveedor");
        }

        if (entity.SecurityUser is not null)
        {
            roles.Add("Usuario");
        }

        return roles.ToArray();
    }
}
