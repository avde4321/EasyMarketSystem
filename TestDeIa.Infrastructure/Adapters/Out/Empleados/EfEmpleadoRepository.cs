using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Empleados.Ports.Out;
using TestDeIa.Domain.Modules.Empleados.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

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
            .OrderBy(empleado => empleado.Persona.RazonSocialONombresCompletos)
            .ToListAsync(cancellationToken);

        return empleados.Select(MapToDomain).ToArray();
    }

    public async Task<PagedResultResponse<Empleado>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(BaseQuery(), term);
        var totalCount = await query.CountAsync(cancellationToken);
        var empleados = await query
            .OrderBy(empleado => empleado.Persona.RazonSocialONombresCompletos)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<Empleado>
        {
            Items = empleados.Select(MapToDomain).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<Empleado?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empleado = await BaseQuery().FirstOrDefaultAsync(current => current.PersonaId == id, cancellationToken);
        return empleado is null ? null : MapToDomain(empleado);
    }

    public async Task<Empleado?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var empleado = await BaseQuery()
            .FirstOrDefaultAsync(
                current => current.PersonaId == personaId && (!excludedId.HasValue || current.PersonaId != excludedId.Value),
                cancellationToken);

        return empleado is null ? null : MapToDomain(empleado);
    }

    public async Task<Empleado> CreateAsync(Empleado empleado, CancellationToken cancellationToken = default)
    {
        var entity = new EmpleadoEntity
        {
            PersonaId = empleado.PersonaId,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para el empleado."),
            CodigoEmpleado = empleado.CodigoEmpleado,
            CodigoBiometrico = empleado.CodigoBiometrico,
            FechaIngreso = empleado.FechaIngreso,
            FechaSalida = empleado.FechaSalida,
            TipoContrato = empleado.TipoContrato,
            CargoPuesto = empleado.CargoPuesto,
            SueldoBase = empleado.SueldoBase,
            PorcentajeComisionVentas = empleado.PorcentajeComisionVentas,
            EstadoLaboral = empleado.EstadoLaboral,
            NombreContactoEmergencia = empleado.NombreContactoEmergencia,
            TelefonoEmergencia = empleado.TelefonoEmergencia,
            IsActive = empleado.IsActive,
            CreatedAt = empleado.CreatedAt,
            UsuarioCreacionId = empleado.UsuarioCreacionId,
            UpdatedAt = empleado.UpdatedAt,
            UsuarioModificacionId = empleado.UsuarioModificacionId
        };

        dbContext.Empleados.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.PersonaId, cancellationToken) ?? empleado;
    }

    public async Task<Empleado?> UpdateAsync(Empleado empleado, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Empleados.FirstOrDefaultAsync(current => current.PersonaId == empleado.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.CodigoEmpleado = empleado.CodigoEmpleado;
        entity.CodigoBiometrico = empleado.CodigoBiometrico;
        entity.FechaIngreso = empleado.FechaIngreso;
        entity.FechaSalida = empleado.FechaSalida;
        entity.TipoContrato = empleado.TipoContrato;
        entity.CargoPuesto = empleado.CargoPuesto;
        entity.SueldoBase = empleado.SueldoBase;
        entity.PorcentajeComisionVentas = empleado.PorcentajeComisionVentas;
        entity.EstadoLaboral = empleado.EstadoLaboral;
        entity.NombreContactoEmergencia = empleado.NombreContactoEmergencia;
        entity.TelefonoEmergencia = empleado.TelefonoEmergencia;
        entity.IsActive = empleado.IsActive;
        entity.UpdatedAt = empleado.UpdatedAt;
        entity.UsuarioModificacionId = empleado.UsuarioModificacionId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.PersonaId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Empleados.FirstOrDefaultAsync(current => current.PersonaId == id, cancellationToken);

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
            .ThenInclude(persona => persona.Proveedor)
            .Include(empleado => empleado.Persona)
            .ThenInclude(persona => persona.SecurityUser);
    }

    private static IQueryable<EmpleadoEntity> ApplyFilter(IQueryable<EmpleadoEntity> query, string? term)
    {
        var normalizedTerm = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTerm))
        {
            return query;
        }

        return query.Where(empleado =>
            empleado.Persona.TipoIdentificacion.Contains(normalizedTerm) ||
            empleado.Persona.Identificacion.Contains(normalizedTerm) ||
            empleado.Persona.RazonSocialONombresCompletos.Contains(normalizedTerm) ||
            (empleado.Persona.NombreComercial != null && empleado.Persona.NombreComercial.Contains(normalizedTerm)) ||
            (empleado.Persona.CorreoElectronicoPrincipal != null && empleado.Persona.CorreoElectronicoPrincipal.Contains(normalizedTerm)) ||
            (empleado.Persona.TelefonoCelular != null && empleado.Persona.TelefonoCelular.Contains(normalizedTerm)) ||
            empleado.Persona.DireccionPrincipal.Contains(normalizedTerm) ||
            (empleado.CodigoEmpleado != null && empleado.CodigoEmpleado.Contains(normalizedTerm)) ||
            (empleado.CargoPuesto != null && empleado.CargoPuesto.Contains(normalizedTerm)) ||
            empleado.TipoContrato.Contains(normalizedTerm) ||
            empleado.EstadoLaboral.Contains(normalizedTerm));
    }

    private static Empleado MapToDomain(EmpleadoEntity entity)
    {
        return new Empleado(
            entity.PersonaId,
            entity.PersonaId,
            entity.EmpresaId,
            entity.Persona.TipoIdentificacion,
            entity.Persona.Identificacion,
            entity.Persona.RazonSocialONombresCompletos,
            entity.Persona.NombreComercial,
            entity.Persona.DireccionPrincipal,
            entity.Persona.CorreoElectronicoPrincipal,
            entity.Persona.TelefonoCelular,
            entity.Persona.FechaNacimiento,
            entity.Persona.Genero,
            entity.CodigoEmpleado,
            entity.CodigoBiometrico,
            entity.FechaIngreso,
            entity.FechaSalida,
            entity.TipoContrato,
            entity.CargoPuesto,
            entity.SueldoBase,
            entity.PorcentajeComisionVentas,
            entity.EstadoLaboral,
            entity.NombreContactoEmergencia,
            entity.TelefonoEmergencia,
            ResolvePersonaRoles(entity.Persona),
            entity.IsActive,
            entity.CreatedAt,
            entity.UsuarioCreacionId,
            entity.UpdatedAt);
    }

    private static string[] ResolvePersonaRoles(PersonaEntity persona)
    {
        var roles = new List<string> { "Empleado" };

        if (persona.Cliente is not null)
        {
            roles.Add("Cliente");
        }

        if (persona.Proveedor is not null)
        {
            roles.Add("Proveedor");
        }

        if (persona.SecurityUser is not null)
        {
            roles.Add("Usuario");
        }

        return roles.ToArray();
    }
}
