using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class EfProveedorRepository : IProveedorRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfProveedorRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<IReadOnlyCollection<Proveedor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var proveedores = await BaseQuery()
            .OrderBy(proveedor => proveedor.Persona.RazonSocialONombresCompletos)
            .ToListAsync(cancellationToken);

        return proveedores.Select(MapToDomain).ToArray();
    }

    public async Task<PagedResultResponse<Proveedor>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(BaseQuery(), term);
        var totalCount = await query.CountAsync(cancellationToken);
        var proveedores = await query
            .OrderBy(proveedor => proveedor.Persona.RazonSocialONombresCompletos)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<Proveedor>
        {
            Items = proveedores.Select(MapToDomain).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<Proveedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proveedor = await BaseQuery().FirstOrDefaultAsync(current => current.PersonaId == id, cancellationToken);
        return proveedor is null ? null : MapToDomain(proveedor);
    }

    public async Task<Proveedor?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var proveedor = await BaseQuery()
            .FirstOrDefaultAsync(
                current => current.PersonaId == personaId && (!excludedId.HasValue || current.PersonaId != excludedId.Value),
                cancellationToken);

        return proveedor is null ? null : MapToDomain(proveedor);
    }

    public async Task<Proveedor> CreateAsync(Proveedor proveedor, CancellationToken cancellationToken = default)
    {
        var entity = new ProveedorEntity
        {
            PersonaId = proveedor.PersonaId,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para el proveedor."),
            CodigoRetencionIvaDefault = proveedor.CodigoRetencionIvaDefault,
            CodigoRetencionRentaDefault = proveedor.CodigoRetencionRentaDefault,
            PermiteCredito = proveedor.PermiteCredito,
            DiasCredito = proveedor.DiasCredito,
            EstadoProveedor = proveedor.EstadoProveedor.ToString(),
            IsActive = proveedor.IsActive,
            CreatedAt = proveedor.CreatedAt,
            UsuarioCreacionId = proveedor.UsuarioCreacionId,
            UpdatedAt = proveedor.UpdatedAt,
            UsuarioModificacionId = proveedor.UsuarioModificacionId
        };

        dbContext.Set<ProveedorEntity>().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.PersonaId, cancellationToken) ?? proveedor;
    }

    public async Task<Proveedor?> UpdateAsync(Proveedor proveedor, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Set<ProveedorEntity>()
            .FirstOrDefaultAsync(current => current.PersonaId == proveedor.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.CodigoRetencionIvaDefault = proveedor.CodigoRetencionIvaDefault;
        entity.CodigoRetencionRentaDefault = proveedor.CodigoRetencionRentaDefault;
        entity.PermiteCredito = proveedor.PermiteCredito;
        entity.DiasCredito = proveedor.DiasCredito;
        entity.EstadoProveedor = proveedor.EstadoProveedor.ToString();
        entity.IsActive = proveedor.IsActive;
        entity.UpdatedAt = proveedor.UpdatedAt;
        entity.UsuarioModificacionId = proveedor.UsuarioModificacionId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.PersonaId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Set<ProveedorEntity>().FirstOrDefaultAsync(current => current.PersonaId == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Set<ProveedorEntity>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<ProveedorEntity> BaseQuery()
    {
        return dbContext.Set<ProveedorEntity>()
            .AsNoTracking()
            .Include(proveedor => proveedor.Persona)
            .ThenInclude(persona => persona.Cliente)
            .Include(proveedor => proveedor.Persona)
            .ThenInclude(persona => persona.Empleado)
            .Include(proveedor => proveedor.Persona)
            .ThenInclude(persona => persona.SecurityUser);
    }

    private static IQueryable<ProveedorEntity> ApplyFilter(IQueryable<ProveedorEntity> query, string? term)
    {
        var normalizedTerm = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTerm))
        {
            return query;
        }

        return query.Where(proveedor =>
            proveedor.Persona.TipoIdentificacion.Contains(normalizedTerm) ||
            proveedor.Persona.Identificacion.Contains(normalizedTerm) ||
            proveedor.Persona.RazonSocialONombresCompletos.Contains(normalizedTerm) ||
            (proveedor.Persona.NombreComercial != null && proveedor.Persona.NombreComercial.Contains(normalizedTerm)) ||
            (proveedor.Persona.CorreoElectronicoPrincipal != null && proveedor.Persona.CorreoElectronicoPrincipal.Contains(normalizedTerm)) ||
            (proveedor.Persona.TelefonoCelular != null && proveedor.Persona.TelefonoCelular.Contains(normalizedTerm)) ||
            proveedor.Persona.DireccionPrincipal.Contains(normalizedTerm) ||
            proveedor.CodigoRetencionIvaDefault.Contains(normalizedTerm) ||
            proveedor.CodigoRetencionRentaDefault.Contains(normalizedTerm) ||
            proveedor.EstadoProveedor.Contains(normalizedTerm));
    }

    private static Proveedor MapToDomain(ProveedorEntity entity)
    {
        return new Proveedor(
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
            entity.CodigoRetencionIvaDefault,
            entity.CodigoRetencionRentaDefault,
            entity.PermiteCredito,
            entity.DiasCredito,
            Enum.TryParse<EstadoProveedor>(entity.EstadoProveedor, true, out var estadoProveedor) ? estadoProveedor : EstadoProveedor.Activo,
            ResolvePersonaRoles(entity.Persona),
            entity.IsActive,
            entity.CreatedAt,
            entity.UsuarioCreacionId,
            entity.UpdatedAt,
            entity.UsuarioModificacionId);
    }

    private static string[] ResolvePersonaRoles(PersonaEntity persona)
    {
        var roles = new List<string> { "Proveedor" };

        if (persona.Cliente is not null)
        {
            roles.Add("Cliente");
        }

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
