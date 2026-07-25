using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Infrastructure.Adapters.Out.Contabilidad;
using TestDeIa.Infrastructure.Persistence.Configurations;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Security;

namespace TestDeIa.Infrastructure.Persistence;

public sealed class DbInitializer(
    TestDeIaDbContext dbContext,
    IPasswordHashService passwordHashService,
    CatalogoNiifSeed catalogoNiifSeed,
    ILogger<DbInitializer> logger)
{
    private static readonly Guid DefaultEmpresaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DefaultAdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid DefaultAdminPersonaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid DefaultBodegaId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid DefaultPuntoEmisionId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid RetencionIvaCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000008");
    private static readonly Guid RetencionRentaCatalogoId = Guid.Parse("70000000-0000-0000-0000-000000000009");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Inicializando base de datos EasyMarket...");
        await dbContext.Database.MigrateAsync(cancellationToken);

        await EnsureDefaultCompanyAsync(cancellationToken);
        await EnsureSecurityRoleAsync(cancellationToken);
        await EnsureDefaultAdminUserAsync(cancellationToken);
        await EnsureDefaultWarehouseAndPointAsync(cancellationToken);
        await EnsureAdminPointAssignmentAsync(cancellationToken);
        await EnsureNiifAccountsAsync(cancellationToken);
        await EnsureRetentionCatalogsAsync(cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Inicializacion de base de datos completada.");
    }

    private async Task EnsureDefaultCompanyAsync(CancellationToken cancellationToken)
    {
        var empresa = await dbContext.EmpresasEmisoras
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.Id == DefaultEmpresaId || current.Ruc == "0999999999001", cancellationToken);

        if (empresa is null)
        {
            dbContext.EmpresasEmisoras.Add(new EmpresaEmisoraEntity
            {
                Id = DefaultEmpresaId,
                OwnerUserId = DefaultAdminUserId,
                RazonSocial = "Empresa Demo S.A.",
                NombreComercial = "EasyMarket Demo",
                Ruc = "0999999999001",
                DireccionMatriz = "Matriz demo",
                DireccionEstablecimiento = "Sucursal demo",
                Establecimiento = "001",
                PuntoEmision = "001",
                AmbienteSri = "1",
                ModoDesarrollo = true,
                TipoEmision = "1",
                ObligadoContabilidad = false,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });

            return;
        }

        empresa.RazonSocial = string.IsNullOrWhiteSpace(empresa.RazonSocial) ? "Empresa Demo S.A." : empresa.RazonSocial;
        empresa.NombreComercial = string.IsNullOrWhiteSpace(empresa.NombreComercial) ? "EasyMarket Demo" : empresa.NombreComercial;
        empresa.AmbienteSri = string.IsNullOrWhiteSpace(empresa.AmbienteSri) ? "1" : empresa.AmbienteSri;
        empresa.TipoEmision = string.IsNullOrWhiteSpace(empresa.TipoEmision) ? "1" : empresa.TipoEmision;
        empresa.ModoDesarrollo = true;
        empresa.IsActive = true;
    }

    private async Task EnsureSecurityRoleAsync(CancellationToken cancellationToken)
    {
        var normalizedName = SecurityRoleNames.Administrador.ToUpperInvariant();
        var exists = await dbContext.SecurityRoles
            .IgnoreQueryFilters()
            .AnyAsync(current => current.NormalizedName == normalizedName, cancellationToken);

        if (!exists)
        {
            dbContext.SecurityRoles.Add(new SecurityRoleEntity
            {
                Id = SecuritySeedIds.AdministradorRoleId,
                Name = SecurityRoleNames.Administrador,
                NormalizedName = normalizedName,
                IsActive = true
            });
        }
    }

    private async Task EnsureDefaultAdminUserAsync(CancellationToken cancellationToken)
    {
        var normalizedEmail = "ADMIN@EASYMARKET.COM";
        var user = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current =>
                current.NormalizedEmail == normalizedEmail ||
                current.Id == DefaultAdminUserId ||
                (current.EmpresaId == DefaultEmpresaId && current.NormalizedUserName == "ADMIN"),
                cancellationToken);

        if (user is null)
        {
            await EnsureAdminPersonaAsync(DefaultAdminPersonaId, cancellationToken);

            user = new SecurityUserEntity
            {
                Id = DefaultAdminUserId,
                EmpresaId = DefaultEmpresaId,
                PersonaId = DefaultAdminPersonaId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                DisplayName = "Administrador del Sistema",
                Email = "admin@easymarket.com",
                NormalizedEmail = normalizedEmail,
                PasswordHash = passwordHashService.Hash("Admin1234!*"),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                IntentosFallidos = 0,
                BloqueadoManualmente = false
            };

            dbContext.SecurityUsers.Add(user);
        }
        else
        {
            user.EmpresaId = DefaultEmpresaId;
            user.UserName = "admin";
            user.NormalizedUserName = "ADMIN";
            user.DisplayName = "Administrador del Sistema";
            user.Email = "admin@easymarket.com";
            user.NormalizedEmail = normalizedEmail;
            user.IsActive = true;
            user.BloqueadoManualmente = false;
        }

        await EnsureAdminPersonaAsync(user.PersonaId, cancellationToken);
        await EnsureAdminRoleAssignmentAsync(user.Id, cancellationToken);
        await EnsureAdminCompanyAssignmentAsync(user.Id, cancellationToken);
    }

    private async Task EnsureAdminPersonaAsync(Guid personaId, CancellationToken cancellationToken)
    {
        var persona = await dbContext.Personas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.Id == personaId || current.CorreoElectronicoPrincipal == "admin@easymarket.com", cancellationToken);

        if (persona is not null)
        {
            persona.EmpresaId = DefaultEmpresaId;
            persona.RazonSocialONombresCompletos = "Administrador del Sistema";
            persona.CorreoElectronicoPrincipal = "admin@easymarket.com";
            persona.IsActive = true;
            persona.IsSystemRecord = true;
            return;
        }

        dbContext.Personas.Add(new PersonaEntity
        {
            Id = personaId,
            EmpresaId = DefaultEmpresaId,
            TipoIdentificacion = "05",
            Identificacion = "ADMIN-EASYMARKET",
            RazonSocialONombresCompletos = "Administrador del Sistema",
            DireccionPrincipal = "Sistema",
            CorreoElectronicoPrincipal = "admin@easymarket.com",
            IsActive = true,
            IsSystemRecord = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private async Task EnsureAdminRoleAssignmentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.SecurityUserRoles
            .IgnoreQueryFilters()
            .AnyAsync(current => current.UserId == userId && current.RoleId == SecuritySeedIds.AdministradorRoleId, cancellationToken);

        if (!exists)
        {
            dbContext.SecurityUserRoles.Add(new SecurityUserRoleEntity
            {
                UserId = userId,
                RoleId = SecuritySeedIds.AdministradorRoleId
            });
        }
    }

    private async Task EnsureAdminCompanyAssignmentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.SecurityUserEmpresas
            .IgnoreQueryFilters()
            .AnyAsync(current => current.SecurityUserId == userId && current.EmpresaId == DefaultEmpresaId, cancellationToken);

        if (!exists)
        {
            dbContext.SecurityUserEmpresas.Add(new SecurityUserEmpresaEntity
            {
                SecurityUserId = userId,
                EmpresaId = DefaultEmpresaId,
                IsDefault = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    private async Task EnsureDefaultWarehouseAndPointAsync(CancellationToken cancellationToken)
    {
        var bodega = await dbContext.Bodegas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.Id == DefaultBodegaId || (current.EmpresaId == DefaultEmpresaId && current.Nombre == "Principal"), cancellationToken);

        if (bodega is null)
        {
            bodega = new BodegaEntity
            {
                Id = DefaultBodegaId,
                EmpresaId = DefaultEmpresaId,
                Nombre = "Principal",
                Direccion = "Matriz principal",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.Bodegas.Add(bodega);
        }
        else
        {
            bodega.IsActive = true;
        }

        var hasPoint = await dbContext.EmpresaPuntosEmision
            .IgnoreQueryFilters()
            .AnyAsync(current => current.Id == DefaultPuntoEmisionId || (current.EmpresaEmisoraId == DefaultEmpresaId && current.Establecimiento == "001" && current.PuntoEmision == "001"), cancellationToken);

        if (!hasPoint)
        {
            dbContext.EmpresaPuntosEmision.Add(new EmpresaPuntoEmisionEntity
            {
                Id = DefaultPuntoEmisionId,
                EmpresaEmisoraId = DefaultEmpresaId,
                BodegaId = bodega.Id,
                Establecimiento = "001",
                PuntoEmision = "001",
                DireccionEstablecimiento = "Sucursal demo",
                IsDefault = true
            });
        }
    }

    private async Task EnsureAdminPointAssignmentAsync(CancellationToken cancellationToken)
    {
        var user = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.NormalizedEmail == "ADMIN@EASYMARKET.COM" || current.Id == DefaultAdminUserId, cancellationToken);

        var point = await dbContext.EmpresaPuntosEmision
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == DefaultPuntoEmisionId || (current.EmpresaEmisoraId == DefaultEmpresaId && current.Establecimiento == "001" && current.PuntoEmision == "001"), cancellationToken);

        if (user is null || point is null)
        {
            return;
        }

        var exists = await dbContext.Set<SecurityUserPuntoEmisionEntity>()
            .IgnoreQueryFilters()
            .AnyAsync(current => current.SecurityUserId == user.Id && current.EmpresaPuntoEmisionId == point.Id, cancellationToken);

        if (!exists)
        {
            dbContext.Set<SecurityUserPuntoEmisionEntity>().Add(new SecurityUserPuntoEmisionEntity
            {
                SecurityUserId = user.Id,
                EmpresaId = DefaultEmpresaId,
                EmpresaPuntoEmisionId = point.Id,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    private async Task EnsureNiifAccountsAsync(CancellationToken cancellationToken)
    {
        var exists = await dbContext.CuentasContables
            .IgnoreQueryFilters()
            .AnyAsync(current => current.EmpresaId == DefaultEmpresaId, cancellationToken);

        if (!exists)
        {
            dbContext.CuentasContables.AddRange(catalogoNiifSeed.BuildForEmpresa(DefaultEmpresaId));
        }
    }

    private async Task EnsureRetentionCatalogsAsync(CancellationToken cancellationToken)
    {
        await EnsureCatalogAsync(
            RetencionIvaCatalogoId,
            "RETENCION_IVA_SRI",
            "Retenciones IVA SRI",
            "Codigos base de retencion de IVA para documentos de proveedor.",
            cancellationToken);

        await EnsureCatalogAsync(
            RetencionRentaCatalogoId,
            "RETENCION_RENTA_SRI",
            "Retenciones Renta SRI",
            "Codigos base de retencion en la fuente para documentos de proveedor.",
            cancellationToken);

        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "0", "Sin retencion IVA", 1, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "10", "Retencion IVA 10%", 2, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "20", "Retencion IVA 20%", 3, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "30", "Retencion IVA 30%", 4, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "70", "Retencion IVA 70%", 5, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "100", "Retencion IVA 100%", 6, cancellationToken);

        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "0", "Sin retencion renta", 1, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "332", "Bienes 1.75%", 2, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "344", "Servicios 2.75%", 3, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "312", "Honorarios profesionales 10%", 4, cancellationToken);
    }

    private async Task EnsureCatalogAsync(Guid id, string codigo, string nombre, string descripcion, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Catalogos
            .AnyAsync(current => current.Id == id || current.Codigo == codigo, cancellationToken);

        if (!exists)
        {
            dbContext.Catalogos.Add(new CatalogoEntity
            {
                Id = id,
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = descripcion,
                IsActive = true
            });
        }
    }

    private async Task EnsureCatalogItemAsync(Guid catalogoId, string codigo, string nombre, int orden, CancellationToken cancellationToken)
    {
        var exists = await dbContext.CatalogoItems
            .AnyAsync(current => current.CatalogoId == catalogoId && current.Codigo == codigo, cancellationToken);

        if (!exists)
        {
            dbContext.CatalogoItems.Add(new CatalogoItemEntity
            {
                Id = Guid.NewGuid(),
                CatalogoId = catalogoId,
                Codigo = codigo,
                Nombre = nombre,
                Orden = orden,
                IsActive = true
            });
        }
    }
}
