using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    GeoEcuadorSeed geoEcuadorSeed,
    ILogger<DbInitializer> logger,
    IConfiguration configuration,
    IHostEnvironment environment)
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
        var adminCredentials = ResolveAdminCredentials();
        logger.LogInformation("Inicializando base de datos EasyMarket...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        await EnsureXmlSriInfrastructureAsync(cancellationToken);

        await EnsureDefaultCompanyAsync(cancellationToken);
        await geoEcuadorSeed.EnsureSeededAsync(cancellationToken);
        await EnsureSecurityPermissionsAsync(cancellationToken);
        await EnsureSecurityRolesAsync(cancellationToken);
        await EnsureSecurityRolePermissionsAsync(cancellationToken);
        await EnsureDefaultAdminUserAsync(adminCredentials, cancellationToken);
        await EnsureDefaultWarehouseAndPointAsync(cancellationToken);
        await EnsureAdminPointAssignmentAsync(cancellationToken);
        await EnsureNiifAccountsAsync(cancellationToken);
        await EnsureRetentionCatalogsAsync(cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Inicializacion de base de datos completada.");
    }

    private async Task EnsureXmlSriInfrastructureAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[FacturaCompraXmlLogs](
                    [Id] uniqueidentifier NOT NULL,
                    [EmpresaId] uniqueidentifier NOT NULL,
                    [ClaveAcceso] nvarchar(49) NOT NULL,
                    [RucEmisor] nvarchar(13) NOT NULL,
                    [RazonSocialEmisor] nvarchar(300) NOT NULL,
                    [RucComprador] nvarchar(13) NOT NULL,
                    [FechaEmision] datetimeoffset NOT NULL,
                    [CodDoc] nvarchar(2) NOT NULL,
                    [EstabPuntoEmiSecuencial] nvarchar(17) NOT NULL,
                    [TotalSinImpuestos] decimal(18,4) NOT NULL,
                    [TotalDescuento] decimal(18,4) NOT NULL,
                    [ImporteTotal] decimal(18,4) NOT NULL,
                    [XmlContenido] nvarchar(max) NOT NULL,
                    [EstadoProcesamiento] tinyint NOT NULL,
                    [CompraId] uniqueidentifier NULL,
                    [CreatedAt] datetimeoffset NOT NULL,
                    [UpdatedAt] datetimeoffset NULL,
                    CONSTRAINT [PK_FacturaCompraXmlLogs] PRIMARY KEY ([Id])
                );
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[Compras]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_FacturaCompraXmlLogs_Compras_CompraId')
            BEGIN
                ALTER TABLE [dbo].[FacturaCompraXmlLogs]
                ADD CONSTRAINT [FK_FacturaCompraXmlLogs_Compras_CompraId]
                FOREIGN KEY ([CompraId]) REFERENCES [dbo].[Compras]([Id]) ON DELETE SET NULL;
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_CompraId' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
            BEGIN
                CREATE INDEX [IX_FacturaCompraXmlLogs_CompraId] ON [dbo].[FacturaCompraXmlLogs]([CompraId]);
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
            BEGIN
                CREATE UNIQUE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso]
                ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [ClaveAcceso]);
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
            BEGIN
                CREATE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision]
                ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [EstadoProcesamiento], [FechaEmision]);
            END
            """,
            cancellationToken);
    }

    /// <summary>Seed reservado a instalación/desarrollo: identifica la empresa demo por Id fijo y solo consulta globalmente si el RUC reservado ya está ocupado.</summary>
    private async Task EnsureDefaultCompanyAsync(CancellationToken cancellationToken)
    {
        var empresa = await dbContext.EmpresasEmisoras
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.Id == DefaultEmpresaId, cancellationToken);

        if (empresa is null)
        {
            var rucInUse = await dbContext.EmpresasEmisoras
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(current => current.Ruc == "0999999999001", cancellationToken);
            if (rucInUse)
            {
                throw new InvalidOperationException("El RUC reservado para la empresa demo ya pertenece a otro registro; el inicializador no reasignará una empresa existente.");
            }

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

    /// <summary>Siembra el catálogo global de roles de seguridad; el bypass es exclusivo de inicialización y usa IDs/nombres de catálogo conocidos.</summary>
    private async Task EnsureSecurityRolesAsync(CancellationToken cancellationToken)
    {
        var roles = new[]
        {
            new { Id = SecuritySeedIds.SuperAdministradorRoleId, Name = SecurityRoleNames.SuperAdministrador },
            new { Id = SecuritySeedIds.AdministradorEmpresaRoleId, Name = SecurityRoleNames.AdministradorEmpresa },
            new { Id = SecuritySeedIds.AdministradorRoleId, Name = SecurityRoleNames.Administrador },
            new { Id = SecuritySeedIds.GerenteRoleId, Name = SecurityRoleNames.Gerente },
            new { Id = SecuritySeedIds.CajeroRoleId, Name = SecurityRoleNames.Cajero },
            new { Id = SecuritySeedIds.AsesorComercialRoleId, Name = SecurityRoleNames.AsesorComercial },
            new { Id = SecuritySeedIds.BodegueroRoleId, Name = SecurityRoleNames.Bodeguero },
            new { Id = SecuritySeedIds.ContadorRoleId, Name = SecurityRoleNames.Contador }
        };

        foreach (var role in roles)
        {
            var normalizedName = role.Name.ToUpperInvariant();
            var entity = await dbContext.SecurityRoles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(current => current.Id == role.Id || current.NormalizedName == normalizedName, cancellationToken);

            if (entity is null)
            {
                dbContext.SecurityRoles.Add(new SecurityRoleEntity
                {
                    Id = role.Id,
                    Name = role.Name,
                    NormalizedName = normalizedName,
                    IsActive = true
                });

                continue;
            }

            if (role.Name == SecurityRoleNames.SuperAdministrador) continue; // No reactivar el rol reservado si fue deshabilitado.
            entity.Name = role.Name;
            entity.NormalizedName = normalizedName;
            entity.IsActive = true;
        }
    }

    /// <summary>Siembra permisos globales del sistema por sus claves constantes; no consulta información operativa de empresas.</summary>
    private async Task EnsureSecurityPermissionsAsync(CancellationToken cancellationToken)
    {
        foreach (var permission in SecurityPermissionCatalog.Definitions)
        {
            var entity = await dbContext.SecurityPermisos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(current => current.Id == permission.Id, cancellationToken);

            if (entity is null)
            {
                dbContext.SecurityPermisos.Add(new SecurityPermisoEntity
                {
                    Id = permission.Id,
                    NombrePermiso = permission.Id,
                    Descripcion = permission.Description,
                    Modulo = permission.Module
                });

                continue;
            }

            entity.NombrePermiso = permission.Id;
            entity.Descripcion = permission.Description;
            entity.Modulo = permission.Module;
        }
    }

    /// <summary>Inicializa relaciones del catálogo global rol-permiso; desactiva filtros solo en la rutina de arranque del sistema.</summary>
    private async Task EnsureSecurityRolePermissionsAsync(CancellationToken cancellationToken)
    {
        var roleIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
        {
            [SecurityRoleNames.SuperAdministrador] = SecuritySeedIds.SuperAdministradorRoleId,
            [SecurityRoleNames.AdministradorEmpresa] = SecuritySeedIds.AdministradorEmpresaRoleId,
            [SecurityRoleNames.Administrador] = SecuritySeedIds.AdministradorRoleId,
            [SecurityRoleNames.Gerente] = SecuritySeedIds.GerenteRoleId,
            [SecurityRoleNames.Cajero] = SecuritySeedIds.CajeroRoleId,
            [SecurityRoleNames.AsesorComercial] = SecuritySeedIds.AsesorComercialRoleId,
            [SecurityRoleNames.Bodeguero] = SecuritySeedIds.BodegueroRoleId,
            [SecurityRoleNames.Contador] = SecuritySeedIds.ContadorRoleId
        };

        foreach (var mapping in SecurityPermissionCatalog.RoleMappings)
        {
            if (!roleIds.TryGetValue(mapping.Key, out var roleId))
            {
                continue;
            }

            foreach (var permission in mapping.Value)
            {
                var exists = await dbContext.SecurityRolPermisos
                    .IgnoreQueryFilters()
                    .AnyAsync(current => current.RoleId == roleId && current.PermisoId == permission, cancellationToken);

                if (!exists)
                {
                    dbContext.SecurityRolPermisos.Add(new SecurityRolPermisoEntity
                    {
                        RoleId = roleId,
                        PermisoId = permission
                    });
                }
            }
        }
    }

    private AdminSeedCredentials ResolveAdminCredentials()
    {
        var configuredEmail = configuration["AdminSettings:Email"]?.Trim();
        var configuredPassword = configuration["AdminSettings:InitialPassword"];
        var environmentEmail = Environment.GetEnvironmentVariable("AdminSettings__Email");
        var environmentPassword = Environment.GetEnvironmentVariable("AdminSettings__InitialPassword");

        if (!environment.IsDevelopment() &&
            (string.IsNullOrWhiteSpace(environmentEmail) || string.IsNullOrWhiteSpace(environmentPassword)))
        {
            throw new InvalidOperationException(
                "Faltan AdminSettings__Email y/o AdminSettings__InitialPassword en las variables de entorno. Se cancela el arranque por seguridad.");
        }

        var email = configuredEmail;
        if (string.IsNullOrWhiteSpace(email))
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException("AdminSettings:Email debe estar configurado.");

            email = "admin@easymarket.local";
            logger.LogWarning("No se configuró AdminSettings:Email. Se usará el correo local de desarrollo {AdminEmail}; configura AdminSettings__Email para personalizarlo.", email);
        }

        if (!new EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException("AdminSettings:Email no contiene una dirección de correo válida.");

        if (string.IsNullOrWhiteSpace(configuredPassword) && !environment.IsDevelopment())
            throw new InvalidOperationException("AdminSettings:InitialPassword debe estar configurado.");

        if (!string.IsNullOrWhiteSpace(configuredPassword))
        {
            if (configuredPassword.Length is < 16 or > 128 || !configuredPassword.Any(char.IsUpper) ||
                !configuredPassword.Any(char.IsLower) || !configuredPassword.Any(char.IsDigit) ||
                !configuredPassword.Any(character => !char.IsLetterOrDigit(character)) || configuredPassword.Any(char.IsWhiteSpace))
            {
                throw new InvalidOperationException(
                    "AdminSettings:InitialPassword debe tener 16–128 caracteres, mayúscula, minúscula, número y símbolo, sin espacios.");
            }
        }

        return new AdminSeedCredentials(email, configuredPassword);
    }

    /// <summary>Seed del administrador inicial: los bypasses se limitan a la identidad de instalación y la empresa demo configurada.</summary>
    private async Task EnsureDefaultAdminUserAsync(AdminSeedCredentials credentials, CancellationToken cancellationToken)
    {
        var normalizedEmail = credentials.Email.ToUpperInvariant();
        var emailOwnedByAnotherTenant = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(current => current.NormalizedEmail == normalizedEmail && current.EmpresaId != DefaultEmpresaId && current.Id != DefaultAdminUserId, cancellationToken);
        if (emailOwnedByAnotherTenant)
        {
            throw new InvalidOperationException("El correo configurado para el administrador inicial ya pertenece a otra empresa.");
        }

        var user = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current =>
                current.Id == DefaultAdminUserId ||
                (current.EmpresaId == DefaultEmpresaId && current.NormalizedUserName == "ADMIN"),
                cancellationToken);

        if (user is null)
        {
            var initialPassword = credentials.InitialPassword;
            if (string.IsNullOrWhiteSpace(initialPassword))
            {
                if (!environment.IsDevelopment())
                    throw new InvalidOperationException("AdminSettings:InitialPassword es obligatorio para crear el administrador inicial.");

                initialPassword = $"{Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))}!aA1";
                logger.LogWarning(
                    "No se configuró AdminSettings:InitialPassword. Se generó una contraseña aleatoria para el administrador local. Configure AdminSettings__InitialPassword; contraseña temporal de desarrollo: {InitialPassword}",
                    initialPassword);
            }

            await EnsureAdminPersonaAsync(DefaultAdminPersonaId, credentials.Email, cancellationToken);

            user = new SecurityUserEntity
            {
                Id = DefaultAdminUserId,
                EmpresaId = DefaultEmpresaId,
                PersonaId = DefaultAdminPersonaId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                DisplayName = "Administrador del Sistema",
                Email = credentials.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = passwordHashService.Hash(initialPassword),
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
            user.Email = credentials.Email;
            user.NormalizedEmail = normalizedEmail;
            user.IsActive = true;
            user.BloqueadoManualmente = false;
        }

        await EnsureAdminPersonaAsync(user.PersonaId, credentials.Email, cancellationToken);
        await EnsureAdminRoleAssignmentAsync(user.Id, cancellationToken);
        await EnsureAdminCompanyAssignmentAsync(user.Id, cancellationToken);
    }

    /// <summary>Seed de persona del administrador inicial. La búsqueda global de email solo evita duplicidad y el registro resultante se asigna al tenant demo.</summary>
    private async Task EnsureAdminPersonaAsync(Guid personaId, string email, CancellationToken cancellationToken)
    {
        var persona = await dbContext.Personas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(current => current.Id == personaId ||
                (current.EmpresaId == DefaultEmpresaId && current.CorreoElectronicoPrincipal == email), cancellationToken);

        if (persona is not null)
        {
            persona.EmpresaId = DefaultEmpresaId;
            persona.RazonSocialONombresCompletos = "Administrador del Sistema";
            persona.CorreoElectronicoPrincipal = email;
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
            CorreoElectronicoPrincipal = email,
            IsActive = true,
            IsSystemRecord = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    /// <summary>Crea la asignación de rol del administrador seed identificado por su UserId estable; uso exclusivo de inicialización.</summary>
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

    /// <summary>Crea el vínculo del usuario seed únicamente con DefaultEmpresaId; bypass necesario durante el arranque sin tenant autenticado.</summary>
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

    /// <summary>Provisiona bodega/punto de emisión únicamente para DefaultEmpresaId; no reutilizar como operación de alta de tenants.</summary>
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
                Codigo = "001",
                Nombre = "Principal",
                Direccion = "Matriz principal",
                EsPrincipal = true,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.Bodegas.Add(bodega);
        }
        else
        {
            bodega.Codigo = string.IsNullOrWhiteSpace(bodega.Codigo) ? "001" : bodega.Codigo;
            bodega.EsPrincipal = true;
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

    /// <summary>Asigna puntos al usuario seed dentro de DefaultEmpresaId. El bypass es exclusivo de inicialización del sistema.</summary>
    private async Task EnsureAdminPointAssignmentAsync(CancellationToken cancellationToken)
    {
        var user = await dbContext.SecurityUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == DefaultAdminUserId, cancellationToken);

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

    private sealed record AdminSeedCredentials(string Email, string? InitialPassword);

    /// <summary>Siembra el plan de cuentas inicial de DefaultEmpresaId; la consulta que omite filtros queda acotada explícitamente a esa empresa.</summary>
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

        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "0", "Sin retencion IVA", "0%", 1, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "10", "Retencion IVA 10%", "10%", 2, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "20", "Retencion IVA 20%", "20%", 3, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "30", "Retencion IVA 30%", "30%", 4, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "50", "Retencion IVA 50%", "50%", 5, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "70", "Retencion IVA 70%", "70%", 6, cancellationToken);
        await EnsureCatalogItemAsync(RetencionIvaCatalogoId, "100", "Retencion IVA 100%", "100%", 7, cancellationToken);

        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "0", "Sin retencion renta", "0%", 1, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "312", "Retencion renta codigo 312", "1.75%", 2, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "320", "Retencion renta codigo 320", "1.75%", 3, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "322", "Retencion renta codigo 322", "1.75%", 4, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "332", "Bienes codigo 332", "1.75%", 5, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "343", "Servicios codigo 343", "2.75%", 6, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "344", "Servicios codigo 344", "2.75%", 7, cancellationToken);
        await EnsureCatalogItemAsync(RetencionRentaCatalogoId, "3440", "Retencion IVA codigo 3440", "70%", 8, cancellationToken);
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

    private async Task EnsureCatalogItemAsync(Guid catalogoId, string codigo, string nombre, string descripcion, int orden, CancellationToken cancellationToken)
    {
        var item = await dbContext.CatalogoItems
            .FirstOrDefaultAsync(current => current.CatalogoId == catalogoId && current.Codigo == codigo, cancellationToken);

        if (item is null)
        {
            dbContext.CatalogoItems.Add(new CatalogoItemEntity
            {
                Id = Guid.NewGuid(),
                CatalogoId = catalogoId,
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = descripcion,
                Orden = orden,
                IsActive = true
            });

            return;
        }

        item.Nombre = nombre;
        item.Descripcion = descripcion;
        item.Orden = orden;
        item.IsActive = true;
    }
}
