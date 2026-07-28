using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Geografia;

namespace TestDeIa.Api.Controllers;

[ApiController]
[Route("api/geografia")]
[AllowAnonymous]
public sealed class GeografiaController(TestDeIaDbContext dbContext) : ControllerBase
{
    [HttpGet("regiones")]
    public async Task<IActionResult> GetRegiones(CancellationToken cancellationToken)
    {
        var items = await dbContext.GeoRegiones
            .AsNoTracking()
            .Where(current => current.IsActive)
            .OrderBy(current => current.Orden)
            .ThenBy(current => current.Nombre)
            .Select(current => new GeoItemResponse
            {
                Id = current.Id,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Orden = current.Orden
            })
            .ToArrayAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("provincias")]
    public async Task<IActionResult> GetProvincias(CancellationToken cancellationToken)
    {
        var items = await dbContext.GeoProvincias
            .AsNoTracking()
            .Where(current => current.IsActive)
            .OrderBy(current => current.Orden)
            .ThenBy(current => current.Nombre)
            .Select(current => new GeoItemResponse
            {
                Id = current.Id,
                ParentId = current.RegionId,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Orden = current.Orden
            })
            .ToArrayAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("ciudades")]
    public async Task<IActionResult> GetCiudades(CancellationToken cancellationToken)
    {
        var items = await dbContext.GeoCiudades
            .AsNoTracking()
            .Where(current => current.IsActive)
            .OrderBy(current => current.Orden)
            .ThenBy(current => current.Nombre)
            .Select(current => new GeoItemResponse
            {
                Id = current.Id,
                ParentId = current.ProvinciaId,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Orden = current.Orden
            })
            .ToArrayAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("sectores")]
    public async Task<IActionResult> GetSectores(CancellationToken cancellationToken)
    {
        var items = await dbContext.GeoSectores
            .AsNoTracking()
            .Where(current => current.IsActive)
            .OrderBy(current => current.Orden)
            .ThenBy(current => current.Nombre)
            .Select(current => new GeoItemResponse
            {
                Id = current.Id,
                ParentId = current.CiudadId,
                Codigo = current.Codigo,
                Nombre = current.Nombre,
                Orden = current.Orden
            })
            .ToArrayAsync(cancellationToken);

        return Ok(items);
    }
}
