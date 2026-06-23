using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Domain.Modules.Personas.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Personas;

public sealed class EfPersonaRepository : IPersonaRepository
{
    private readonly TestDeIaDbContext dbContext;

    public EfPersonaRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Persona>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personas = await dbContext.Personas
            .AsNoTracking()
            .Where(persona => !persona.IsSystemRecord)
            .OrderBy(persona => persona.Apellidos)
            .ThenBy(persona => persona.Nombres)
            .ToListAsync(cancellationToken);

        return personas.Select(MapToDomain).ToArray();
    }

    public async Task<Persona?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var persona = await dbContext.Personas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                current => current.Id == id && !current.IsSystemRecord,
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

        return MapToDomain(entity);
    }

    public async Task<Persona?> UpdateAsync(Persona persona, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Personas
            .FirstOrDefaultAsync(
                current => current.Id == persona.Id && !current.IsSystemRecord,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.TipoIdentificacion = persona.TipoIdentificacion;
        entity.Identificacion = persona.Identificacion;
        entity.Nombres = persona.Nombres;
        entity.Apellidos = persona.Apellidos;
        entity.FechaNacimiento = persona.FechaNacimiento;
        entity.Email = persona.Email;
        entity.Telefono = persona.Telefono;
        entity.Direccion = persona.Direccion;
        entity.IsActive = persona.IsActive;
        entity.UpdatedAt = persona.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDomain(entity);
    }

    private static Persona MapToDomain(PersonaEntity entity)
    {
        return new Persona(
            entity.Id,
            entity.TipoIdentificacion,
            entity.Identificacion,
            entity.Nombres,
            entity.Apellidos,
            entity.FechaNacimiento,
            entity.Email,
            entity.Telefono,
            entity.Direccion,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static PersonaEntity MapToEntity(Persona persona)
    {
        return new PersonaEntity
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
}
