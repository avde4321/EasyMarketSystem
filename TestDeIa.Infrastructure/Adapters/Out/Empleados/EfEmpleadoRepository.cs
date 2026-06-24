using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Empleados.Ports.Out;
using TestDeIa.Domain.Modules.Empleados.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Empleados;

public sealed class EfEmpleadoRepository : IEmpleadoRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfEmpleadoRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<IReadOnlyCollection<Empleado>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var empleados = await BaseQuery()
            .OrderBy(empleado => empleado.Persona.Apellidos)
            .ThenBy(empleado => empleado.Persona.Nombres)
            .ToListAsync(cancellationToken);

        return empleados.Select(MapToDomain).ToArray();
    }

    public async Task<Empleado?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empleado = await BaseQuery().FirstOrDefaultAsync(current => current.Id == id, cancellationToken);
        return empleado is null ? null : MapToDomain(empleado);
    }

    public async Task<Empleado?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var empleado = await BaseQuery()
            .FirstOrDefaultAsync(
                current => current.PersonaId == personaId && (!excludedId.HasValue || current.Id != excludedId.Value),
                cancellationToken);

        return empleado is null ? null : MapToDomain(empleado);
    }

    public async Task<Empleado> CreateAsync(Empleado empleado, CancellationToken cancellationToken = default)
    {
        var entity = new EmpleadoEntity
        {
            Id = empleado.Id,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para el empleado."),
            PersonaId = empleado.PersonaId,
            IsActive = empleado.IsActive,
            CreatedAt = empleado.CreatedAt,
            UpdatedAt = empleado.UpdatedAt
        };

        dbContext.Empleados.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? empleado;
    }

    public async Task<Empleado?> UpdateAsync(Empleado empleado, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Empleados.FirstOrDefaultAsync(current => current.Id == empleado.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.IsActive = empleado.IsActive;
        entity.UpdatedAt = empleado.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Empleados.FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        dbContext.Empleados.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<EmpleadoEntity> BaseQuery()
    {
        return dbContext.Empleados
            .AsNoTracking()
            .Include(empleado => empleado.Persona)
            .ThenInclude(persona => persona.Cliente)
            .Include(empleado => empleado.Persona)
            .ThenInclude(persona => persona.SecurityUser);
    }

    private static Empleado MapToDomain(EmpleadoEntity entity)
    {
        return new Empleado(
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
        var roles = new List<string> { "Empleado" };

        if (persona.Cliente is not null)
        {
            roles.Add("Cliente");
        }

        if (persona.SecurityUser is not null)
        {
            roles.Add("Usuario");
        }

        return roles.ToArray();
    }
}
