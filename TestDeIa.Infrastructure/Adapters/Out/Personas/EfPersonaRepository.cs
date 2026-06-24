using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

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
            .OrderBy(persona => persona.Apellidos)
            .ThenBy(persona => persona.Nombres)
            .ToListAsync(cancellationToken);

        return personas.Select(MapToDomain).ToArray();
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
        entity.Nombres = persona.Nombres;
        entity.Apellidos = persona.Apellidos;
        entity.EstadoCivil = persona.EstadoCivil;
        entity.FechaNacimiento = persona.FechaNacimiento;
        entity.Email = persona.Email;
        entity.Telefono = persona.Telefono;
        entity.Direccion = persona.Direccion;
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
            .Include(persona => persona.SecurityUser);
    }

    private static Persona MapToDomain(PersonaEntity entity)
    {
        return new Persona(
            entity.Id,
            entity.TipoIdentificacion,
            entity.Identificacion,
            entity.Nombres,
            entity.Apellidos,
            entity.EstadoCivil,
            entity.FechaNacimiento,
            entity.Email,
            entity.Telefono,
            entity.Direccion,
            ResolvePersonaRoles(entity),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private PersonaEntity MapToEntity(Persona persona)
    {
        return new PersonaEntity
        {
            Id = persona.Id,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para la persona."),
            TipoIdentificacion = persona.TipoIdentificacion,
            Identificacion = persona.Identificacion,
            Nombres = persona.Nombres,
            Apellidos = persona.Apellidos,
            EstadoCivil = persona.EstadoCivil,
            FechaNacimiento = persona.FechaNacimiento,
            Email = persona.Email,
            Telefono = persona.Telefono,
            Direccion = persona.Direccion,
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

        if (entity.SecurityUser is not null)
        {
            roles.Add("Usuario");
        }

        return roles.ToArray();
    }
}
