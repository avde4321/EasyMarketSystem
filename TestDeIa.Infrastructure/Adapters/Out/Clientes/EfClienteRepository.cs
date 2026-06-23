using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Clientes.Ports.Out;
using TestDeIa.Domain.Modules.Clientes.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Clientes;

public sealed class EfClienteRepository : IClienteRepository
{
    private readonly TestDeIaDbContext dbContext;

    public EfClienteRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Cliente>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await dbContext.Clientes
            .AsNoTracking()
            .Include(cliente => cliente.Persona)
            .OrderBy(cliente => cliente.Persona.Apellidos)
            .ThenBy(cliente => cliente.Persona.Nombres)
            .ToListAsync(cancellationToken);

        return clientes.Select(MapToDomain).ToArray();
    }

    public async Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .Include(current => current.Persona)
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return cliente is null ? null : MapToDomain(cliente);
    }

    public Task<bool> ExistsByIdentificacionAsync(
        string identificacion,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedIdentificacion = identificacion.Trim();

        return dbContext.Clientes
            .Include(cliente => cliente.Persona)
            .AnyAsync(
                cliente =>
                    cliente.Persona.Identificacion == normalizedIdentificacion &&
                    (!excludedId.HasValue || cliente.Id != excludedId.Value),
                cancellationToken);
    }

    public Task<bool> ExistsPersonaByIdentificacionAsync(
        string identificacion,
        Guid? excludedPersonaId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedIdentificacion = identificacion.Trim();

        return dbContext.Personas.AnyAsync(
            persona =>
                persona.Identificacion == normalizedIdentificacion &&
                (!excludedPersonaId.HasValue || persona.Id != excludedPersonaId.Value),
            cancellationToken);
    }

    public async Task<Cliente> CreateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var persona = await dbContext.Personas
            .FirstOrDefaultAsync(
                current => current.Identificacion == cliente.Identificacion && !current.IsSystemRecord,
                cancellationToken);

        if (persona is null)
        {
            persona = new PersonaEntity
            {
                Id = cliente.PersonaId,
                TipoIdentificacion = cliente.TipoIdentificacion,
                Identificacion = cliente.Identificacion,
                Nombres = cliente.Nombres,
                Apellidos = cliente.Apellidos,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                IsActive = cliente.IsActive,
                CreatedAt = cliente.CreatedAt
            };
        }
        else
        {
            persona.TipoIdentificacion = cliente.TipoIdentificacion;
            persona.Nombres = cliente.Nombres;
            persona.Apellidos = cliente.Apellidos;
            persona.Email = cliente.Email;
            persona.Telefono = cliente.Telefono;
            persona.Direccion = cliente.Direccion;
            persona.IsActive = cliente.IsActive;
            persona.UpdatedAt = cliente.UpdatedAt ?? DateTimeOffset.UtcNow;
        }

        var entity = new ClienteEntity
        {
            Id = cliente.Id,
            PersonaId = persona.Id,
            Persona = persona,
            IsActive = cliente.IsActive,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt
        };

        dbContext.Clientes.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToDomain(entity);
    }

    public async Task<Cliente?> UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Clientes
            .Include(current => current.Persona)
            .FirstOrDefaultAsync(current => current.Id == cliente.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.IsActive = cliente.IsActive;
        entity.UpdatedAt = cliente.UpdatedAt;
        entity.Persona.TipoIdentificacion = cliente.TipoIdentificacion;
        entity.Persona.Identificacion = cliente.Identificacion;
        entity.Persona.Nombres = cliente.Nombres;
        entity.Persona.Apellidos = cliente.Apellidos;
        entity.Persona.Email = cliente.Email;
        entity.Persona.Telefono = cliente.Telefono;
        entity.Persona.Direccion = cliente.Direccion;
        entity.Persona.IsActive = cliente.IsActive;
        entity.Persona.UpdatedAt = cliente.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToDomain(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Clientes
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        dbContext.Clientes.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static Cliente MapToDomain(ClienteEntity entity)
    {
        return new Cliente(
            entity.Id,
            entity.PersonaId,
            entity.Persona.TipoIdentificacion,
            entity.Persona.Identificacion,
            entity.Persona.Nombres,
            entity.Persona.Apellidos,
            entity.Persona.Email,
            entity.Persona.Telefono,
            entity.Persona.Direccion,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
