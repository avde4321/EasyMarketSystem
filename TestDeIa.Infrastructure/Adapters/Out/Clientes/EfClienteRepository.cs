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
        var clientes = await BaseQuery()
            .OrderBy(cliente => cliente.Persona.Apellidos)
            .ThenBy(cliente => cliente.Persona.Nombres)
            .ToListAsync(cancellationToken);

        return clientes.Select(MapToDomain).ToArray();
    }

    public async Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await BaseQuery().FirstOrDefaultAsync(current => current.Id == id, cancellationToken);
        return cliente is null ? null : MapToDomain(cliente);
    }

    public async Task<Cliente?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var cliente = await BaseQuery()
            .FirstOrDefaultAsync(
                current => current.PersonaId == personaId && (!excludedId.HasValue || current.Id != excludedId.Value),
                cancellationToken);

        return cliente is null ? null : MapToDomain(cliente);
    }

    public async Task<Cliente> CreateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var entity = new ClienteEntity
        {
            Id = cliente.Id,
            PersonaId = cliente.PersonaId,
            IsActive = cliente.IsActive,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt
        };

        dbContext.Clientes.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? cliente;
    }

    public async Task<Cliente?> UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Clientes
            .FirstOrDefaultAsync(current => current.Id == cliente.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.IsActive = cliente.IsActive;
        entity.UpdatedAt = cliente.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Clientes.FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        dbContext.Clientes.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<ClienteEntity> BaseQuery()
    {
        return dbContext.Clientes
            .AsNoTracking()
            .Include(cliente => cliente.Persona)
            .ThenInclude(persona => persona.Empleado)
            .Include(cliente => cliente.Persona)
            .ThenInclude(persona => persona.SecurityUser);
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
            ResolvePersonaRoles(entity.Persona),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static string[] ResolvePersonaRoles(PersonaEntity persona)
    {
        var roles = new List<string> { "Cliente" };

        if (persona.Empleado is not null)
        {
            roles.Add("Empleado");
        }

        if (persona.SecurityUser is not null)
        {
            roles.Add("Usuario");
        }

        return roles.ToArray();
    }
}
