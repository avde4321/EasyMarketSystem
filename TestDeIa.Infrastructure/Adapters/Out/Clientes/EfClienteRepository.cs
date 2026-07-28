using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Clientes.Ports.Out;
using TestDeIa.Domain.Modules.Clientes.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Infrastructure.Adapters.Out.Clientes;

public sealed class EfClienteRepository : IClienteRepository
{
    private readonly TestDeIaDbContext dbContext;
    private readonly ITenantContextAccessor tenantContextAccessor;

    public EfClienteRepository(TestDeIaDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        this.dbContext = dbContext;
        this.tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<IReadOnlyCollection<Cliente>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await BaseQuery()
            .OrderBy(cliente => cliente.Persona.RazonSocialONombresCompletos)
            .ToListAsync(cancellationToken);

        return clientes.Select(MapToDomain).ToArray();
    }

    public async Task<PagedResultResponse<Cliente>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(BaseQuery(), term);
        var totalCount = await query.CountAsync(cancellationToken);
        var clientes = await query
            .OrderBy(cliente => cliente.Persona.RazonSocialONombresCompletos)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResultResponse<Cliente>
        {
            Items = clientes.Select(MapToDomain).ToArray(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };
    }

    public async Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await BaseQuery().FirstOrDefaultAsync(current => current.PersonaId == id, cancellationToken);
        return cliente is null ? null : MapToDomain(cliente);
    }

    public async Task<Cliente?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default)
    {
        var cliente = await BaseQuery()
            .FirstOrDefaultAsync(
                current => current.PersonaId == personaId && (!excludedId.HasValue || current.PersonaId != excludedId.Value),
                cancellationToken);

        return cliente is null ? null : MapToDomain(cliente);
    }

    public async Task<Cliente> CreateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var entity = new ClienteEntity
        {
            PersonaId = cliente.PersonaId,
            EmpresaId = tenantContextAccessor.EmpresaId ?? throw new InvalidOperationException("No existe una empresa activa para el cliente."),
            CorreoFacturacionElectronica = cliente.CorreoFacturacionElectronica,
            TipoCliente = cliente.TipoCliente,
            ObligadoContabilidad = cliente.ObligadoContabilidad,
            EsContribuyenteEspecial = cliente.EsContribuyenteEspecial,
            PermiteCredito = cliente.PermiteCredito,
            LimiteCredito = cliente.LimiteCredito,
            DiasCreditoMaximo = cliente.DiasCreditoMaximo,
            EstadoCredito = cliente.EstadoCredito,
            IsActive = cliente.IsActive,
            CreatedAt = cliente.CreatedAt,
            UsuarioCreacionId = cliente.UsuarioCreacionId,
            UpdatedAt = cliente.UpdatedAt,
            UsuarioModificacionId = cliente.UsuarioModificacionId
        };

        dbContext.Clientes.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.PersonaId, cancellationToken) ?? cliente;
    }

    public async Task<Cliente?> UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Clientes
            .FirstOrDefaultAsync(current => current.PersonaId == cliente.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.CorreoFacturacionElectronica = cliente.CorreoFacturacionElectronica;
        entity.TipoCliente = cliente.TipoCliente;
        entity.ObligadoContabilidad = cliente.ObligadoContabilidad;
        entity.EsContribuyenteEspecial = cliente.EsContribuyenteEspecial;
        entity.PermiteCredito = cliente.PermiteCredito;
        entity.LimiteCredito = cliente.LimiteCredito;
        entity.DiasCreditoMaximo = cliente.DiasCreditoMaximo;
        entity.EstadoCredito = cliente.EstadoCredito;
        entity.IsActive = cliente.IsActive;
        entity.UpdatedAt = cliente.UpdatedAt;
        entity.UsuarioModificacionId = cliente.UsuarioModificacionId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.PersonaId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Clientes.FirstOrDefaultAsync(current => current.PersonaId == id, cancellationToken);

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
            .ThenInclude(persona => persona.Proveedor)
            .Include(cliente => cliente.Persona)
            .ThenInclude(persona => persona.SecurityUser);
    }

    private static IQueryable<ClienteEntity> ApplyFilter(IQueryable<ClienteEntity> query, string? term)
    {
        var normalizedTerm = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTerm))
        {
            return query;
        }

        return query.Where(cliente =>
            cliente.Persona.TipoIdentificacion.Contains(normalizedTerm) ||
            cliente.Persona.Identificacion.Contains(normalizedTerm) ||
            cliente.Persona.RazonSocialONombresCompletos.Contains(normalizedTerm) ||
            (cliente.Persona.NombreComercial != null && cliente.Persona.NombreComercial.Contains(normalizedTerm)) ||
            (cliente.Persona.CorreoElectronicoPrincipal != null && cliente.Persona.CorreoElectronicoPrincipal.Contains(normalizedTerm)) ||
            (cliente.Persona.TelefonoCelular != null && cliente.Persona.TelefonoCelular.Contains(normalizedTerm)) ||
            (cliente.Persona.DireccionPrincipal.Contains(normalizedTerm)) ||
            cliente.TipoCliente.Contains(normalizedTerm) ||
            cliente.EstadoCredito.Contains(normalizedTerm));
    }

    private static Cliente MapToDomain(ClienteEntity entity)
    {
        return new Cliente(
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
            entity.CorreoFacturacionElectronica,
            entity.TipoCliente,
            entity.ObligadoContabilidad,
            entity.EsContribuyenteEspecial,
            entity.PermiteCredito,
            entity.LimiteCredito,
            entity.DiasCreditoMaximo,
            entity.EstadoCredito,
            ResolvePersonaRoles(entity.Persona),
            entity.IsActive,
            entity.CreatedAt,
            entity.UsuarioCreacionId,
            entity.UpdatedAt,
            entity.Persona.RegionCodigo,
            entity.Persona.ProvinciaCodigo,
            entity.Persona.CiudadCodigo,
            entity.Persona.SectorCodigo);
    }

    private static string[] ResolvePersonaRoles(PersonaEntity persona)
    {
        var roles = new List<string> { "Cliente" };

        if (persona.Empleado is not null)
        {
            roles.Add("Empleado");
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
