using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Security.Ports.In;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Security;
using TestDeIa.Shared.Responses.Security;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Route("api/security/usuarios-admin")]
[Authorize(Roles = SecurityRoleNames.Administrador)]
[Authorize(Policy = SecurityPolicyNames.UsuariosAdministrar)]
public sealed class SecurityUsuariosAdminController : ControllerBase
{
    private readonly ISecurityManagementUseCase securityManagementUseCase;
    private readonly TestDeIaDbContext dbContext;

    public SecurityUsuariosAdminController(
        ISecurityManagementUseCase securityManagementUseCase,
        TestDeIaDbContext dbContext)
    {
        this.securityManagementUseCase = securityManagementUseCase;
        this.dbContext = dbContext;
    }

    [HttpGet("puntos-emision")]
    public async Task<IActionResult> GetPuntosEmision(CancellationToken cancellationToken)
    {
        var empresaId = ResolveEmpresaId();
        if (!empresaId.HasValue)
        {
            return Unauthorized(new { message = "No se pudo identificar la empresa activa." });
        }

        var puntos = await dbContext.Set<EmpresaPuntoEmisionEntity>()
            .AsNoTracking()
            .Where(current => current.EmpresaEmisoraId == empresaId.Value)
            .OrderByDescending(current => current.IsDefault)
            .ThenBy(current => current.Establecimiento)
            .ThenBy(current => current.PuntoEmision)
            .Select(current => new SecurityPointEmissionResponse
            {
                Id = current.Id,
                EmpresaId = current.EmpresaEmisoraId,
                BodegaId = current.BodegaId,
                Establecimiento = current.Establecimiento,
                PuntoEmision = current.PuntoEmision,
                DisplayName = $"{current.Establecimiento}-{current.PuntoEmision}",
                DireccionEstablecimiento = current.DireccionEstablecimiento,
                IsDefault = current.IsDefault
            })
            .ToArrayAsync(cancellationToken);

        return Ok(puntos);
    }

    [HttpGet("{id:guid}/puntos-emision")]
    public async Task<IActionResult> GetPuntosEmisionByUser(Guid id, CancellationToken cancellationToken)
    {
        var empresaId = ResolveEmpresaId();
        if (!empresaId.HasValue)
        {
            return Unauthorized(new { message = "No se pudo identificar la empresa activa." });
        }

        var puntoEmisionIds = await dbContext.Set<SecurityUserPuntoEmisionEntity>()
            .AsNoTracking()
            .Where(current => current.SecurityUserId == id && current.EmpresaId == empresaId.Value)
            .Select(current => current.EmpresaPuntoEmisionId)
            .ToArrayAsync(cancellationToken);

        return Ok(puntoEmisionIds);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SecurityUserAdminRequest request, CancellationToken cancellationToken)
    {
        return await SaveAsync(null, request, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SecurityUserAdminRequest request, CancellationToken cancellationToken)
    {
        return await SaveAsync(id, request, cancellationToken);
    }

    private async Task<IActionResult> SaveAsync(Guid? id, SecurityUserAdminRequest request, CancellationToken cancellationToken)
    {
        var empresaId = ResolveEmpresaId();
        if (!empresaId.HasValue)
        {
            return Unauthorized(new { message = "No se pudo identificar la empresa activa." });
        }

        var requiresPuntosEmision = request.Roles.Any(role => string.Equals(role, SecurityRoleNames.Cajero, StringComparison.OrdinalIgnoreCase));
        var selectedPuntoEmisionIds = request.PuntoEmisionIds.Distinct().ToArray();
        if (requiresPuntosEmision && selectedPuntoEmisionIds.Length == 0)
        {
            return BadRequest(new { message = "Si el usuario tiene el rol Cajero debes asignar al menos un punto de emision." });
        }

        var puntosValidos = await dbContext.Set<EmpresaPuntoEmisionEntity>()
            .AsNoTracking()
            .Where(current => current.EmpresaEmisoraId == empresaId.Value)
            .Select(current => current.Id)
            .ToArrayAsync(cancellationToken);

        if (selectedPuntoEmisionIds.Any(idPunto => !puntosValidos.Contains(idPunto)))
        {
            return BadRequest(new { message = "Uno o mas puntos de emision seleccionados no pertenecen a la empresa activa." });
        }

        var mappedRequest = new SecurityUserRequest
        {
            TipoIdentificacion = request.TipoIdentificacion,
            Identificacion = request.Identificacion,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Email = request.Email,
            Telefono = request.Telefono,
            Direccion = request.Direccion,
            UserName = request.UserName,
            Password = request.Password,
            Roles = request.Roles.ToArray(),
            IsActive = request.IsActive
        };

        SecurityUserResponse? result = id.HasValue
            ? await securityManagementUseCase.UpdateUserAsync(id.Value, mappedRequest, cancellationToken)
            : await securityManagementUseCase.CreateUserAsync(mappedRequest, cancellationToken);

        if (result is null)
        {
            return BadRequest(new { message = "No se pudo guardar el usuario." });
        }

        await SyncPuntosEmisionAsync(result.Id, selectedPuntoEmisionIds, empresaId.Value, cancellationToken);

        return Ok(result);
    }

    private async Task SyncPuntosEmisionAsync(Guid userId, IReadOnlyCollection<Guid> puntoEmisionIds, Guid empresaId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Set<SecurityUserPuntoEmisionEntity>()
            .Where(current => current.SecurityUserId == userId && current.EmpresaId == empresaId)
            .ToListAsync(cancellationToken);

        dbContext.Set<SecurityUserPuntoEmisionEntity>().RemoveRange(existing);

        if (puntoEmisionIds.Count > 0)
        {
            var assignments = puntoEmisionIds.Select(puntoEmisionId => new SecurityUserPuntoEmisionEntity
            {
                SecurityUserId = userId,
                EmpresaId = empresaId,
                EmpresaPuntoEmisionId = puntoEmisionId,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await dbContext.Set<SecurityUserPuntoEmisionEntity>().AddRangeAsync(assignments, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Guid? ResolveEmpresaId()
    {
        if (!Request.Headers.TryGetValue("X-Empresa-Id", out var value))
        {
            return null;
        }

        return Guid.TryParse(value.FirstOrDefault(), out var empresaId)
            ? empresaId
            : null;
    }
}


