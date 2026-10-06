IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    CREATE TABLE [SecurityRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(80) NOT NULL,
        [NormalizedName] nvarchar(80) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SecurityRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    CREATE TABLE [SecurityUsers] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(80) NOT NULL,
        [NormalizedUserName] nvarchar(80) NOT NULL,
        [DisplayName] nvarchar(150) NOT NULL,
        [Email] nvarchar(180) NOT NULL,
        [NormalizedEmail] nvarchar(180) NOT NULL,
        [PasswordHash] nvarchar(128) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_SecurityUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    CREATE TABLE [SecurityUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_SecurityUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_SecurityUserRoles_SecurityRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SecurityRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SecurityUserRoles_SecurityUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [SecurityUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[SecurityRoles]'))
        SET IDENTITY_INSERT [SecurityRoles] ON;
    EXEC(N'INSERT INTO [SecurityRoles] ([Id], [IsActive], [Name], [NormalizedName])
    VALUES (''22222222-2222-2222-2222-222222222222'', CAST(1 AS bit), N''Administrador'', N''ADMINISTRADOR'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[SecurityRoles]'))
        SET IDENTITY_INSERT [SecurityRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DisplayName', N'Email', N'IsActive', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'UserName') AND [object_id] = OBJECT_ID(N'[SecurityUsers]'))
        SET IDENTITY_INSERT [SecurityUsers] ON;
    EXEC(N'INSERT INTO [SecurityUsers] ([Id], [CreatedAt], [DisplayName], [Email], [IsActive], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [UserName])
    VALUES (''11111111-1111-1111-1111-111111111111'', ''2026-06-17T00:00:00.0000000+00:00'', N''Administrador'', N''admin@testdeia.local'', CAST(1 AS bit), N''ADMIN@TESTDEIA.LOCAL'', N''ADMIN'', N''0A5BC3E342432F1BAD92FFD51B785343EC72906CDBA6A26131060B008E786656'', N''admin'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DisplayName', N'Email', N'IsActive', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'UserName') AND [object_id] = OBJECT_ID(N'[SecurityUsers]'))
        SET IDENTITY_INSERT [SecurityUsers] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[SecurityUserRoles]'))
        SET IDENTITY_INSERT [SecurityUserRoles] ON;
    EXEC(N'INSERT INTO [SecurityUserRoles] ([RoleId], [UserId])
    VALUES (''22222222-2222-2222-2222-222222222222'', ''11111111-1111-1111-1111-111111111111'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[SecurityUserRoles]'))
        SET IDENTITY_INSERT [SecurityUserRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityRoles_NormalizedName] ON [SecurityRoles] ([NormalizedName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    CREATE INDEX [IX_SecurityUserRoles_RoleId] ON [SecurityUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityUsers_NormalizedEmail] ON [SecurityUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityUsers_NormalizedUserName] ON [SecurityUsers] ([NormalizedUserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617053517_InitialSecuritySchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617053517_InitialSecuritySchema', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617061856_AddClientesModule'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] uniqueidentifier NOT NULL,
        [Identificacion] nvarchar(30) NOT NULL,
        [Nombres] nvarchar(120) NOT NULL,
        [Apellidos] nvarchar(120) NOT NULL,
        [Email] nvarchar(180) NULL,
        [Telefono] nvarchar(40) NULL,
        [Direccion] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617061856_AddClientesModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_Identificacion] ON [Clientes] ([Identificacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617061856_AddClientesModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617061856_AddClientesModule', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD [PersonaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    ALTER TABLE [Clientes] ADD [PersonaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    CREATE TABLE [Personas] (
        [Id] uniqueidentifier NOT NULL,
        [TipoIdentificacion] nvarchar(30) NOT NULL,
        [Identificacion] nvarchar(30) NOT NULL,
        [Nombres] nvarchar(120) NOT NULL,
        [Apellidos] nvarchar(120) NOT NULL,
        [FechaNacimiento] date NULL,
        [Email] nvarchar(180) NULL,
        [Telefono] nvarchar(40) NULL,
        [Direccion] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Personas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Apellidos', N'CreatedAt', N'Direccion', N'Email', N'FechaNacimiento', N'Identificacion', N'IsActive', N'Nombres', N'Telefono', N'TipoIdentificacion', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Personas]'))
        SET IDENTITY_INSERT [Personas] ON;
    EXEC(N'INSERT INTO [Personas] ([Id], [Apellidos], [CreatedAt], [Direccion], [Email], [FechaNacimiento], [Identificacion], [IsActive], [Nombres], [Telefono], [TipoIdentificacion], [UpdatedAt])
    VALUES (''33333333-3333-3333-3333-333333333333'', N''Sistema'', ''2026-06-18T00:00:00.0000000+00:00'', NULL, N''admin@testdeia.local'', NULL, N''ADMIN'', CAST(1 AS bit), N''Administrador'', NULL, N''Sistema'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Apellidos', N'CreatedAt', N'Direccion', N'Email', N'FechaNacimiento', N'Identificacion', N'IsActive', N'Nombres', N'Telefono', N'TipoIdentificacion', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Personas]'))
        SET IDENTITY_INSERT [Personas] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    EXEC(N'UPDATE [SecurityUsers] SET [PersonaId] = ''33333333-3333-3333-3333-333333333333''
    WHERE [Id] = ''11111111-1111-1111-1111-111111111111'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    CREATE INDEX [IX_SecurityUsers_PersonaId] ON [SecurityUsers] ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    CREATE INDEX [IX_Clientes_PersonaId] ON [Clientes] ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Personas_Identificacion] ON [Personas] ([Identificacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    ALTER TABLE [Clientes] ADD CONSTRAINT [FK_Clientes_Personas_PersonaId] FOREIGN KEY ([PersonaId]) REFERENCES [Personas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD CONSTRAINT [FK_SecurityUsers_Personas_PersonaId] FOREIGN KEY ([PersonaId]) REFERENCES [Personas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619022424_AddPersonasModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619022424_AddPersonasModule', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    INSERT INTO [Personas] (
        [Id],
        [TipoIdentificacion],
        [Identificacion],
        [Nombres],
        [Apellidos],
        [FechaNacimiento],
        [Email],
        [Telefono],
        [Direccion],
        [IsActive],
        [CreatedAt],
        [UpdatedAt])
    SELECT
        NEWID(),
        N'Cedula',
        [c].[Identificacion],
        [c].[Nombres],
        [c].[Apellidos],
        NULL,
        [c].[Email],
        [c].[Telefono],
        [c].[Direccion],
        [c].[IsActive],
        [c].[CreatedAt],
        [c].[UpdatedAt]
    FROM [Clientes] [c]
    WHERE [c].[PersonaId] IS NULL
      AND NOT EXISTS (
          SELECT 1
          FROM [Personas] [p]
          WHERE [p].[Identificacion] = [c].[Identificacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    UPDATE [c]
    SET [c].[PersonaId] = [p].[Id]
    FROM [Clientes] [c]
    INNER JOIN [Personas] [p] ON [p].[Identificacion] = [c].[Identificacion]
    WHERE [c].[PersonaId] IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DROP INDEX [IX_Clientes_Identificacion] ON [Clientes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DROP INDEX [IX_Clientes_PersonaId] ON [Clientes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Apellidos');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Clientes] DROP COLUMN [Apellidos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Direccion');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Clientes] DROP COLUMN [Direccion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Email');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Clientes] DROP COLUMN [Email];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Identificacion');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Clientes] DROP COLUMN [Identificacion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Nombres');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Clientes] DROP COLUMN [Nombres];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Telefono');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Clientes] DROP COLUMN [Telefono];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'PersonaId');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var6 + '];');
    EXEC(N'UPDATE [Clientes] SET [PersonaId] = ''00000000-0000-0000-0000-000000000000'' WHERE [PersonaId] IS NULL');
    ALTER TABLE [Clientes] ALTER COLUMN [PersonaId] uniqueidentifier NOT NULL;
    ALTER TABLE [Clientes] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [PersonaId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_PersonaId] ON [Clientes] ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619025829_NormalizeClientesWithPersonas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619025829_NormalizeClientesWithPersonas', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619030014_SyncSnapshotAfterClienteNormalization'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619030014_SyncSnapshotAfterClienteNormalization', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619031607_HideSystemPersonasFromCrm'
)
BEGIN
    ALTER TABLE [Personas] ADD [IsSystemRecord] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619031607_HideSystemPersonasFromCrm'
)
BEGIN
    EXEC(N'UPDATE [Personas] SET [IsSystemRecord] = CAST(1 AS bit)
    WHERE [Id] = ''33333333-3333-3333-3333-333333333333'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619031607_HideSystemPersonasFromCrm'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619031607_HideSystemPersonasFromCrm', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619034150_AddInventarioModule'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(40) NOT NULL,
        [Nombre] nvarchar(160) NOT NULL,
        [Descripcion] nvarchar(300) NULL,
        [CodigoIva] nvarchar(20) NOT NULL,
        [PorcentajeIva] decimal(9,2) NOT NULL,
        [StockActual] decimal(18,4) NOT NULL,
        [StockMinimo] decimal(18,4) NOT NULL,
        [CostoPromedio] decimal(18,6) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619034150_AddInventarioModule'
)
BEGIN
    CREATE TABLE [KardexMovimientos] (
        [Id] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [TipoMovimiento] nvarchar(30) NOT NULL,
        [Concepto] nvarchar(160) NOT NULL,
        [Referencia] nvarchar(80) NULL,
        [CantidadEntrada] decimal(18,4) NOT NULL,
        [CantidadSalida] decimal(18,4) NOT NULL,
        [SaldoCantidad] decimal(18,4) NOT NULL,
        [CostoUnitario] decimal(18,6) NOT NULL,
        [CostoPromedio] decimal(18,6) NOT NULL,
        [SaldoValor] decimal(18,6) NOT NULL,
        [FechaMovimiento] datetimeoffset NOT NULL,
        CONSTRAINT [PK_KardexMovimientos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KardexMovimientos_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619034150_AddInventarioModule'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_ProductoId_FechaMovimiento] ON [KardexMovimientos] ([ProductoId], [FechaMovimiento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619034150_AddInventarioModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [Productos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619034150_AddInventarioModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619034150_AddInventarioModule', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    ALTER TABLE [Productos] ADD [PrecioVenta] decimal(18,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    CREATE TABLE [Facturas] (
        [Id] uniqueidentifier NOT NULL,
        [Secuencial] bigint NOT NULL IDENTITY,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoEmision] nvarchar(3) NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [ClienteIdentificacion] nvarchar(20) NOT NULL,
        [ClienteNombre] nvarchar(200) NOT NULL,
        [FormaPago] nvarchar(60) NOT NULL,
        [Estado] nvarchar(30) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [SubtotalIva0] decimal(18,2) NOT NULL,
        [SubtotalIva5] decimal(18,2) NOT NULL,
        [SubtotalIva8] decimal(18,2) NOT NULL,
        [SubtotalIva15] decimal(18,2) NOT NULL,
        [IvaTotal] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Observacion] nvarchar(300) NULL,
        [ClaveAcceso] nvarchar(80) NULL,
        [NumeroAutorizacion] nvarchar(80) NULL,
        [MensajeEstado] nvarchar(400) NULL,
        [XmlFirmado] nvarchar(max) NULL,
        [FechaEmision] datetimeoffset NOT NULL,
        [FechaAutorizacion] datetimeoffset NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [ProcessingNode] nvarchar(100) NULL,
        [ProcessingStartedAt] datetimeoffset NULL,
        [RetryCount] int NOT NULL,
        [NextRetryAt] datetimeoffset NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Facturas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    CREATE TABLE [FacturaDetalles] (
        [Id] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [CodigoProducto] nvarchar(40) NOT NULL,
        [NombreProducto] nvarchar(160) NOT NULL,
        [CodigoIva] nvarchar(20) NOT NULL,
        [PorcentajeIva] decimal(9,2) NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,6) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [IvaValor] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_FacturaDetalles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacturaDetalles_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [Facturas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    CREATE TABLE [FacturaSriEventos] (
        [Id] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NOT NULL,
        [Estado] nvarchar(30) NOT NULL,
        [Mensaje] nvarchar(400) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_FacturaSriEventos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacturaSriEventos_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [Facturas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    CREATE INDEX [IX_FacturaDetalles_FacturaId] ON [FacturaDetalles] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Facturas_Establecimiento_PuntoEmision_Secuencial] ON [Facturas] ([Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    CREATE INDEX [IX_Facturas_Estado_CreatedAt] ON [Facturas] ([Estado], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    CREATE INDEX [IX_FacturaSriEventos_FacturaId_CreatedAt] ON [FacturaSriEventos] ([FacturaId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622030129_AddFacturacionElectronica'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622030129_AddFacturacionElectronica', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [AgenteRetencionResolucion] nvarchar(60) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [AmbienteSri] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [ContribuyenteEspecial] nvarchar(40) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [DireccionEstablecimientoEmisor] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [DireccionMatrizEmisor] nvarchar(300) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [EmpresaEmisoraId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [NombreComercialEmisor] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [ObligadoContabilidad] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [RazonSocialEmisor] nvarchar(300) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [RegimenRimpe] nvarchar(60) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [RucEmisor] nvarchar(13) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    ALTER TABLE [Facturas] ADD [TipoEmision] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    CREATE TABLE [EmpresasEmisoras] (
        [Id] uniqueidentifier NOT NULL,
        [RazonSocial] nvarchar(300) NOT NULL,
        [NombreComercial] nvarchar(300) NULL,
        [Ruc] nvarchar(13) NOT NULL,
        [DireccionMatriz] nvarchar(300) NOT NULL,
        [DireccionEstablecimiento] nvarchar(300) NULL,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoEmision] nvarchar(3) NOT NULL,
        [AmbienteSri] nvarchar(20) NOT NULL,
        [ModoDesarrollo] bit NOT NULL,
        [TipoEmision] nvarchar(20) NOT NULL,
        [ObligadoContabilidad] bit NOT NULL,
        [ContribuyenteEspecial] nvarchar(40) NULL,
        [RegimenRimpe] nvarchar(60) NULL,
        [AgenteRetencionResolucion] nvarchar(60) NULL,
        [CertificadoRuta] nvarchar(500) NULL,
        [CertificadoClave] nvarchar(200) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_EmpresasEmisoras] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmpresasEmisoras_Ruc] ON [EmpresasEmisoras] ([Ruc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622031831_AddEmpresaEmisoraConfig'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622031831_AddEmpresaEmisoraConfig', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622034354_StoreEmpresaCertificateInDb'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EmpresasEmisoras]') AND [c].[name] = N'CertificadoRuta');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [EmpresasEmisoras] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [EmpresasEmisoras] DROP COLUMN [CertificadoRuta];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622034354_StoreEmpresaCertificateInDb'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [CertificadoContenido] varbinary(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622034354_StoreEmpresaCertificateInDb'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [CertificadoNombreArchivo] nvarchar(260) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622034354_StoreEmpresaCertificateInDb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622034354_StoreEmpresaCertificateInDb', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Facturas]') AND [c].[name] = N'ClienteNombre');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Facturas] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Facturas] ALTER COLUMN [ClienteNombre] nvarchar(300) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    ALTER TABLE [Facturas] ADD [ClienteDireccion] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    ALTER TABLE [Facturas] ADD [ClienteEmail] nvarchar(180) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    ALTER TABLE [Facturas] ADD [ClienteTelefono] nvarchar(40) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    ALTER TABLE [Facturas] ADD [ClienteTipoIdentificacion] nvarchar(2) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    ALTER TABLE [Facturas] ADD [FormaPagoSriCodigo] nvarchar(2) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    ALTER TABLE [Facturas] ADD [TotalDescuento] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    UPDATE facturas
    SET facturas.ClienteTipoIdentificacion =
        CASE UPPER(LTRIM(RTRIM(personas.TipoIdentificacion)))
            WHEN 'RUC' THEN '04'
            WHEN 'CEDULA' THEN '05'
            WHEN 'CÉDULA' THEN '05'
            WHEN 'PASAPORTE' THEN '06'
            WHEN 'CONSUMIDOR FINAL' THEN '07'
            WHEN 'IDENTIFICACION DEL EXTERIOR' THEN '08'
            WHEN 'IDENTIFICACIÓN DEL EXTERIOR' THEN '08'
            WHEN 'PLACA' THEN '09'
            ELSE '05'
        END
    FROM Facturas facturas
    INNER JOIN Clientes clientes ON clientes.Id = facturas.ClienteId
    INNER JOIN Personas personas ON personas.Id = clientes.PersonaId
    WHERE facturas.ClienteTipoIdentificacion = '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    UPDATE Facturas
    SET FormaPagoSriCodigo =
        CASE UPPER(LTRIM(RTRIM(FormaPago)))
            WHEN 'EFECTIVO' THEN '01'
            WHEN 'COMPENSACION' THEN '15'
            WHEN 'COMPENSACIÓN' THEN '15'
            WHEN 'TARJETA DE DEBITO' THEN '16'
            WHEN 'TARJETA DE DÉBITO' THEN '16'
            WHEN 'DINERO ELECTRONICO' THEN '17'
            WHEN 'DINERO ELECTRÓNICO' THEN '17'
            WHEN 'TARJETA PREPAGO' THEN '18'
            WHEN 'TARJETA' THEN '19'
            WHEN 'TARJETA DE CREDITO' THEN '19'
            WHEN 'TARJETA DE CRÉDITO' THEN '19'
            WHEN 'TRANSFERENCIA' THEN '20'
            WHEN 'OTROS CON UTILIZACION DEL SISTEMA FINANCIERO' THEN '20'
            WHEN 'OTROS CON UTILIZACIÓN DEL SISTEMA FINANCIERO' THEN '20'
            WHEN 'ENDOSO DE TITULOS' THEN '21'
            WHEN 'ENDOSO DE TÍTULOS' THEN '21'
            ELSE '01'
        END
    WHERE FormaPagoSriCodigo = '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622041421_RefineSriInvoiceData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622041421_RefineSriInvoiceData', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    ALTER TABLE [Personas] ADD [EstadoCivil] nvarchar(30) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    CREATE TABLE [Catalogos] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Descripcion] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Catalogos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    CREATE TABLE [CatalogoItems] (
        [Id] uniqueidentifier NOT NULL,
        [CatalogoId] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Descripcion] nvarchar(250) NULL,
        [Orden] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CatalogoItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CatalogoItems_Catalogos_CatalogoId] FOREIGN KEY ([CatalogoId]) REFERENCES [Catalogos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Codigo', N'Descripcion', N'IsActive', N'Nombre') AND [object_id] = OBJECT_ID(N'[Catalogos]'))
        SET IDENTITY_INSERT [Catalogos] ON;
    EXEC(N'INSERT INTO [Catalogos] ([Id], [Codigo], [Descripcion], [IsActive], [Nombre])
    VALUES (''70000000-0000-0000-0000-000000000001'', N''TIPO_IDENTIFICACION'', N''Tipos base de identificacion para personas y clientes.'', CAST(1 AS bit), N''Tipo de identificacion''),
    (''70000000-0000-0000-0000-000000000002'', N''ESTADO_CIVIL'', N''Estado civil de personas.'', CAST(1 AS bit), N''Estado civil''),
    (''70000000-0000-0000-0000-000000000003'', N''ESTADO_REGISTRO'', N''Estados funcionales de registros activos e inactivos.'', CAST(1 AS bit), N''Estado de registro''),
    (''70000000-0000-0000-0000-000000000004'', N''ESTADO_DOCUMENTO_ELECTRONICO'', N''Estados de comprobantes electronicos.'', CAST(1 AS bit), N''Estado documento electronico''),
    (''70000000-0000-0000-0000-000000000005'', N''AMBIENTE_SRI'', N''Ambiente de emision para comprobantes electronicos.'', CAST(1 AS bit), N''Ambiente SRI''),
    (''70000000-0000-0000-0000-000000000006'', N''TIPO_EMISION'', N''Tipo de emision de documentos electronicos.'', CAST(1 AS bit), N''Tipo de emision''),
    (''70000000-0000-0000-0000-000000000007'', N''FORMA_PAGO_SRI'', N''Formas de pago segun catalogo del SRI.'', CAST(1 AS bit), N''Forma de pago SRI'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Codigo', N'Descripcion', N'IsActive', N'Nombre') AND [object_id] = OBJECT_ID(N'[Catalogos]'))
        SET IDENTITY_INSERT [Catalogos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    EXEC(N'UPDATE [Personas] SET [EstadoCivil] = NULL
    WHERE [Id] = ''33333333-3333-3333-3333-333333333333'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CatalogoId', N'Codigo', N'Descripcion', N'IsActive', N'Nombre', N'Orden') AND [object_id] = OBJECT_ID(N'[CatalogoItems]'))
        SET IDENTITY_INSERT [CatalogoItems] ON;
    EXEC(N'INSERT INTO [CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden])
    VALUES (''71000000-0000-0000-0000-000000000001'', ''70000000-0000-0000-0000-000000000001'', N''RUC'', NULL, CAST(1 AS bit), N''RUC'', 1),
    (''71000000-0000-0000-0000-000000000002'', ''70000000-0000-0000-0000-000000000001'', N''Cedula'', NULL, CAST(1 AS bit), N''Cedula'', 2),
    (''71000000-0000-0000-0000-000000000003'', ''70000000-0000-0000-0000-000000000001'', N''Pasaporte'', NULL, CAST(1 AS bit), N''Pasaporte'', 3),
    (''71000000-0000-0000-0000-000000000004'', ''70000000-0000-0000-0000-000000000001'', N''Consumidor Final'', NULL, CAST(1 AS bit), N''Consumidor final'', 4),
    (''71000000-0000-0000-0000-000000000005'', ''70000000-0000-0000-0000-000000000001'', N''Identificacion del Exterior'', NULL, CAST(1 AS bit), N''Identificacion del exterior'', 5),
    (''71000000-0000-0000-0000-000000000006'', ''70000000-0000-0000-0000-000000000001'', N''Placa'', NULL, CAST(1 AS bit), N''Placa'', 6),
    (''71000000-0000-0000-0000-000000000011'', ''70000000-0000-0000-0000-000000000002'', N''SOLTERO'', NULL, CAST(1 AS bit), N''Soltero'', 1),
    (''71000000-0000-0000-0000-000000000012'', ''70000000-0000-0000-0000-000000000002'', N''CASADO'', NULL, CAST(1 AS bit), N''Casado'', 2),
    (''71000000-0000-0000-0000-000000000013'', ''70000000-0000-0000-0000-000000000002'', N''DIVORCIADO'', NULL, CAST(1 AS bit), N''Divorciado'', 3),
    (''71000000-0000-0000-0000-000000000014'', ''70000000-0000-0000-0000-000000000002'', N''VIUDO'', NULL, CAST(1 AS bit), N''Viudo'', 4),
    (''71000000-0000-0000-0000-000000000015'', ''70000000-0000-0000-0000-000000000002'', N''UNION_LIBRE'', NULL, CAST(1 AS bit), N''Union libre'', 5),
    (''71000000-0000-0000-0000-000000000021'', ''70000000-0000-0000-0000-000000000003'', N''ACTIVO'', NULL, CAST(1 AS bit), N''Activo'', 1),
    (''71000000-0000-0000-0000-000000000022'', ''70000000-0000-0000-0000-000000000003'', N''INACTIVO'', NULL, CAST(1 AS bit), N''Inactivo'', 2),
    (''71000000-0000-0000-0000-000000000031'', ''70000000-0000-0000-0000-000000000004'', N''Pendiente'', NULL, CAST(1 AS bit), N''Pendiente'', 1),
    (''71000000-0000-0000-0000-000000000032'', ''70000000-0000-0000-0000-000000000004'', N''EnProceso'', NULL, CAST(1 AS bit), N''En proceso'', 2),
    (''71000000-0000-0000-0000-000000000033'', ''70000000-0000-0000-0000-000000000004'', N''Recibido'', NULL, CAST(1 AS bit), N''Recibido'', 3),
    (''71000000-0000-0000-0000-000000000034'', ''70000000-0000-0000-0000-000000000004'', N''Autorizado'', NULL, CAST(1 AS bit), N''Autorizado'', 4),
    (''71000000-0000-0000-0000-000000000035'', ''70000000-0000-0000-0000-000000000004'', N''Rechazado'', NULL, CAST(1 AS bit), N''Rechazado'', 5),
    (''71000000-0000-0000-0000-000000000036'', ''70000000-0000-0000-0000-000000000004'', N''Error'', NULL, CAST(1 AS bit), N''Error'', 6),
    (''71000000-0000-0000-0000-000000000041'', ''70000000-0000-0000-0000-000000000005'', N''Pruebas'', N''Codigo SRI 1'', CAST(1 AS bit), N''Pruebas'', 1),
    (''71000000-0000-0000-0000-000000000042'', ''70000000-0000-0000-0000-000000000005'', N''Produccion'', N''Codigo SRI 2'', CAST(1 AS bit), N''Produccion'', 2),
    (''71000000-0000-0000-0000-000000000051'', ''70000000-0000-0000-0000-000000000006'', N''Normal'', N''Codigo SRI 1'', CAST(1 AS bit), N''Normal'', 1),
    (''71000000-0000-0000-0000-000000000061'', ''70000000-0000-0000-0000-000000000007'', N''Efectivo'', N''Codigo SRI 01'', CAST(1 AS bit), N''Efectivo'', 1),
    (''71000000-0000-0000-0000-000000000062'', ''70000000-0000-0000-0000-000000000007'', N''Compensacion'', N''Codigo SRI 15'', CAST(1 AS bit), N''Compensacion'', 2),
    (''71000000-0000-0000-0000-000000000063'', ''70000000-0000-0000-0000-000000000007'', N''Tarjeta de debito'', N''Codigo SRI 16'', CAST(1 AS bit), N''Tarjeta de debito'', 3),
    (''71000000-0000-0000-0000-000000000064'', ''70000000-0000-0000-0000-000000000007'', N''Dinero electronico'', N''Codigo SRI 17'', CAST(1 AS bit), N''Dinero electronico'', 4),
    (''71000000-0000-0000-0000-000000000065'', ''70000000-0000-0000-0000-000000000007'', N''Tarjeta prepago'', N''Codigo SRI 18'', CAST(1 AS bit), N''Tarjeta prepago'', 5),
    (''71000000-0000-0000-0000-000000000066'', ''70000000-0000-0000-0000-000000000007'', N''Tarjeta de credito'', N''Codigo SRI 19'', CAST(1 AS bit), N''Tarjeta de credito'', 6),
    (''71000000-0000-0000-0000-000000000067'', ''70000000-0000-0000-0000-000000000007'', N''Transferencia'', N''Codigo SRI 20'', CAST(1 AS bit), N''Transferencia'', 7),
    (''71000000-0000-0000-0000-000000000068'', ''70000000-0000-0000-0000-000000000007'', N''Endoso de titulos'', N''Codigo SRI 21'', CAST(1 AS bit), N''Endoso de titulos'', 8)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CatalogoId', N'Codigo', N'Descripcion', N'IsActive', N'Nombre', N'Orden') AND [object_id] = OBJECT_ID(N'[CatalogoItems]'))
        SET IDENTITY_INSERT [CatalogoItems] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CatalogoItems_CatalogoId_Codigo] ON [CatalogoItems] ([CatalogoId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Catalogos_Codigo] ON [Catalogos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623044931_AddCatalogosModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260623044931_AddCatalogosModule', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623054240_RestructurePersonaRolesAndEmployees'
)
BEGIN
    DROP INDEX [IX_SecurityUsers_PersonaId] ON [SecurityUsers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623054240_RestructurePersonaRolesAndEmployees'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SecurityUsers]') AND [c].[name] = N'PersonaId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [SecurityUsers] DROP CONSTRAINT [' + @var9 + '];');
    EXEC(N'UPDATE [SecurityUsers] SET [PersonaId] = ''00000000-0000-0000-0000-000000000000'' WHERE [PersonaId] IS NULL');
    ALTER TABLE [SecurityUsers] ALTER COLUMN [PersonaId] uniqueidentifier NOT NULL;
    ALTER TABLE [SecurityUsers] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [PersonaId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623054240_RestructurePersonaRolesAndEmployees'
)
BEGIN
    CREATE TABLE [Empleados] (
        [Id] uniqueidentifier NOT NULL,
        [PersonaId] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Empleados] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Empleados_Personas_PersonaId] FOREIGN KEY ([PersonaId]) REFERENCES [Personas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623054240_RestructurePersonaRolesAndEmployees'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityUsers_PersonaId] ON [SecurityUsers] ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623054240_RestructurePersonaRolesAndEmployees'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Empleados_PersonaId] ON [Empleados] ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623054240_RestructurePersonaRolesAndEmployees'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260623054240_RestructurePersonaRolesAndEmployees', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    DROP INDEX [IX_SecurityUsers_NormalizedEmail] ON [SecurityUsers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    DROP INDEX [IX_SecurityUsers_NormalizedUserName] ON [SecurityUsers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    DROP INDEX [IX_Productos_Codigo] ON [Productos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    DROP INDEX [IX_Personas_Identificacion] ON [Personas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [Productos] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [Personas] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [Facturas] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [OwnerUserId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [Empleados] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    ALTER TABLE [Clientes] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE TABLE [SecurityUserEmpresas] (
        [SecurityUserId] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [IsDefault] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_SecurityUserEmpresas] PRIMARY KEY ([SecurityUserId], [EmpresaId]),
        CONSTRAINT [FK_SecurityUserEmpresas_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SecurityUserEmpresas_SecurityUsers_SecurityUserId] FOREIGN KEY ([SecurityUserId]) REFERENCES [SecurityUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AgenteRetencionResolucion', N'AmbienteSri', N'CertificadoClave', N'CertificadoContenido', N'CertificadoNombreArchivo', N'ContribuyenteEspecial', N'CreatedAt', N'DireccionEstablecimiento', N'DireccionMatriz', N'Establecimiento', N'IsActive', N'ModoDesarrollo', N'NombreComercial', N'ObligadoContabilidad', N'OwnerUserId', N'PuntoEmision', N'RazonSocial', N'RegimenRimpe', N'Ruc', N'TipoEmision', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[EmpresasEmisoras]'))
        SET IDENTITY_INSERT [EmpresasEmisoras] ON;
    EXEC(N'INSERT INTO [EmpresasEmisoras] ([Id], [AgenteRetencionResolucion], [AmbienteSri], [CertificadoClave], [CertificadoContenido], [CertificadoNombreArchivo], [ContribuyenteEspecial], [CreatedAt], [DireccionEstablecimiento], [DireccionMatriz], [Establecimiento], [IsActive], [ModoDesarrollo], [NombreComercial], [ObligadoContabilidad], [OwnerUserId], [PuntoEmision], [RazonSocial], [RegimenRimpe], [Ruc], [TipoEmision], [UpdatedAt])
    VALUES (''22222222-2222-2222-2222-222222222222'', NULL, N''Pruebas'', NULL, NULL, NULL, NULL, ''2026-06-23T00:00:00.0000000+00:00'', N''Sucursal demo'', N''Matriz demo'', N''001'', CAST(1 AS bit), CAST(1 AS bit), N''EasyMarket Demo'', CAST(0 AS bit), ''11111111-1111-1111-1111-111111111111'', N''001'', N''EasyMarket Demo S.A.'', NULL, N''0999999999001'', N''Normal'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AgenteRetencionResolucion', N'AmbienteSri', N'CertificadoClave', N'CertificadoContenido', N'CertificadoNombreArchivo', N'ContribuyenteEspecial', N'CreatedAt', N'DireccionEstablecimiento', N'DireccionMatriz', N'Establecimiento', N'IsActive', N'ModoDesarrollo', N'NombreComercial', N'ObligadoContabilidad', N'OwnerUserId', N'PuntoEmision', N'RazonSocial', N'RegimenRimpe', N'Ruc', N'TipoEmision', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[EmpresasEmisoras]'))
        SET IDENTITY_INSERT [EmpresasEmisoras] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    EXEC(N'UPDATE [Personas] SET [EmpresaId] = ''22222222-2222-2222-2222-222222222222''
    WHERE [Id] = ''33333333-3333-3333-3333-333333333333'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    EXEC(N'UPDATE [SecurityUsers] SET [EmpresaId] = ''22222222-2222-2222-2222-222222222222''
    WHERE [Id] = ''11111111-1111-1111-1111-111111111111'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmpresaId', N'SecurityUserId', N'CreatedAt', N'IsDefault') AND [object_id] = OBJECT_ID(N'[SecurityUserEmpresas]'))
        SET IDENTITY_INSERT [SecurityUserEmpresas] ON;
    EXEC(N'INSERT INTO [SecurityUserEmpresas] ([EmpresaId], [SecurityUserId], [CreatedAt], [IsDefault])
    VALUES (''22222222-2222-2222-2222-222222222222'', ''11111111-1111-1111-1111-111111111111'', ''2026-06-23T00:00:00.0000000+00:00'', CAST(1 AS bit))');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EmpresaId', N'SecurityUserId', N'CreatedAt', N'IsDefault') AND [object_id] = OBJECT_ID(N'[SecurityUserEmpresas]'))
        SET IDENTITY_INSERT [SecurityUserEmpresas] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityUsers_EmpresaId_NormalizedEmail] ON [SecurityUsers] ([EmpresaId], [NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityUsers_EmpresaId_NormalizedUserName] ON [SecurityUsers] ([EmpresaId], [NormalizedUserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityUsers_EmpresaId_PersonaId] ON [SecurityUsers] ([EmpresaId], [PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_EmpresaId_Codigo] ON [Productos] ([EmpresaId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Personas_EmpresaId_Identificacion] ON [Personas] ([EmpresaId], [Identificacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmpresasEmisoras_OwnerUserId_Ruc] ON [EmpresasEmisoras] ([OwnerUserId], [Ruc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Empleados_EmpresaId_PersonaId] ON [Empleados] ([EmpresaId], [PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_EmpresaId_PersonaId] ON [Clientes] ([EmpresaId], [PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    CREATE INDEX [IX_SecurityUserEmpresas_EmpresaId] ON [SecurityUserEmpresas] ([EmpresaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624024637_AddMultiEmpresaSaasSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260624024637_AddMultiEmpresaSaasSupport', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624035440_RefineFacturaXmlLifecycleAndReporting'
)
BEGIN
    ALTER TABLE [Facturas] ADD [XmlGenerado] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624035440_RefineFacturaXmlLifecycleAndReporting'
)
BEGIN
    ;WITH LegacyFacturas AS (
        SELECT
            Id,
            ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS RowNumber
        FROM Facturas
        WHERE ClaveAcceso IS NULL OR LTRIM(RTRIM(ClaveAcceso)) = ''
    )
    UPDATE Facturas
    SET ClaveAcceso = RIGHT(REPLICATE('0', 49) + CAST(LegacyFacturas.RowNumber AS varchar(49)), 49)
    FROM Facturas
    INNER JOIN LegacyFacturas ON LegacyFacturas.Id = Facturas.Id;

    UPDATE Facturas
    SET XmlGenerado = COALESCE(XmlGenerado, XmlFirmado)
    WHERE XmlGenerado IS NULL AND XmlFirmado IS NOT NULL;

    UPDATE Facturas
    SET Estado = CASE
        WHEN UPPER(Estado) = 'AUTORIZADO' THEN 'AUTORIZADO'
        WHEN UPPER(Estado) = 'RECHAZADO' THEN 'RECHAZADO'
        WHEN XmlFirmado IS NOT NULL THEN 'PENDIENTE'
        ELSE 'NO_FIRMADO'
    END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624035440_RefineFacturaXmlLifecycleAndReporting'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Facturas]') AND [c].[name] = N'ClaveAcceso');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Facturas] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Facturas] ALTER COLUMN [ClaveAcceso] nvarchar(49) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624035440_RefineFacturaXmlLifecycleAndReporting'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Facturas_ClaveAcceso] ON [Facturas] ([ClaveAcceso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260624035440_RefineFacturaXmlLifecycleAndReporting'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260624035440_RefineFacturaXmlLifecycleAndReporting', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625033712_HardenFacturaStateAndEmpresaRideLogo'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [LogoRideContenido] varbinary(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625033712_HardenFacturaStateAndEmpresaRideLogo'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [LogoRideMimeType] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625033712_HardenFacturaStateAndEmpresaRideLogo'
)
BEGIN
    EXEC(N'UPDATE [EmpresasEmisoras] SET [LogoRideContenido] = NULL, [LogoRideMimeType] = NULL
    WHERE [Id] = ''22222222-2222-2222-2222-222222222222'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625033712_HardenFacturaStateAndEmpresaRideLogo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260625033712_HardenFacturaStateAndEmpresaRideLogo', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625045302_AddTransactionalInventoryConcurrency'
)
BEGIN
    ALTER TABLE [Productos] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625045302_AddTransactionalInventoryConcurrency'
)
BEGIN
    ALTER TABLE [Facturas] ADD [InventarioAplicado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625045302_AddTransactionalInventoryConcurrency'
)
BEGIN
    ALTER TABLE [Facturas] ADD [InventarioAplicadoAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625045302_AddTransactionalInventoryConcurrency'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260625045302_AddTransactionalInventoryConcurrency', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625051237_RemoveFacturaRowVersion'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Facturas]') AND [c].[name] = N'RowVersion');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Facturas] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Facturas] DROP COLUMN [RowVersion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260625051237_RemoveFacturaRowVersion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260625051237_RemoveFacturaRowVersion', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626032541_AddProductoControlaStock'
)
BEGIN
    ALTER TABLE [Productos] ADD [ControlaStock] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626032541_AddProductoControlaStock'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260626032541_AddProductoControlaStock', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626050659_AddEmpresaPuntosEmision'
)
BEGIN
    CREATE TABLE [EmpresaPuntosEmision] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaEmisoraId] uniqueidentifier NOT NULL,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoEmision] nvarchar(3) NOT NULL,
        [DireccionEstablecimiento] nvarchar(300) NULL,
        [IsDefault] bit NOT NULL,
        CONSTRAINT [PK_EmpresaPuntosEmision] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmpresaPuntosEmision_EmpresasEmisoras_EmpresaEmisoraId] FOREIGN KEY ([EmpresaEmisoraId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626050659_AddEmpresaPuntosEmision'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmpresaPuntosEmision_EmpresaEmisoraId_Establecimiento_PuntoEmision] ON [EmpresaPuntosEmision] ([EmpresaEmisoraId], [Establecimiento], [PuntoEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626050659_AddEmpresaPuntosEmision'
)
BEGIN
    INSERT INTO EmpresaPuntosEmision (Id, EmpresaEmisoraId, Establecimiento, PuntoEmision, DireccionEstablecimiento, IsDefault)
    SELECT NEWID(), Id, Establecimiento, PuntoEmision, DireccionEstablecimiento, CAST(1 AS bit)
    FROM EmpresasEmisoras
    WHERE NOT EXISTS (
        SELECT 1
        FROM EmpresaPuntosEmision puntos
        WHERE puntos.EmpresaEmisoraId = EmpresasEmisoras.Id
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260626050659_AddEmpresaPuntosEmision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260626050659_AddEmpresaPuntosEmision', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    CREATE TABLE [FacturaSecuenciales] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoEmision] nvarchar(3) NOT NULL,
        [UltimoSecuencial] bigint NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_FacturaSecuenciales] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FacturaSecuenciales_EmpresaId_Establecimiento_PuntoEmision] ON [FacturaSecuenciales] ([EmpresaId], [Establecimiento], [PuntoEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    INSERT INTO FacturaSecuenciales (Id, EmpresaId, Establecimiento, PuntoEmision, UltimoSecuencial, CreatedAt, UpdatedAt)
    SELECT NEWID(), EmpresaId, Establecimiento, PuntoEmision, MAX(Secuencial), SYSUTCDATETIME(), SYSUTCDATETIME()
    FROM Facturas
    GROUP BY EmpresaId, Establecimiento, PuntoEmision;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    DROP INDEX [IX_Facturas_Establecimiento_PuntoEmision_Secuencial] ON [Facturas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    ALTER TABLE [Facturas] ADD [SecuencialTmp] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    UPDATE Facturas
    SET SecuencialTmp = Secuencial;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Facturas]') AND [c].[name] = N'Secuencial');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Facturas] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Facturas] DROP COLUMN [Secuencial];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    EXEC sp_rename N'[Facturas].[SecuencialTmp]', N'Secuencial', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Facturas]') AND [c].[name] = N'Secuencial');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Facturas] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Facturas] ALTER COLUMN [Secuencial] bigint NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Facturas_Establecimiento_PuntoEmision_Secuencial] ON [Facturas] ([Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630024155_AddPosOperationalContextAndPointSequences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260630024155_AddPosOperationalContextAndPointSequences', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630035147_AddFacturaDetalleDiscountAndSriSigningQueue'
)
BEGIN
    ALTER TABLE [FacturaDetalles] ADD [Descuento] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630035147_AddFacturaDetalleDiscountAndSriSigningQueue'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260630035147_AddFacturaDetalleDiscountAndSriSigningQueue', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707041729_AddEmpleadoLaborFields'
)
BEGIN
    ALTER TABLE [Empleados] ADD [Cargo] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707041729_AddEmpleadoLaborFields'
)
BEGIN
    ALTER TABLE [Empleados] ADD [FechaIngreso] date NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707041729_AddEmpleadoLaborFields'
)
BEGIN
    ALTER TABLE [Empleados] ADD [PerfilLaboral] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707041729_AddEmpleadoLaborFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707041729_AddEmpleadoLaborFields', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Personas] ADD [DireccionPrincipal] nvarchar(250) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Personas] ADD [NombreComercial] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Personas] ADD [RazonSocialONombresCompletos] nvarchar(180) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Personas] ADD [Genero] nvarchar(30) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] DROP CONSTRAINT [PK_Empleados];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DROP INDEX [IX_Empleados_PersonaId] ON [Empleados];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] DROP CONSTRAINT [PK_Clientes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DROP INDEX [IX_Clientes_PersonaId] ON [Clientes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC sp_rename N'[Personas].[Telefono]', N'TelefonoCelular', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC sp_rename N'[Personas].[Email]', N'CorreoElectronicoPrincipal', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC sp_rename N'[Empleados].[PerfilLaboral]', N'CargoPuesto', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [UsuarioCreacionId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [UsuarioCreacionId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    UPDATE Personas
    SET TipoIdentificacion = CASE UPPER(COALESCE(TipoIdentificacion, ''))
        WHEN 'RUC' THEN '04'
        WHEN 'CEDULA' THEN '05'
        WHEN 'CÉDULA' THEN '05'
        WHEN 'PASAPORTE' THEN '06'
        WHEN 'CONSUMIDOR FINAL' THEN '07'
        WHEN 'IDENTIFICACION DEL EXTERIOR' THEN '08'
        WHEN 'IDENTIFICACIÓN DEL EXTERIOR' THEN '08'
        WHEN 'PLACA' THEN '09'
        WHEN '04' THEN '04'
        WHEN '05' THEN '05'
        WHEN '06' THEN '06'
        WHEN '07' THEN '07'
        WHEN '08' THEN '08'
        WHEN '09' THEN '09'
        ELSE '05'
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Personas]') AND [c].[name] = N'TipoIdentificacion');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Personas] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Personas] ALTER COLUMN [TipoIdentificacion] nvarchar(2) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [CodigoBiometrico] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [CodigoEmpleado] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [EstadoLaboral] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [FechaSalida] date NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [NombreContactoEmergencia] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [PorcentajeComisionVentas] decimal(5,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [SueldoBase] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [TelefonoEmergencia] nvarchar(40) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [TipoContrato] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD [UsuarioModificacionId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [CorreoFacturacionElectronica] nvarchar(180) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [DiasCreditoMaximo] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [EsContribuyenteEspecial] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [EstadoCredito] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [LimiteCredito] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [ObligadoContabilidad] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [PermiteCredito] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [TipoCliente] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD [UsuarioModificacionId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    UPDATE Personas
    SET RazonSocialONombresCompletos = LTRIM(RTRIM(
            COALESCE(NULLIF(Nombres, ''), '') +
            CASE
                WHEN Nombres IS NOT NULL AND Nombres <> '' AND Apellidos IS NOT NULL AND Apellidos <> '' THEN ' '
                ELSE ''
            END +
            COALESCE(NULLIF(Apellidos, ''), '')
        )),
        DireccionPrincipal = COALESCE(NULLIF(Direccion, ''), 'Sin direccion registrada'),
        TipoIdentificacion = CASE UPPER(TipoIdentificacion)
            WHEN 'RUC' THEN '04'
            WHEN 'CEDULA' THEN '05'
            WHEN 'CÉDULA' THEN '05'
            WHEN 'PASAPORTE' THEN '06'
            WHEN 'CONSUMIDOR FINAL' THEN '07'
            WHEN 'IDENTIFICACION DEL EXTERIOR' THEN '08'
            WHEN 'IDENTIFICACIÓN DEL EXTERIOR' THEN '08'
            WHEN 'PLACA' THEN '09'
            WHEN '04' THEN '04'
            WHEN '05' THEN '05'
            WHEN '06' THEN '06'
            WHEN '07' THEN '07'
            WHEN '08' THEN '08'
            WHEN '09' THEN '09'
            ELSE '05'
        END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    UPDATE Clientes
    SET UsuarioCreacionId = '33333333-3333-3333-3333-333333333333',
        TipoCliente = 'Natural',
        EstadoCredito = 'Normal'
    WHERE UsuarioCreacionId = '00000000-0000-0000-0000-000000000000'
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    UPDATE Empleados
    SET UsuarioCreacionId = '33333333-3333-3333-3333-333333333333',
        TipoContrato = 'Indefinido',
        EstadoLaboral = 'Activo'
    WHERE UsuarioCreacionId = '00000000-0000-0000-0000-000000000000'
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Personas]') AND [c].[name] = N'Apellidos');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Personas] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Personas] DROP COLUMN [Apellidos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Personas]') AND [c].[name] = N'Direccion');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Personas] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [Personas] DROP COLUMN [Direccion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Personas]') AND [c].[name] = N'Nombres');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Personas] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [Personas] DROP COLUMN [Nombres];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Personas]') AND [c].[name] = N'EstadoCivil');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Personas] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [Personas] DROP COLUMN [EstadoCivil];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Empleados]') AND [c].[name] = N'Cargo');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Empleados] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [Empleados] DROP COLUMN [Cargo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Empleados]') AND [c].[name] = N'Id');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Empleados] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Empleados] DROP COLUMN [Id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Id');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [Clientes] DROP COLUMN [Id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Empleados] ADD CONSTRAINT [PK_Empleados] PRIMARY KEY ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    ALTER TABLE [Clientes] ADD CONSTRAINT [PK_Clientes] PRIMARY KEY ([PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''04''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000001'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''05''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000002'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''06''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000003'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''07''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000004'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''08''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000005'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''09''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000006'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    EXEC(N'UPDATE [Personas] SET [DireccionPrincipal] = N''Sistema'', [Genero] = NULL, [NombreComercial] = NULL, [RazonSocialONombresCompletos] = N''Administrador Sistema'', [TipoIdentificacion] = N''05''
    WHERE [Id] = ''33333333-3333-3333-3333-333333333333'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707045619_ReengineerPersonaClienteEmpleadoExtensions', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    DROP INDEX [IX_KardexMovimientos_ProductoId_FechaMovimiento] ON [KardexMovimientos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [BodegaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    CREATE TABLE [Bodegas] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Direccion] nvarchar(250) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Bodegas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    CREATE TABLE [ProductosBodega] (
        [ProductoId] uniqueidentifier NOT NULL,
        [BodegaId] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [StockActual] decimal(18,4) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProductosBodega] PRIMARY KEY ([ProductoId], [BodegaId]),
        CONSTRAINT [FK_ProductosBodega_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductosBodega_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    INSERT INTO Bodegas (Id, EmpresaId, Nombre, Direccion, IsActive, CreatedAt, UpdatedAt)
    SELECT NEWID(), source.EmpresaId, N'Principal', N'Matriz principal', CAST(1 AS bit), SYSDATETIMEOFFSET(), NULL
    FROM (
        SELECT DISTINCT EmpresaId
        FROM Productos
    ) AS source
    WHERE NOT EXISTS (
        SELECT 1
        FROM Bodegas currentBodega
        WHERE currentBodega.EmpresaId = source.EmpresaId
          AND currentBodega.Nombre = N'Principal'
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    INSERT INTO ProductosBodega (ProductoId, BodegaId, EmpresaId, StockActual)
    SELECT producto.Id,
           bodega.Id,
           producto.EmpresaId,
           producto.StockActual
    FROM Productos producto
    INNER JOIN Bodegas bodega
        ON bodega.EmpresaId = producto.EmpresaId
       AND bodega.Nombre = N'Principal'
    WHERE NOT EXISTS (
        SELECT 1
        FROM ProductosBodega productoBodega
        WHERE productoBodega.ProductoId = producto.Id
          AND productoBodega.BodegaId = bodega.Id
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    UPDATE kardex
    SET kardex.BodegaId = bodega.Id
    FROM KardexMovimientos kardex
    INNER JOIN Productos producto
        ON producto.Id = kardex.ProductoId
    INNER JOIN Bodegas bodega
        ON bodega.EmpresaId = producto.EmpresaId
       AND bodega.Nombre = N'Principal'
    WHERE kardex.BodegaId IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KardexMovimientos]') AND [c].[name] = N'BodegaId');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [KardexMovimientos] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [KardexMovimientos] ALTER COLUMN [BodegaId] uniqueidentifier NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'RowVersion');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [Productos] DROP COLUMN [RowVersion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'StockActual');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [Productos] DROP COLUMN [StockActual];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_BodegaId] ON [KardexMovimientos] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_ProductoId_BodegaId_FechaMovimiento] ON [KardexMovimientos] ([ProductoId], [BodegaId], [FechaMovimiento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bodegas_EmpresaId_Nombre] ON [Bodegas] ([EmpresaId], [Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    CREATE INDEX [IX_ProductosBodega_BodegaId] ON [ProductosBodega] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    CREATE INDEX [IX_ProductosBodega_EmpresaId_BodegaId] ON [ProductosBodega] ([EmpresaId], [BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD CONSTRAINT [FK_KardexMovimientos_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710035318_AddMultiBodegaStage41'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710035318_AddMultiBodegaStage41', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710044028_AddFacturaDispatchBodegaStage42'
)
BEGIN
    ALTER TABLE [Facturas] ADD [BodegaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710044028_AddFacturaDispatchBodegaStage42'
)
BEGIN
    UPDATE factura
    SET factura.BodegaId = bodega.Id
    FROM Facturas factura
    INNER JOIN Bodegas bodega
        ON bodega.EmpresaId = factura.EmpresaId
       AND bodega.Nombre = N'Principal'
    WHERE factura.BodegaId IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710044028_AddFacturaDispatchBodegaStage42'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Facturas]') AND [c].[name] = N'BodegaId');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Facturas] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [Facturas] ALTER COLUMN [BodegaId] uniqueidentifier NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710044028_AddFacturaDispatchBodegaStage42'
)
BEGIN
    CREATE INDEX [IX_Facturas_BodegaId] ON [Facturas] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710044028_AddFacturaDispatchBodegaStage42'
)
BEGIN
    ALTER TABLE [Facturas] ADD CONSTRAINT [FK_Facturas_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710044028_AddFacturaDispatchBodegaStage42'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710044028_AddFacturaDispatchBodegaStage42', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711174338_AddPointDispatchBodegaAndPhysicalCountStage44'
)
BEGIN
    ALTER TABLE [EmpresaPuntosEmision] ADD [BodegaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711174338_AddPointDispatchBodegaAndPhysicalCountStage44'
)
BEGIN
    INSERT INTO Bodegas (Id, EmpresaId, Nombre, Direccion, IsActive, CreatedAt, UpdatedAt)
    SELECT NEWID(), empresa.Id, N'Principal', NULL, CAST(1 AS bit), SYSUTCDATETIME(), NULL
    FROM EmpresasEmisoras empresa
    WHERE EXISTS (
        SELECT 1
        FROM EmpresaPuntosEmision punto
        WHERE punto.EmpresaEmisoraId = empresa.Id)
      AND NOT EXISTS (
        SELECT 1
        FROM Bodegas bodega
        WHERE bodega.EmpresaId = empresa.Id);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711174338_AddPointDispatchBodegaAndPhysicalCountStage44'
)
BEGIN
    ;WITH BodegaPreferida AS (
        SELECT
            punto.Id AS PuntoId,
            COALESCE(
                (
                    SELECT TOP (1) bodega.Id
                    FROM Bodegas bodega
                    WHERE bodega.EmpresaId = punto.EmpresaEmisoraId
                      AND bodega.IsActive = 1
                      AND bodega.Nombre = N'Principal'
                    ORDER BY bodega.Nombre
                ),
                (
                    SELECT TOP (1) bodega.Id
                    FROM Bodegas bodega
                    WHERE bodega.EmpresaId = punto.EmpresaEmisoraId
                      AND bodega.IsActive = 1
                    ORDER BY bodega.Nombre
                )
            ) AS BodegaId
        FROM EmpresaPuntosEmision punto
    )
    UPDATE punto
    SET punto.BodegaId = bodega.BodegaId
    FROM EmpresaPuntosEmision punto
    INNER JOIN BodegaPreferida bodega ON bodega.PuntoId = punto.Id;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711174338_AddPointDispatchBodegaAndPhysicalCountStage44'
)
BEGIN
    CREATE INDEX [IX_EmpresaPuntosEmision_BodegaId] ON [EmpresaPuntosEmision] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711174338_AddPointDispatchBodegaAndPhysicalCountStage44'
)
BEGIN
    ALTER TABLE [EmpresaPuntosEmision] ADD CONSTRAINT [FK_EmpresaPuntosEmision_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711174338_AddPointDispatchBodegaAndPhysicalCountStage44'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260711174338_AddPointDispatchBodegaAndPhysicalCountStage44', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    UPDATE CatalogoItems
    SET Codigo = CASE UPPER(LTRIM(RTRIM(Codigo)))
        WHEN 'PRUEBAS' THEN '1'
        WHEN 'PRODUCCION' THEN '2'
        WHEN 'NORMAL' THEN '1'
        WHEN 'EFECTIVO' THEN '01'
        WHEN 'COMPENSACION' THEN '15'
        WHEN 'TARJETA DE DEBITO' THEN '16'
        WHEN 'DINERO ELECTRONICO' THEN '17'
        WHEN 'TARJETA PREPAGO' THEN '18'
        WHEN 'TARJETA DE CREDITO' THEN '19'
        WHEN 'TRANSFERENCIA' THEN '20'
        WHEN 'ENDOSO DE TITULOS' THEN '21'
        ELSE Codigo
    END
    WHERE CatalogoId IN (
        '70000000-0000-0000-0000-000000000005',
        '70000000-0000-0000-0000-000000000006',
        '70000000-0000-0000-0000-000000000007');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    UPDATE EmpresasEmisoras
    SET AmbienteSri = CASE UPPER(LTRIM(RTRIM(AmbienteSri)))
            WHEN 'PRUEBAS' THEN '1'
            WHEN 'PRODUCCION' THEN '2'
            ELSE AmbienteSri
        END,
        TipoEmision = CASE UPPER(LTRIM(RTRIM(TipoEmision)))
            WHEN 'NORMAL' THEN '1'
            ELSE TipoEmision
        END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    UPDATE Facturas
    SET AmbienteSri = CASE UPPER(LTRIM(RTRIM(AmbienteSri)))
            WHEN 'PRUEBAS' THEN '1'
            WHEN 'PRODUCCION' THEN '2'
            ELSE AmbienteSri
        END,
        TipoEmision = CASE UPPER(LTRIM(RTRIM(TipoEmision)))
            WHEN 'NORMAL' THEN '1'
            ELSE TipoEmision
        END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''1''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000041'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''2''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000042'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''1''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000051'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''01''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000061'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''15''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000062'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''16''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000063'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''17''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000064'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''18''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000065'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''19''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000066'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''20''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000067'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [Codigo] = N''21''
    WHERE [Id] = ''71000000-0000-0000-0000-000000000068'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    EXEC(N'UPDATE [EmpresasEmisoras] SET [AmbienteSri] = N''1'', [TipoEmision] = N''1''
    WHERE [Id] = ''22222222-2222-2222-2222-222222222222'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711182917_NormalizeSriCatalogInternalCodes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260711182917_NormalizeSriCatalogInternalCodes', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711235129_AddProveedoresModuleStage51'
)
BEGIN
    CREATE TABLE [Proveedores] (
        [PersonaId] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [CodigoRetencionIvaDefault] nvarchar(20) NOT NULL,
        [CodigoRetencionRentaDefault] nvarchar(20) NOT NULL,
        [PermiteCredito] bit NOT NULL,
        [DiasCredito] int NOT NULL,
        [EstadoProveedor] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UsuarioModificacionId] uniqueidentifier NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([PersonaId]),
        CONSTRAINT [FK_Proveedores_Personas_PersonaId] FOREIGN KEY ([PersonaId]) REFERENCES [Personas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711235129_AddProveedoresModuleStage51'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Proveedores_EmpresaId_PersonaId] ON [Proveedores] ([EmpresaId], [PersonaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711235129_AddProveedoresModuleStage51'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260711235129_AddProveedoresModuleStage51', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    CREATE TABLE [Compras] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [ProveedorId] uniqueidentifier NOT NULL,
        [BodegaId] uniqueidentifier NOT NULL,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoEmision] nvarchar(3) NOT NULL,
        [Secuencial] nvarchar(9) NOT NULL,
        [ClaveAccesoProveedor] nvarchar(49) NULL,
        [FechaEmision] datetimeoffset NOT NULL,
        [SubtotalIva0] decimal(18,2) NOT NULL,
        [SubtotalIva5] decimal(18,2) NOT NULL,
        [SubtotalIva8] decimal(18,2) NOT NULL,
        [SubtotalIva15] decimal(18,2) NOT NULL,
        [TotalDescuento] decimal(18,2) NOT NULL,
        [TotalImpuestos] decimal(18,2) NOT NULL,
        [ImporteTotal] decimal(18,2) NOT NULL,
        [EstadoCompra] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UsuarioModificacionId] uniqueidentifier NULL,
        CONSTRAINT [PK_Compras] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Compras_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Compras_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([PersonaId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    CREATE TABLE [CompraDetalles] (
        [Id] uniqueidentifier NOT NULL,
        [CompraId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [ProductoCodigo] nvarchar(40) NOT NULL,
        [ProductoNombre] nvarchar(200) NOT NULL,
        [CodigoIva] nvarchar(10) NOT NULL,
        [PorcentajeIva] decimal(8,2) NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [CostoUnitario] decimal(18,6) NOT NULL,
        [Descuento] decimal(18,2) NOT NULL,
        [CostoTotalSinImpuesto] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_CompraDetalles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompraDetalles_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CompraDetalles_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    CREATE INDEX [IX_CompraDetalles_CompraId] ON [CompraDetalles] ([CompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    CREATE INDEX [IX_CompraDetalles_ProductoId] ON [CompraDetalles] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    CREATE INDEX [IX_Compras_BodegaId] ON [Compras] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Compras_EmpresaId_Establecimiento_PuntoEmision_Secuencial] ON [Compras] ([EmpresaId], [Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    CREATE INDEX [IX_Compras_ProveedorId] ON [Compras] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712004841_AddComprasModuleStage52'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712004841_AddComprasModuleStage52', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    DROP INDEX [IX_FacturaSecuenciales_EmpresaId_Establecimiento_PuntoEmision] ON [FacturaSecuenciales];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    DROP INDEX [IX_Compras_EmpresaId_Establecimiento_PuntoEmision_Secuencial] ON [Compras];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [FacturaSecuenciales] ADD [CodigoDocumento] nvarchar(2) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [ClaveAccesoGenerada] nvarchar(49) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [EstadoSri] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [FormaPagoSriCodigo] nvarchar(2) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [MensajeEstado] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [NextRetryAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [NumeroAutorizacion] nvarchar(64) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [Observacion] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [ProcessingNode] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [ProcessingStartedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [RetryCount] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [TipoDocumentoCodigo] nvarchar(2) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [XmlFirmado] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    ALTER TABLE [Compras] ADD [XmlGenerado] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    UPDATE FacturaSecuenciales
    SET CodigoDocumento = '01'
    WHERE CodigoDocumento = '';

    UPDATE Compras
    SET TipoDocumentoCodigo = '01'
    WHERE TipoDocumentoCodigo = '';

    UPDATE Compras
    SET FormaPagoSriCodigo = '01'
    WHERE FormaPagoSriCodigo = '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FacturaSecuenciales_EmpresaId_CodigoDocumento_Establecimiento_PuntoEmision] ON [FacturaSecuenciales] ([EmpresaId], [CodigoDocumento], [Establecimiento], [PuntoEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Compras_EmpresaId_TipoDocumentoCodigo_Establecimiento_PuntoEmision_Secuencial] ON [Compras] ([EmpresaId], [TipoDocumentoCodigo], [Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012119_AddCompraElectronicDocumentsStage53'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712012119_AddCompraElectronicDocumentsStage53', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012249_BackfillCompraElectronicDocumentCodesStage53'
)
BEGIN
    UPDATE FacturaSecuenciales
    SET CodigoDocumento = '01'
    WHERE CodigoDocumento = '' OR CodigoDocumento IS NULL;

    UPDATE Compras
    SET TipoDocumentoCodigo = '01'
    WHERE TipoDocumentoCodigo = '' OR TipoDocumentoCodigo IS NULL;

    UPDATE Compras
    SET FormaPagoSriCodigo = '01'
    WHERE FormaPagoSriCodigo = '' OR FormaPagoSriCodigo IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712012249_BackfillCompraElectronicDocumentCodesStage53'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712012249_BackfillCompraElectronicDocumentCodesStage53', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712051428_AddCuentasPorPagarStage54'
)
BEGIN
    CREATE TABLE [CuentasPorPagar] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [CompraId] uniqueidentifier NULL,
        [ProveedorId] uniqueidentifier NOT NULL,
        [FechaEmision] datetimeoffset NOT NULL,
        [FechaVence] datetimeoffset NOT NULL,
        [MontoOriginal] decimal(18,2) NOT NULL,
        [SaldoActual] decimal(18,2) NOT NULL,
        [EstadoDeuda] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UsuarioModificacionId] uniqueidentifier NULL,
        CONSTRAINT [PK_CuentasPorPagar] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CuentasPorPagar_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CuentasPorPagar_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([PersonaId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712051428_AddCuentasPorPagarStage54'
)
BEGIN
    CREATE TABLE [PagosCxP] (
        [Id] uniqueidentifier NOT NULL,
        [CuentaPorPagarId] uniqueidentifier NOT NULL,
        [FechaPago] datetimeoffset NOT NULL,
        [MontoPagado] decimal(18,2) NOT NULL,
        [FormaPago] nvarchar(2) NOT NULL,
        [ReferenciaTransaccion] nvarchar(100) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_PagosCxP] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PagosCxP_CuentasPorPagar_CuentaPorPagarId] FOREIGN KEY ([CuentaPorPagarId]) REFERENCES [CuentasPorPagar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712051428_AddCuentasPorPagarStage54'
)
BEGIN
    CREATE INDEX [IX_CuentasPorPagar_CompraId] ON [CuentasPorPagar] ([CompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712051428_AddCuentasPorPagarStage54'
)
BEGIN
    CREATE INDEX [IX_CuentasPorPagar_EmpresaId_ProveedorId_FechaVence] ON [CuentasPorPagar] ([EmpresaId], [ProveedorId], [FechaVence]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712051428_AddCuentasPorPagarStage54'
)
BEGIN
    CREATE INDEX [IX_CuentasPorPagar_ProveedorId] ON [CuentasPorPagar] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712051428_AddCuentasPorPagarStage54'
)
BEGIN
    CREATE INDEX [IX_PagosCxP_CuentaPorPagarId_FechaPago] ON [PagosCxP] ([CuentaPorPagarId], [FechaPago]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712051428_AddCuentasPorPagarStage54'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712051428_AddCuentasPorPagarStage54', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712172516_AddMemoriaAnalisisFiscalStage62Evolution'
)
BEGIN
    CREATE TABLE [MemoriasAnalisisFiscal] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Mes] int NOT NULL,
        [Anio] int NOT NULL,
        [ResumenNumericoJson] nvarchar(max) NOT NULL,
        [RazonamientoIA] nvarchar(max) NOT NULL,
        [ContextoPrevioUtilizado] nvarchar(500) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UsuarioModificacionId] uniqueidentifier NULL,
        CONSTRAINT [PK_MemoriasAnalisisFiscal] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712172516_AddMemoriaAnalisisFiscalStage62Evolution'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MemoriasAnalisisFiscal_EmpresaId_Mes_Anio] ON [MemoriasAnalisisFiscal] ([EmpresaId], [Mes], [Anio]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712172516_AddMemoriaAnalisisFiscalStage62Evolution'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712172516_AddMemoriaAnalisisFiscalStage62Evolution', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712201409_AddEstudioMercadoCompraStage63'
)
BEGIN
    CREATE TABLE [EstudiosMercadoCompra] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Anio] int NOT NULL,
        [Mes] int NOT NULL,
        [TopProductosVendidosJson] nvarchar(max) NOT NULL,
        [SugerenciasCompraJson] nvarchar(max) NOT NULL,
        [AnalisisEstrategicoIA] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UsuarioModificacionId] uniqueidentifier NULL,
        CONSTRAINT [PK_EstudiosMercadoCompra] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712201409_AddEstudioMercadoCompraStage63'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EstudiosMercadoCompra_EmpresaId_Anio_Mes] ON [EstudiosMercadoCompra] ([EmpresaId], [Anio], [Mes]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712201409_AddEstudioMercadoCompraStage63'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712201409_AddEstudioMercadoCompraStage63', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    ALTER TABLE [Facturas] ADD [CajaSesionId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    ALTER TABLE [Facturas] ADD [UsuarioId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    ALTER TABLE [CompraDetalles] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    ALTER TABLE [CompraDetalles] ADD [FechaEmisionCompra] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    UPDATE detalle
    SET
        detalle.EmpresaId = compra.EmpresaId,
        detalle.FechaEmisionCompra = compra.FechaEmision
    FROM CompraDetalles detalle
    INNER JOIN Compras compra ON compra.Id = detalle.CompraId
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    UPDATE factura
    SET factura.UsuarioId = empresa.OwnerUserId
    FROM Facturas factura
    INNER JOIN EmpresasEmisoras empresa ON empresa.Id = factura.EmpresaId
    WHERE factura.UsuarioId = '00000000-0000-0000-0000-000000000000'
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    CREATE TABLE [CajaSesiones] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [FechaApertura] datetimeoffset NOT NULL,
        [FechaCierre] datetimeoffset NULL,
        [MontoApertura] decimal(18,2) NOT NULL,
        [TotalVentasEfectivoCalculado] decimal(18,2) NOT NULL,
        [TotalVentasTarjetaCalculado] decimal(18,2) NOT NULL,
        [MontoFisicoEfectivoReal] decimal(18,2) NOT NULL,
        [MontoFisicoTarjetaReal] decimal(18,2) NOT NULL,
        [DiferenciaEfectivo] decimal(18,2) NOT NULL,
        [DiferenciaTarjeta] decimal(18,2) NOT NULL,
        [EstadoCaja] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UsuarioModificacionId] uniqueidentifier NULL,
        CONSTRAINT [PK_CajaSesiones] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    CREATE INDEX [IX_MemoriasAnalisisFiscal_EmpresaId_Anio_Mes_CreatedAt] ON [MemoriasAnalisisFiscal] ([EmpresaId], [Anio], [Mes], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_EmpresaId_FechaMovimiento_BodegaId_ProductoId] ON [KardexMovimientos] ([EmpresaId], [FechaMovimiento], [BodegaId], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    CREATE INDEX [IX_Facturas_CajaSesionId] ON [Facturas] ([CajaSesionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_CajaSesionId_CreatedAt] ON [Facturas] ([EmpresaId], [CajaSesionId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    CREATE INDEX [IX_CompraDetalles_EmpresaId_FechaEmisionCompra_ProductoId] ON [CompraDetalles] ([EmpresaId], [FechaEmisionCompra], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    CREATE INDEX [IX_CajaSesiones_EmpresaId_UsuarioId_EstadoCaja] ON [CajaSesiones] ([EmpresaId], [UsuarioId], [EstadoCaja]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    ALTER TABLE [Facturas] ADD CONSTRAINT [FK_Facturas_CajaSesiones_CajaSesionId] FOREIGN KEY ([CajaSesionId]) REFERENCES [CajaSesiones] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712203436_AddCajaSesionStage64'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712203436_AddCajaSesionStage64', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SecurityUsers]') AND [c].[name] = N'PasswordHash');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [SecurityUsers] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [SecurityUsers] ALTER COLUMN [PasswordHash] nvarchar(512) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD [BloqueadoHasta] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD [IntentosFallidos] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD [UltimoAcceso] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    CREATE TABLE [SecurityAuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [FechaEvento] datetimeoffset NOT NULL,
        [TipoEvento] nvarchar(40) NOT NULL,
        [DireccionIP] nvarchar(80) NULL,
        [Detalles] nvarchar(500) NULL,
        CONSTRAINT [PK_SecurityAuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    EXEC(N'UPDATE [SecurityUsers] SET [BloqueadoHasta] = NULL, [UltimoAcceso] = NULL
    WHERE [Id] = ''11111111-1111-1111-1111-111111111111'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditLogs_EmpresaId_FechaEvento] ON [SecurityAuditLogs] ([EmpresaId], [FechaEvento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditLogs_UsuarioId_FechaEvento] ON [SecurityAuditLogs] ([UsuarioId], [FechaEvento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713001700_AddSecurityHardeningStage71'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713001700_AddSecurityHardeningStage71', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    CREATE TABLE [SecurityPermisos] (
        [Id] nvarchar(120) NOT NULL,
        [NombrePermiso] nvarchar(120) NOT NULL,
        [Descripcion] nvarchar(240) NOT NULL,
        [Modulo] nvarchar(80) NOT NULL,
        CONSTRAINT [PK_SecurityPermisos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    CREATE TABLE [SecurityRolPermisos] (
        [RoleId] uniqueidentifier NOT NULL,
        [PermisoId] nvarchar(120) NOT NULL,
        CONSTRAINT [PK_SecurityRolPermisos] PRIMARY KEY ([RoleId], [PermisoId]),
        CONSTRAINT [FK_SecurityRolPermisos_SecurityPermisos_PermisoId] FOREIGN KEY ([PermisoId]) REFERENCES [SecurityPermisos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SecurityRolPermisos_SecurityRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SecurityRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Modulo', N'NombrePermiso') AND [object_id] = OBJECT_ID(N'[SecurityPermisos]'))
        SET IDENTITY_INSERT [SecurityPermisos] ON;
    EXEC(N'INSERT INTO [SecurityPermisos] ([Id], [Descripcion], [Modulo], [NombrePermiso])
    VALUES (N''caja.operar'', N''Apertura, cierre y control de caja.'', N''Caja'', N''caja.operar''),
    (N''catalogos.administrar'', N''Administracion de catalogos internos.'', N''Configuracion'', N''catalogos.administrar''),
    (N''clientes.ver'', N''Consulta y mantenimiento de clientes.'', N''Comercial'', N''clientes.ver''),
    (N''compras.cuentas-por-pagar'', N''Control de cuentas por pagar y abonos.'', N''Compras'', N''compras.cuentas-por-pagar''),
    (N''compras.estudio-mercado'', N''Analitica IA y estudio de mercado.'', N''Compras'', N''compras.estudio-mercado''),
    (N''compras.liquidaciones'', N''Emision y consulta de liquidaciones de compra.'', N''Compras'', N''compras.liquidaciones''),
    (N''compras.registrar'', N''Registro de compras y documentos de proveedor.'', N''Compras'', N''compras.registrar''),
    (N''dashboard.ver'', N''Acceso al dashboard principal.'', N''Dashboard'', N''dashboard.ver''),
    (N''empleados.ver'', N''Consulta y mantenimiento de empleados.'', N''Comercial'', N''empleados.ver''),
    (N''empresa.configurar'', N''Configuracion de empresa emisora y puntos de emision.'', N''Configuracion'', N''empresa.configurar''),
    (N''facturacion.monitor'', N''Consulta del monitor de comprobantes.'', N''Ventas'', N''facturacion.monitor''),
    (N''financiero.iva'', N''Consulta del reporte mensual de IVA.'', N''Financiero'', N''financiero.iva''),
    (N''inventario.ajustar'', N''Ajustes, mermas, transferencias y tomas fisicas.'', N''Inventario'', N''inventario.ajustar''),
    (N''inventario.bodegas'', N''Administracion de bodegas.'', N''Inventario'', N''inventario.bodegas''),
    (N''inventario.productos'', N''Creacion y actualizacion de productos.'', N''Inventario'', N''inventario.productos''),
    (N''inventario.ver'', N''Consulta de inventario y kardex.'', N''Inventario'', N''inventario.ver''),
    (N''personas.ver'', N''Consulta y mantenimiento de personas.'', N''Comercial'', N''personas.ver''),
    (N''pos.facturar'', N''Operacion del punto de venta y facturacion.'', N''Ventas'', N''pos.facturar''),
    (N''proveedores.ver'', N''Consulta y mantenimiento de proveedores.'', N''Comercial'', N''proveedores.ver''),
    (N''seguridad.usuarios'', N''Administracion de usuarios, roles y reseteo de claves.'', N''Seguridad'', N''seguridad.usuarios'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Modulo', N'NombrePermiso') AND [object_id] = OBJECT_ID(N'[SecurityPermisos]'))
        SET IDENTITY_INSERT [SecurityPermisos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[SecurityRoles]'))
        SET IDENTITY_INSERT [SecurityRoles] ON;
    EXEC(N'INSERT INTO [SecurityRoles] ([Id], [IsActive], [Name], [NormalizedName])
    VALUES (''44444444-4444-4444-4444-444444444444'', CAST(1 AS bit), N''Cajero'', N''CAJERO''),
    (''55555555-5555-5555-5555-555555555555'', CAST(1 AS bit), N''Bodeguero'', N''BODEGUERO''),
    (''66666666-6666-6666-6666-666666666666'', CAST(1 AS bit), N''Contador'', N''CONTADOR'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[SecurityRoles]'))
        SET IDENTITY_INSERT [SecurityRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermisoId', N'RoleId') AND [object_id] = OBJECT_ID(N'[SecurityRolPermisos]'))
        SET IDENTITY_INSERT [SecurityRolPermisos] ON;
    EXEC(N'INSERT INTO [SecurityRolPermisos] ([PermisoId], [RoleId])
    VALUES (N''caja.operar'', ''22222222-2222-2222-2222-222222222222''),
    (N''catalogos.administrar'', ''22222222-2222-2222-2222-222222222222''),
    (N''clientes.ver'', ''22222222-2222-2222-2222-222222222222''),
    (N''compras.cuentas-por-pagar'', ''22222222-2222-2222-2222-222222222222''),
    (N''compras.estudio-mercado'', ''22222222-2222-2222-2222-222222222222''),
    (N''compras.liquidaciones'', ''22222222-2222-2222-2222-222222222222''),
    (N''compras.registrar'', ''22222222-2222-2222-2222-222222222222''),
    (N''dashboard.ver'', ''22222222-2222-2222-2222-222222222222''),
    (N''empleados.ver'', ''22222222-2222-2222-2222-222222222222''),
    (N''empresa.configurar'', ''22222222-2222-2222-2222-222222222222''),
    (N''facturacion.monitor'', ''22222222-2222-2222-2222-222222222222''),
    (N''financiero.iva'', ''22222222-2222-2222-2222-222222222222''),
    (N''inventario.ajustar'', ''22222222-2222-2222-2222-222222222222''),
    (N''inventario.bodegas'', ''22222222-2222-2222-2222-222222222222''),
    (N''inventario.productos'', ''22222222-2222-2222-2222-222222222222''),
    (N''inventario.ver'', ''22222222-2222-2222-2222-222222222222''),
    (N''personas.ver'', ''22222222-2222-2222-2222-222222222222''),
    (N''pos.facturar'', ''22222222-2222-2222-2222-222222222222''),
    (N''proveedores.ver'', ''22222222-2222-2222-2222-222222222222''),
    (N''seguridad.usuarios'', ''22222222-2222-2222-2222-222222222222''),
    (N''caja.operar'', ''44444444-4444-4444-4444-444444444444''),
    (N''clientes.ver'', ''44444444-4444-4444-4444-444444444444''),
    (N''dashboard.ver'', ''44444444-4444-4444-4444-444444444444''),
    (N''facturacion.monitor'', ''44444444-4444-4444-4444-444444444444''),
    (N''pos.facturar'', ''44444444-4444-4444-4444-444444444444''),
    (N''dashboard.ver'', ''55555555-5555-5555-5555-555555555555''),
    (N''inventario.ajustar'', ''55555555-5555-5555-5555-555555555555''),
    (N''inventario.bodegas'', ''55555555-5555-5555-5555-555555555555''),
    (N''inventario.productos'', ''55555555-5555-5555-5555-555555555555''),
    (N''inventario.ver'', ''55555555-5555-5555-5555-555555555555''),
    (N''compras.cuentas-por-pagar'', ''66666666-6666-6666-6666-666666666666''),
    (N''compras.liquidaciones'', ''66666666-6666-6666-6666-666666666666''),
    (N''compras.registrar'', ''66666666-6666-6666-6666-666666666666''),
    (N''dashboard.ver'', ''66666666-6666-6666-6666-666666666666''),
    (N''empresa.configurar'', ''66666666-6666-6666-6666-666666666666''),
    (N''facturacion.monitor'', ''66666666-6666-6666-6666-666666666666''),
    (N''financiero.iva'', ''66666666-6666-6666-6666-666666666666''),
    (N''proveedores.ver'', ''66666666-6666-6666-6666-666666666666'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermisoId', N'RoleId') AND [object_id] = OBJECT_ID(N'[SecurityRolPermisos]'))
        SET IDENTITY_INSERT [SecurityRolPermisos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityPermisos_NombrePermiso] ON [SecurityPermisos] ([NombrePermiso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    CREATE INDEX [IX_SecurityRolPermisos_PermisoId] ON [SecurityRolPermisos] ([PermisoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713004438_AddSecurityPermissionsStage72'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713004438_AddSecurityPermissionsStage72', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713022644_AddSecurityUiManagementStage73'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD [BloqueadoManualmente] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713022644_AddSecurityUiManagementStage73'
)
BEGIN
    ALTER TABLE [SecurityUsers] ADD [TokensInvalidosDesde] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713022644_AddSecurityUiManagementStage73'
)
BEGIN
    EXEC(N'UPDATE [SecurityUsers] SET [TokensInvalidosDesde] = NULL
    WHERE [Id] = ''11111111-1111-1111-1111-111111111111'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713022644_AddSecurityUiManagementStage73'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713022644_AddSecurityUiManagementStage73', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713174208_AddSecurityUserPuntosEmisionAssignments'
)
BEGIN
    CREATE TABLE [SecurityUserPuntosEmision] (
        [SecurityUserId] uniqueidentifier NOT NULL,
        [EmpresaPuntoEmisionId] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_SecurityUserPuntosEmision] PRIMARY KEY ([SecurityUserId], [EmpresaPuntoEmisionId]),
        CONSTRAINT [FK_SecurityUserPuntosEmision_EmpresaPuntosEmision_EmpresaPuntoEmisionId] FOREIGN KEY ([EmpresaPuntoEmisionId]) REFERENCES [EmpresaPuntosEmision] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SecurityUserPuntosEmision_SecurityUsers_SecurityUserId] FOREIGN KEY ([SecurityUserId]) REFERENCES [SecurityUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713174208_AddSecurityUserPuntosEmisionAssignments'
)
BEGIN
    CREATE INDEX [IX_SecurityUserPuntosEmision_EmpresaId_EmpresaPuntoEmisionId] ON [SecurityUserPuntosEmision] ([EmpresaId], [EmpresaPuntoEmisionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713174208_AddSecurityUserPuntosEmisionAssignments'
)
BEGIN
    CREATE INDEX [IX_SecurityUserPuntosEmision_EmpresaId_SecurityUserId] ON [SecurityUserPuntosEmision] ([EmpresaId], [SecurityUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713174208_AddSecurityUserPuntosEmisionAssignments'
)
BEGIN
    CREATE INDEX [IX_SecurityUserPuntosEmision_EmpresaPuntoEmisionId] ON [SecurityUserPuntosEmision] ([EmpresaPuntoEmisionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713174208_AddSecurityUserPuntosEmisionAssignments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713174208_AddSecurityUserPuntosEmisionAssignments', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713210808_AddServiceItemSupportStage91'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'StockMinimo');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [Productos] ALTER COLUMN [StockMinimo] decimal(18,4) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713210808_AddServiceItemSupportStage91'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713210808_AddServiceItemSupportStage91', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714050348_AddServiceCommissionsStage93'
)
BEGIN
    ALTER TABLE [Productos] ADD [AplicaComision] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714050348_AddServiceCommissionsStage93'
)
BEGIN
    ALTER TABLE [Productos] ADD [TipoComision] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714050348_AddServiceCommissionsStage93'
)
BEGIN
    ALTER TABLE [Productos] ADD [ValorComision] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714050348_AddServiceCommissionsStage93'
)
BEGIN
    ALTER TABLE [FacturaDetalles] ADD [MontoComisionCalculado] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714050348_AddServiceCommissionsStage93'
)
BEGIN
    ALTER TABLE [FacturaDetalles] ADD [UsuarioIdOperador] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714050348_AddServiceCommissionsStage93'
)
BEGIN
    CREATE INDEX [IX_FacturaDetalles_UsuarioIdOperador] ON [FacturaDetalles] ([UsuarioIdOperador]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714050348_AddServiceCommissionsStage93'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714050348_AddServiceCommissionsStage93', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714112242_AddAccountingCoreStage101'
)
BEGIN
    CREATE TABLE [CuentasContables] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(40) NOT NULL,
        [Nombre] nvarchar(180) NOT NULL,
        [Nivel] int NOT NULL,
        [TipoCuenta] nvarchar(20) NOT NULL,
        [EsAceptable] bit NOT NULL DEFAULT CAST(0 AS bit),
        [SaldoActual] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_CuentasContables] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714112242_AddAccountingCoreStage101'
)
BEGIN
    CREATE TABLE [PeriodosContables] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Anio] int NOT NULL,
        [Mes] int NOT NULL,
        [EstaCerrado] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PeriodosContables] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714112242_AddAccountingCoreStage101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CuentasContables_EmpresaId_Codigo] ON [CuentasContables] ([EmpresaId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714112242_AddAccountingCoreStage101'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PeriodosContables_EmpresaId_Anio_Mes] ON [PeriodosContables] ([EmpresaId], [Anio], [Mes]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714112242_AddAccountingCoreStage101'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714112242_AddAccountingCoreStage101', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718045246_AddDoubleEntryJournalStage102'
)
BEGIN
    CREATE TABLE [AsientosContables] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [NumeroAsiento] nvarchar(20) NOT NULL,
        [FechaContable] datetime2 NOT NULL,
        [Concepto] nvarchar(300) NOT NULL,
        [ModuloOrigen] nvarchar(20) NOT NULL,
        [DocumentoSoporte] nvarchar(49) NULL,
        [Estado] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_AsientosContables] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718045246_AddDoubleEntryJournalStage102'
)
BEGIN
    CREATE TABLE [AsientosDetalle] (
        [Id] uniqueidentifier NOT NULL,
        [AsientoContableId] uniqueidentifier NOT NULL,
        [CuentaContableId] uniqueidentifier NOT NULL,
        [Debe] decimal(18,2) NOT NULL,
        [Haber] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_AsientosDetalle] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AsientosDetalle_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [AsientosContables] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AsientosDetalle_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [CuentasContables] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718045246_AddDoubleEntryJournalStage102'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AsientosContables_EmpresaId_NumeroAsiento] ON [AsientosContables] ([EmpresaId], [NumeroAsiento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718045246_AddDoubleEntryJournalStage102'
)
BEGIN
    CREATE INDEX [IX_AsientosDetalle_AsientoContableId] ON [AsientosDetalle] ([AsientoContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718045246_AddDoubleEntryJournalStage102'
)
BEGIN
    CREATE INDEX [IX_AsientosDetalle_CuentaContableId] ON [AsientosDetalle] ([CuentaContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718045246_AddDoubleEntryJournalStage102'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718045246_AddDoubleEntryJournalStage102', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [AsientoContableId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [Diferencia] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [DiferenciaTransferencia] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoFisicoTransferenciaReal] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [TotalVentasTransferenciaCalculado] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    CREATE INDEX [IX_CajaSesiones_AsientoContableId] ON [CajaSesiones] ([AsientoContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD CONSTRAINT [FK_CajaSesiones_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [AsientosContables] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722042410_AddCajaCierreDiarioContable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260722042410_AddCajaCierreDiarioContable', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722043159_AddFiscalPeriodClosingAuditStage105'
)
BEGIN
    ALTER TABLE [PeriodosContables] ADD [FechaCierre] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722043159_AddFiscalPeriodClosingAuditStage105'
)
BEGIN
    ALTER TABLE [PeriodosContables] ADD [UsuarioCierreId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722043159_AddFiscalPeriodClosingAuditStage105'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260722043159_AddFiscalPeriodClosingAuditStage105', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    ALTER TABLE [Compras] ADD [NaturalezaCompra] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    ALTER TABLE [Compras] ADD [SustentoTributarioSRI] nvarchar(2) NOT NULL DEFAULT N'01';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    ALTER TABLE [Compras] ADD [TipoComprobanteSRI] nvarchar(2) NOT NULL DEFAULT N'01';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CompraDetalles]') AND [c].[name] = N'ProductoId');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [CompraDetalles] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [CompraDetalles] ALTER COLUMN [ProductoId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    ALTER TABLE [CompraDetalles] ADD [CategoriaSriActivo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    ALTER TABLE [CompraDetalles] ADD [NaturalezaCompra] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    ALTER TABLE [CompraDetalles] ADD [NombreActivo] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    ALTER TABLE [CompraDetalles] ADD [SerieUbicacionActivo] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055053_AddCompraFiscalClassificationStage111'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260722055053_AddCompraFiscalClassificationStage111', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055809_AddFixedAssetsStage112'
)
BEGIN
    CREATE TABLE [ActivosFijos] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [CompraDetalleId] uniqueidentifier NULL,
        [CodigoActivo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(160) NOT NULL,
        [SerieMarca] nvarchar(120) NULL,
        [CategoriaSRI] nvarchar(40) NOT NULL,
        [FechaAdquisicion] datetime2 NOT NULL,
        [CostoInicial] decimal(18,2) NOT NULL,
        [ValorResidual] decimal(18,2) NOT NULL,
        [VidaUtilAnios] int NOT NULL,
        [PorcentajeDepreciacionAnual] decimal(8,2) NOT NULL,
        [UbicacionFisica] nvarchar(160) NULL,
        [CustodioResponsable] nvarchar(160) NULL,
        [EstadoActivo] nvarchar(30) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_ActivosFijos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivosFijos_CompraDetalles_CompraDetalleId] FOREIGN KEY ([CompraDetalleId]) REFERENCES [CompraDetalles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055809_AddFixedAssetsStage112'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ActivosFijos_CompraDetalleId] ON [ActivosFijos] ([CompraDetalleId]) WHERE [CompraDetalleId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055809_AddFixedAssetsStage112'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ActivosFijos_EmpresaId_CodigoActivo] ON [ActivosFijos] ([EmpresaId], [CodigoActivo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722055809_AddFixedAssetsStage112'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260722055809_AddFixedAssetsStage112', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724060503_AddMonetaryFlowBankingStage113'
)
BEGIN
    ALTER TABLE [PagosCxP] ADD [CuentaContableSalidaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724060503_AddMonetaryFlowBankingStage113'
)
BEGIN
    ALTER TABLE [PagosCxP] ADD [NumeroComprobantePago] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724060503_AddMonetaryFlowBankingStage113'
)
BEGIN
    ALTER TABLE [Compras] ADD [FormaPagoCompra] nvarchar(40) NOT NULL DEFAULT N'ContadoEfectivo';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724060503_AddMonetaryFlowBankingStage113'
)
BEGIN
    ALTER TABLE [Compras] ADD [RequiereBancarizacion] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724060503_AddMonetaryFlowBankingStage113'
)
BEGIN
    CREATE INDEX [IX_PagosCxP_CuentaContableSalidaId] ON [PagosCxP] ([CuentaContableSalidaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724060503_AddMonetaryFlowBankingStage113'
)
BEGIN
    ALTER TABLE [PagosCxP] ADD CONSTRAINT [FK_PagosCxP_CuentasContables_CuentaContableSalidaId] FOREIGN KEY ([CuentaContableSalidaId]) REFERENCES [CuentasContables] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724060503_AddMonetaryFlowBankingStage113'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724060503_AddMonetaryFlowBankingStage113', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728031253_AddPersonaNaturalEmpresaClienteSeparation'
)
BEGIN
    CREATE TABLE [EmpresasCliente] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [RazonSocial] nvarchar(180) NOT NULL,
        [NombreComercial] nvarchar(150) NULL,
        [Ruc] nvarchar(13) NOT NULL,
        [RepresentanteLegal] nvarchar(180) NULL,
        [ObligadoLlevarContabilidad] bit NOT NULL,
        [ContribuyenteEspecial] nvarchar(30) NULL,
        [EmailFacturacion] nvarchar(180) NULL,
        [Telefono] nvarchar(40) NULL,
        [DireccionMatriz] nvarchar(250) NOT NULL,
        CONSTRAINT [PK_EmpresasCliente] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728031253_AddPersonaNaturalEmpresaClienteSeparation'
)
BEGIN
    CREATE TABLE [PersonasNaturales] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [PrimerNombre] nvarchar(80) NOT NULL,
        [SegundoNombre] nvarchar(80) NULL,
        [PrimerApellido] nvarchar(80) NOT NULL,
        [SegundoApellido] nvarchar(80) NULL,
        [TipoDocumento] int NOT NULL,
        [NumeroDocumento] nvarchar(13) NOT NULL,
        [TieneRuc] bit NOT NULL,
        [Email] nvarchar(180) NULL,
        [Telefono] nvarchar(40) NULL,
        [Direccion] nvarchar(250) NOT NULL,
        CONSTRAINT [PK_PersonasNaturales] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728031253_AddPersonaNaturalEmpresaClienteSeparation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmpresasCliente_EmpresaId_Ruc] ON [EmpresasCliente] ([EmpresaId], [Ruc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728031253_AddPersonaNaturalEmpresaClienteSeparation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PersonasNaturales_EmpresaId_NumeroDocumento] ON [PersonasNaturales] ([EmpresaId], [NumeroDocumento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728031253_AddPersonaNaturalEmpresaClienteSeparation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728031253_AddPersonaNaturalEmpresaClienteSeparation', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728035310_AddPersonaClassificationFlags'
)
BEGIN
    ALTER TABLE [Personas] ADD [EsEmpresa] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728035310_AddPersonaClassificationFlags'
)
BEGIN
    ALTER TABLE [Personas] ADD [EsPersonaJuridica] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728035310_AddPersonaClassificationFlags'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728035310_AddPersonaClassificationFlags', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [Personas] ADD [CiudadCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [Personas] ADD [ProvinciaCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [Personas] ADD [RegionCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [Personas] ADD [SectorCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [CiudadCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [ProvinciaCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [RegionCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [EmpresasEmisoras] ADD [SectorCodigo] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [CatalogoItems] ADD [ParentItemId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE TABLE [GeoRegiones] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(40) NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Orden] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_GeoRegiones] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE TABLE [GeoProvincias] (
        [Id] uniqueidentifier NOT NULL,
        [RegionId] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(40) NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Orden] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_GeoProvincias] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GeoProvincias_GeoRegiones_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [GeoRegiones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE TABLE [GeoCiudades] (
        [Id] uniqueidentifier NOT NULL,
        [ProvinciaId] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(40) NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Orden] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_GeoCiudades] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GeoCiudades_GeoProvincias_ProvinciaId] FOREIGN KEY ([ProvinciaId]) REFERENCES [GeoProvincias] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE TABLE [GeoSectores] (
        [Id] uniqueidentifier NOT NULL,
        [CiudadId] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [Nombre] nvarchar(160) NOT NULL,
        [Orden] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_GeoSectores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GeoSectores_GeoCiudades_CiudadId] FOREIGN KEY ([CiudadId]) REFERENCES [GeoCiudades] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000001'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000002'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000003'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000004'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000005'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000006'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000011'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000012'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000013'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000014'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000015'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000021'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000022'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000031'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000032'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000033'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000034'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000035'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000036'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000041'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000042'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000051'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000061'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000062'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000063'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000064'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000065'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000066'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000067'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [CatalogoItems] SET [ParentItemId] = NULL
    WHERE [Id] = ''71000000-0000-0000-0000-000000000068'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [EmpresasEmisoras] SET [CiudadCodigo] = N''GUAYAQUIL'', [ProvinciaCodigo] = N''GUAYAS'', [RegionCodigo] = N''COSTA'', [SectorCodigo] = N''GUAYAQUIL_NORTE''
    WHERE [Id] = ''22222222-2222-2222-2222-222222222222'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    EXEC(N'UPDATE [Personas] SET [CiudadCodigo] = N''QUITO'', [ProvinciaCodigo] = N''PICHINCHA'', [RegionCodigo] = N''NORTE'', [SectorCodigo] = N''QUITO_NORTE''
    WHERE [Id] = ''33333333-3333-3333-3333-333333333333'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE INDEX [IX_CatalogoItems_ParentItemId] ON [CatalogoItems] ([ParentItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GeoCiudades_Codigo] ON [GeoCiudades] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE INDEX [IX_GeoCiudades_ProvinciaId] ON [GeoCiudades] ([ProvinciaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GeoProvincias_Codigo] ON [GeoProvincias] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE INDEX [IX_GeoProvincias_RegionId] ON [GeoProvincias] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GeoRegiones_Codigo] ON [GeoRegiones] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GeoSectores_CiudadId_Codigo] ON [GeoSectores] ([CiudadId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    ALTER TABLE [CatalogoItems] ADD CONSTRAINT [FK_CatalogoItems_CatalogoItems_ParentItemId] FOREIGN KEY ([ParentItemId]) REFERENCES [CatalogoItems] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728043215_AddEcuadorGeographicAddressEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728043215_AddEcuadorGeographicAddressEntities', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728045155_AddProductMaintenanceFields'
)
BEGIN
    ALTER TABLE [Productos] ADD [CategoriaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728045155_AddProductMaintenanceFields'
)
BEGIN
    ALTER TABLE [Productos] ADD [CostoReferencial] decimal(18,6) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728045155_AddProductMaintenanceFields'
)
BEGIN
    ALTER TABLE [Productos] ADD [NaturalezaItem] nvarchar(30) NOT NULL DEFAULT N'Mercaderia';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728045155_AddProductMaintenanceFields'
)
BEGIN
    ALTER TABLE [Productos] ADD [UnidadMedida] nvarchar(60) NOT NULL DEFAULT N'Unidad';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728045155_AddProductMaintenanceFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728045155_AddProductMaintenanceFields', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    ALTER TABLE [Bodegas] ADD [Codigo] nvarchar(3) NOT NULL DEFAULT N'001';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    ALTER TABLE [Bodegas] ADD [EsPrincipal] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    WITH BodegasOrdenadas AS
    (
        SELECT
            Id,
            ROW_NUMBER() OVER (
                PARTITION BY EmpresaId
                ORDER BY
                    CASE WHEN Nombre = 'Principal' THEN 0 ELSE 1 END,
                    Nombre,
                    Id) AS Numero
        FROM Bodegas
    )
    UPDATE b
    SET
        Codigo = RIGHT('000' + CAST(o.Numero AS varchar(3)), 3),
        EsPrincipal = CASE WHEN o.Numero = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
    FROM Bodegas b
    INNER JOIN BodegasOrdenadas o ON o.Id = b.Id;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE TABLE [TransferenciasInventario] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [BodegaOrigenId] uniqueidentifier NOT NULL,
        [BodegaDestinoId] uniqueidentifier NOT NULL,
        [FechaEmision] datetimeoffset NOT NULL,
        [FechaTraslado] datetimeoffset NULL,
        [Estado] nvarchar(20) NOT NULL,
        [MotivoTraslado] nvarchar(250) NOT NULL,
        [GuiaRemisionId] uniqueidentifier NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_TransferenciasInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransferenciasInventario_Bodegas_BodegaDestinoId] FOREIGN KEY ([BodegaDestinoId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransferenciasInventario_Bodegas_BodegaOrigenId] FOREIGN KEY ([BodegaOrigenId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE TABLE [TransferenciasInventarioDetalle] (
        [Id] uniqueidentifier NOT NULL,
        [TransferenciaInventarioId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [CantidadEnviada] decimal(18,4) NOT NULL,
        [CantidadRecibida] decimal(18,4) NOT NULL,
        [CostoUnitario] decimal(18,6) NOT NULL,
        CONSTRAINT [PK_TransferenciasInventarioDetalle] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransferenciasInventarioDetalle_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransferenciasInventarioDetalle_TransferenciasInventario_TransferenciaInventarioId] FOREIGN KEY ([TransferenciaInventarioId]) REFERENCES [TransferenciasInventario] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bodegas_EmpresaId_Codigo] ON [Bodegas] ([EmpresaId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE INDEX [IX_TransferenciasInventario_BodegaDestinoId] ON [TransferenciasInventario] ([BodegaDestinoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE INDEX [IX_TransferenciasInventario_BodegaOrigenId] ON [TransferenciasInventario] ([BodegaOrigenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE INDEX [IX_TransferenciasInventario_EmpresaId_BodegaOrigenId_BodegaDestinoId] ON [TransferenciasInventario] ([EmpresaId], [BodegaOrigenId], [BodegaDestinoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE INDEX [IX_TransferenciasInventario_EmpresaId_Estado] ON [TransferenciasInventario] ([EmpresaId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE INDEX [IX_TransferenciasInventarioDetalle_ProductoId] ON [TransferenciasInventarioDetalle] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TransferenciasInventarioDetalle_TransferenciaInventarioId_ProductoId] ON [TransferenciasInventarioDetalle] ([TransferenciaInventarioId], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728050239_AddWarehouseTransferManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728050239_AddWarehouseTransferManagement', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE TABLE [ColaProcesamientoSRI] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [ComprobanteId] uniqueidentifier NOT NULL,
        [TipoDocumentoId] nvarchar(2) NOT NULL,
        [Estado] nvarchar(30) NOT NULL,
        [Intentos] int NOT NULL,
        [NextRetryAt] datetimeoffset NULL,
        [Mensaje] nvarchar(400) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_ColaProcesamientoSRI] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE TABLE [ComprobanteCabecera] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [TipoDocumentoId] nvarchar(2) NOT NULL,
        [Secuencial] bigint NOT NULL,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoEmision] nvarchar(3) NOT NULL,
        [ComprobanteModificadoId] uniqueidentifier NULL,
        [MotivoModificacion] nvarchar(300) NULL,
        [CodDocModificado] nvarchar(2) NULL,
        [NumDocModificado] nvarchar(17) NULL,
        [FechaEmisionDocSustento] datetimeoffset NULL,
        [RucEmisor] nvarchar(13) NOT NULL,
        [RazonSocialEmisor] nvarchar(300) NOT NULL,
        [NombreComercialEmisor] nvarchar(300) NULL,
        [DireccionMatrizEmisor] nvarchar(300) NOT NULL,
        [DireccionEstablecimientoEmisor] nvarchar(300) NULL,
        [AmbienteSri] nvarchar(20) NOT NULL,
        [TipoEmision] nvarchar(20) NOT NULL,
        [ObligadoContabilidad] bit NOT NULL,
        [ClienteTipoIdentificacion] nvarchar(2) NOT NULL,
        [ClienteIdentificacion] nvarchar(20) NOT NULL,
        [ClienteNombre] nvarchar(300) NOT NULL,
        [ClienteDireccion] nvarchar(300) NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [TotalDescuento] decimal(18,2) NOT NULL,
        [IvaTotal] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [ClaveAcceso] nvarchar(49) NOT NULL,
        [NumeroAutorizacion] nvarchar(80) NULL,
        [XmlGenerado] nvarchar(max) NULL,
        [XmlFirmado] nvarchar(max) NULL,
        [Estado] nvarchar(30) NOT NULL,
        [FechaEmision] datetimeoffset NOT NULL,
        [FechaAutorizacion] datetimeoffset NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_ComprobanteCabecera] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprobanteCabecera_Facturas_ComprobanteModificadoId] FOREIGN KEY ([ComprobanteModificadoId]) REFERENCES [Facturas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE TABLE [ComprobanteDetalle] (
        [Id] uniqueidentifier NOT NULL,
        [ComprobanteCabeceraId] uniqueidentifier NOT NULL,
        [FacturaDetalleOrigenId] uniqueidentifier NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [CodigoProducto] nvarchar(40) NOT NULL,
        [NombreProducto] nvarchar(160) NOT NULL,
        [CodigoIva] nvarchar(20) NOT NULL,
        [PorcentajeIva] decimal(9,2) NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,6) NOT NULL,
        [Descuento] decimal(18,2) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [IvaValor] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ComprobanteDetalle] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprobanteDetalle_ComprobanteCabecera_ComprobanteCabeceraId] FOREIGN KEY ([ComprobanteCabeceraId]) REFERENCES [ComprobanteCabecera] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[SecurityRoles]'))
        SET IDENTITY_INSERT [SecurityRoles] ON;
    EXEC(N'INSERT INTO [SecurityRoles] ([Id], [IsActive], [Name], [NormalizedName])
    VALUES (''77777777-7777-7777-7777-777777777777'', CAST(1 AS bit), N''Gerente'', N''GERENTE''),
    (''88888888-8888-8888-8888-888888888888'', CAST(1 AS bit), N''AsesorComercial'', N''ASESORCOMERCIAL'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[SecurityRoles]'))
        SET IDENTITY_INSERT [SecurityRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermisoId', N'RoleId') AND [object_id] = OBJECT_ID(N'[SecurityRolPermisos]'))
        SET IDENTITY_INSERT [SecurityRolPermisos] ON;
    EXEC(N'INSERT INTO [SecurityRolPermisos] ([PermisoId], [RoleId])
    VALUES (N''caja.operar'', ''77777777-7777-7777-7777-777777777777''),
    (N''catalogos.administrar'', ''77777777-7777-7777-7777-777777777777''),
    (N''clientes.ver'', ''77777777-7777-7777-7777-777777777777''),
    (N''compras.cuentas-por-pagar'', ''77777777-7777-7777-7777-777777777777''),
    (N''compras.estudio-mercado'', ''77777777-7777-7777-7777-777777777777''),
    (N''compras.liquidaciones'', ''77777777-7777-7777-7777-777777777777''),
    (N''compras.registrar'', ''77777777-7777-7777-7777-777777777777''),
    (N''dashboard.ver'', ''77777777-7777-7777-7777-777777777777''),
    (N''empleados.ver'', ''77777777-7777-7777-7777-777777777777''),
    (N''empresa.configurar'', ''77777777-7777-7777-7777-777777777777''),
    (N''facturacion.monitor'', ''77777777-7777-7777-7777-777777777777''),
    (N''financiero.iva'', ''77777777-7777-7777-7777-777777777777''),
    (N''inventario.ajustar'', ''77777777-7777-7777-7777-777777777777''),
    (N''inventario.bodegas'', ''77777777-7777-7777-7777-777777777777''),
    (N''inventario.productos'', ''77777777-7777-7777-7777-777777777777''),
    (N''inventario.ver'', ''77777777-7777-7777-7777-777777777777''),
    (N''personas.ver'', ''77777777-7777-7777-7777-777777777777''),
    (N''pos.facturar'', ''77777777-7777-7777-7777-777777777777''),
    (N''proveedores.ver'', ''77777777-7777-7777-7777-777777777777''),
    (N''seguridad.usuarios'', ''77777777-7777-7777-7777-777777777777''),
    (N''clientes.ver'', ''88888888-8888-8888-8888-888888888888''),
    (N''dashboard.ver'', ''88888888-8888-8888-8888-888888888888''),
    (N''facturacion.monitor'', ''88888888-8888-8888-8888-888888888888''),
    (N''pos.facturar'', ''88888888-8888-8888-8888-888888888888'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermisoId', N'RoleId') AND [object_id] = OBJECT_ID(N'[SecurityRolPermisos]'))
        SET IDENTITY_INSERT [SecurityRolPermisos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE INDEX [IX_ColaProcesamientoSRI_ComprobanteId_TipoDocumentoId] ON [ColaProcesamientoSRI] ([ComprobanteId], [TipoDocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE INDEX [IX_ColaProcesamientoSRI_EmpresaId_Estado_NextRetryAt_CreatedAt] ON [ColaProcesamientoSRI] ([EmpresaId], [Estado], [NextRetryAt], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprobanteCabecera_ClaveAcceso] ON [ComprobanteCabecera] ([ClaveAcceso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_ComprobanteModificadoId] ON [ComprobanteCabecera] ([ComprobanteModificadoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprobanteCabecera_EmpresaId_TipoDocumentoId_Establecimiento_PuntoEmision_Secuencial] ON [ComprobanteCabecera] ([EmpresaId], [TipoDocumentoId], [Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_Estado_CreatedAt] ON [ComprobanteCabecera] ([Estado], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE INDEX [IX_ComprobanteDetalle_ComprobanteCabeceraId] ON [ComprobanteDetalle] ([ComprobanteCabeceraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    CREATE INDEX [IX_ComprobanteDetalle_FacturaDetalleOrigenId] ON [ComprobanteDetalle] ([FacturaDetalleOrigenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729042951_AddNotaCreditoElectronicaTipo04'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729042951_AddNotaCreditoElectronicaTipo04', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807040343_AddFacturaCashSettlementFields'
)
BEGIN
    ALTER TABLE [Facturas] ADD [MontoRecibido] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807040343_AddFacturaCashSettlementFields'
)
BEGIN
    ALTER TABLE [Facturas] ADD [VueltoEntregado] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807040343_AddFacturaCashSettlementFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807040343_AddFacturaCashSettlementFields', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812024336_AddCertificadosDigitalesEmpresa'
)
BEGIN
    CREATE TABLE [CertificadosDigitalesEmpresa] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(160) NOT NULL,
        [NombreArchivo] nvarchar(260) NOT NULL,
        [Contenido] varbinary(max) NOT NULL,
        [Clave] nvarchar(200) NOT NULL,
        [Sujeto] nvarchar(500) NULL,
        [Emisor] nvarchar(500) NULL,
        [NumeroSerie] nvarchar(120) NULL,
        [HuellaDigital] nvarchar(120) NULL,
        [FechaInicioVigencia] datetimeoffset NOT NULL,
        [FechaFinVigencia] datetimeoffset NOT NULL,
        [IsActive] bit NOT NULL,
        [EsPrincipal] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UsuarioCreacionId] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UsuarioModificacionId] uniqueidentifier NULL,
        CONSTRAINT [PK_CertificadosDigitalesEmpresa] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CertificadosDigitalesEmpresa_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812024336_AddCertificadosDigitalesEmpresa'
)
BEGIN
    CREATE INDEX [IX_CertificadosDigitalesEmpresa_EmpresaId_EsPrincipal] ON [CertificadosDigitalesEmpresa] ([EmpresaId], [EsPrincipal]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812024336_AddCertificadosDigitalesEmpresa'
)
BEGIN
    CREATE INDEX [IX_CertificadosDigitalesEmpresa_EmpresaId_FechaFinVigencia] ON [CertificadosDigitalesEmpresa] ([EmpresaId], [FechaFinVigencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812024336_AddCertificadosDigitalesEmpresa'
)
BEGIN
    CREATE INDEX [IX_CertificadosDigitalesEmpresa_IsActive_FechaFinVigencia] ON [CertificadosDigitalesEmpresa] ([IsActive], [FechaFinVigencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812024336_AddCertificadosDigitalesEmpresa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812024336_AddCertificadosDigitalesEmpresa', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE TABLE [ComprobantesRetencion] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [CompraId] uniqueidentifier NULL,
        [ProveedorId] uniqueidentifier NOT NULL,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoEmision] nvarchar(3) NOT NULL,
        [Secuencial] nvarchar(9) NOT NULL,
        [ClaveAcceso] nvarchar(49) NULL,
        [FechaEmision] datetimeoffset NOT NULL,
        [AmbienteSRI] tinyint NOT NULL,
        [EstadoSRI] nvarchar(30) NOT NULL,
        [NumeroAutorizacion] nvarchar(49) NULL,
        [FechaAutorizacion] datetimeoffset NULL,
        [MensajeErrorSRI] nvarchar(max) NULL,
        [TotalRetenido] decimal(18,4) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_ComprobantesRetencion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprobantesRetencion_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ComprobantesRetencion_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ComprobantesRetencion_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([PersonaId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE TABLE [Proformas] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [BodegaId] uniqueidentifier NOT NULL,
        [Secuencial] nvarchar(9) NOT NULL,
        [FechaEmision] datetimeoffset NOT NULL,
        [FechaVencimiento] datetimeoffset NULL,
        [Estado] tinyint NOT NULL,
        [SubtotalSinImpuestos] decimal(18,4) NOT NULL,
        [SubtotalIVA] decimal(18,4) NOT NULL,
        [DescuentoTotal] decimal(18,4) NOT NULL,
        [Total] decimal(18,4) NOT NULL,
        [Observacion] nvarchar(500) NULL,
        [FacturaId] uniqueidentifier NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Proformas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Proformas_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proformas_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([PersonaId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proformas_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proformas_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [Facturas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proformas_SecurityUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [SecurityUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE TABLE [ComprobanteRetencionDetalles] (
        [Id] uniqueidentifier NOT NULL,
        [ComprobanteRetencionId] uniqueidentifier NOT NULL,
        [CodigoImpuesto] nvarchar(5) NOT NULL,
        [CodigoRetencionSRI] nvarchar(10) NOT NULL,
        [BaseImponible] decimal(18,4) NOT NULL,
        [PorcentajeRetencion] decimal(5,2) NOT NULL,
        [ValorRetenido] decimal(18,4) NOT NULL,
        [CodDocSustento] nvarchar(2) NOT NULL,
        [NumDocSustento] nvarchar(15) NOT NULL,
        [FechaEmisionDocSustento] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ComprobanteRetencionDetalles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprobanteRetencionDetalles_ComprobantesRetencion_ComprobanteRetencionId] FOREIGN KEY ([ComprobanteRetencionId]) REFERENCES [ComprobantesRetencion] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE TABLE [ProformaDetalles] (
        [Id] uniqueidentifier NOT NULL,
        [ProformaId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [Descuento] decimal(18,4) NOT NULL,
        [TarifaIVA] decimal(18,4) NOT NULL,
        [ValorIVA] decimal(18,4) NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        CONSTRAINT [PK_ProformaDetalles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProformaDetalles_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProformaDetalles_Proformas_ProformaId] FOREIGN KEY ([ProformaId]) REFERENCES [Proformas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Modulo', N'NombrePermiso') AND [object_id] = OBJECT_ID(N'[SecurityPermisos]'))
        SET IDENTITY_INSERT [SecurityPermisos] ON;
    EXEC(N'INSERT INTO [SecurityPermisos] ([Id], [Descripcion], [Modulo], [NombrePermiso])
    VALUES (N''reporteria.ventas'', N''Consulta de reportes detallados y proyecciones de ventas.'', N''Reporteria'', N''reporteria.ventas'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Modulo', N'NombrePermiso') AND [object_id] = OBJECT_ID(N'[SecurityPermisos]'))
        SET IDENTITY_INSERT [SecurityPermisos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermisoId', N'RoleId') AND [object_id] = OBJECT_ID(N'[SecurityRolPermisos]'))
        SET IDENTITY_INSERT [SecurityRolPermisos] ON;
    EXEC(N'INSERT INTO [SecurityRolPermisos] ([PermisoId], [RoleId])
    VALUES (N''reporteria.ventas'', ''22222222-2222-2222-2222-222222222222''),
    (N''reporteria.ventas'', ''66666666-6666-6666-6666-666666666666''),
    (N''reporteria.ventas'', ''77777777-7777-7777-7777-777777777777'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermisoId', N'RoleId') AND [object_id] = OBJECT_ID(N'[SecurityRolPermisos]'))
        SET IDENTITY_INSERT [SecurityRolPermisos] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_ComprobanteRetencionDetalles_ComprobanteRetencionId_CodigoImpuesto_CodigoRetencionSRI] ON [ComprobanteRetencionDetalles] ([ComprobanteRetencionId], [CodigoImpuesto], [CodigoRetencionSRI]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ComprobantesRetencion_ClaveAcceso] ON [ComprobantesRetencion] ([ClaveAcceso]) WHERE [ClaveAcceso] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_ComprobantesRetencion_CompraId] ON [ComprobantesRetencion] ([CompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprobantesRetencion_EmpresaId_Establecimiento_PuntoEmision_Secuencial] ON [ComprobantesRetencion] ([EmpresaId], [Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_ComprobantesRetencion_EmpresaId_EstadoSRI_FechaEmision] ON [ComprobantesRetencion] ([EmpresaId], [EstadoSRI], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_ComprobantesRetencion_EmpresaId_ProveedorId_FechaEmision] ON [ComprobantesRetencion] ([EmpresaId], [ProveedorId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_ComprobantesRetencion_ProveedorId] ON [ComprobantesRetencion] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_ProformaDetalles_ProductoId] ON [ProformaDetalles] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_ProformaDetalles_ProformaId_ProductoId] ON [ProformaDetalles] ([ProformaId], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_Proformas_BodegaId] ON [Proformas] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_Proformas_ClienteId] ON [Proformas] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_Proformas_EmpresaId_ClienteId_FechaEmision] ON [Proformas] ([EmpresaId], [ClienteId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_Proformas_EmpresaId_Estado_FechaEmision] ON [Proformas] ([EmpresaId], [Estado], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Proformas_EmpresaId_Secuencial] ON [Proformas] ([EmpresaId], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_Proformas_FacturaId] ON [Proformas] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    CREATE INDEX [IX_Proformas_UsuarioId] ON [Proformas] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820032923_AddProformasAndRetencionesSRI'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820032923_AddProformasAndRetencionesSRI', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824024306_AddXmlSriLogAndOfflinePOS'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[FacturaCompraXmlLogs] (
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
            [CreatedAt] datetimeoffset NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_CreatedAt] DEFAULT SYSUTCDATETIME(),
            [UpdatedAt] datetimeoffset NULL,
            CONSTRAINT [PK_FacturaCompraXmlLogs] PRIMARY KEY ([Id])
        );
    END;

    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'EmpresaId') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [EmpresaId] uniqueidentifier NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_EmpresaId] DEFAULT '00000000-0000-0000-0000-000000000000';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'ClaveAcceso') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [ClaveAcceso] nvarchar(49) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_ClaveAcceso] DEFAULT '';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'RucEmisor') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [RucEmisor] nvarchar(13) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_RucEmisor] DEFAULT '';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'RazonSocialEmisor') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [RazonSocialEmisor] nvarchar(300) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_RazonSocialEmisor] DEFAULT '';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'RucComprador') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [RucComprador] nvarchar(13) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_RucComprador] DEFAULT '';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'FechaEmision') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [FechaEmision] datetimeoffset NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_FechaEmision] DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'CodDoc') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [CodDoc] nvarchar(2) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_CodDoc] DEFAULT '01';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'EstabPuntoEmiSecuencial') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [EstabPuntoEmiSecuencial] nvarchar(17) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_EstabPuntoEmiSecuencial] DEFAULT '';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'TotalSinImpuestos') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [TotalSinImpuestos] decimal(18,4) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_TotalSinImpuestos] DEFAULT 0;
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'TotalDescuento') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [TotalDescuento] decimal(18,4) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_TotalDescuento] DEFAULT 0;
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'ImporteTotal') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [ImporteTotal] decimal(18,4) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_ImporteTotal] DEFAULT 0;
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'XmlContenido') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [XmlContenido] nvarchar(max) NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_XmlContenido] DEFAULT '';
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'EstadoProcesamiento') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [EstadoProcesamiento] tinyint NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_EstadoProcesamiento] DEFAULT 0;
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'CompraId') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [CompraId] uniqueidentifier NULL;
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'CreatedAt') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [CreatedAt] datetimeoffset NOT NULL CONSTRAINT [DF_FacturaCompraXmlLogs_CreatedAt_Add] DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH(N'[dbo].[FacturaCompraXmlLogs]', N'UpdatedAt') IS NULL
        ALTER TABLE [dbo].[FacturaCompraXmlLogs] ADD [UpdatedAt] datetimeoffset NULL;

    IF OBJECT_ID(N'[dbo].[Compras]', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_FacturaCompraXmlLogs_Compras_CompraId')
    BEGIN
        ALTER TABLE [dbo].[FacturaCompraXmlLogs]
        ADD CONSTRAINT [FK_FacturaCompraXmlLogs_Compras_CompraId]
        FOREIGN KEY ([CompraId]) REFERENCES [dbo].[Compras]([Id]) ON DELETE SET NULL;
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_CompraId' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
        CREATE INDEX [IX_FacturaCompraXmlLogs_CompraId] ON [dbo].[FacturaCompraXmlLogs]([CompraId]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
        CREATE UNIQUE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_ClaveAcceso] ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [ClaveAcceso]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision' AND [object_id] = OBJECT_ID(N'[dbo].[FacturaCompraXmlLogs]'))
        CREATE INDEX [IX_FacturaCompraXmlLogs_EmpresaId_EstadoProcesamiento_FechaEmision] ON [dbo].[FacturaCompraXmlLogs]([EmpresaId], [EstadoProcesamiento], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824024306_AddXmlSriLogAndOfflinePOS'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Catalogos] WHERE [Id] = '70000000-0000-0000-0000-000000000008')
        INSERT INTO [dbo].[Catalogos] ([Id], [Codigo], [Descripcion], [IsActive], [Nombre]) VALUES ('70000000-0000-0000-0000-000000000008', N'RETENCION_IVA_SRI', N'Codigos base de retencion de IVA para documentos de proveedor.', 1, N'Retenciones IVA SRI');
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Catalogos] WHERE [Id] = '70000000-0000-0000-0000-000000000009')
        INSERT INTO [dbo].[Catalogos] ([Id], [Codigo], [Descripcion], [IsActive], [Nombre]) VALUES ('70000000-0000-0000-0000-000000000009', N'RETENCION_RENTA_SRI', N'Codigos base de retencion en la fuente para documentos de proveedor.', 1, N'Retenciones Renta SRI');

    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'0') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000081', '70000000-0000-0000-0000-000000000008', N'0', N'0%', 1, N'Sin retencion IVA', 1, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'10') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000082', '70000000-0000-0000-0000-000000000008', N'10', N'10%', 1, N'Retencion IVA 10%', 2, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'20') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000083', '70000000-0000-0000-0000-000000000008', N'20', N'20%', 1, N'Retencion IVA 20%', 3, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'30') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000084', '70000000-0000-0000-0000-000000000008', N'30', N'30%', 1, N'Retencion IVA 30%', 4, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'50') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000085', '70000000-0000-0000-0000-000000000008', N'50', N'50%', 1, N'Retencion IVA 50%', 5, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'70') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000086', '70000000-0000-0000-0000-000000000008', N'70', N'70%', 1, N'Retencion IVA 70%', 6, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000008' AND [Codigo] = N'100') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000087', '70000000-0000-0000-0000-000000000008', N'100', N'100%', 1, N'Retencion IVA 100%', 7, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'0') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000091', '70000000-0000-0000-0000-000000000009', N'0', N'0%', 1, N'Sin retencion renta', 1, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'312') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000092', '70000000-0000-0000-0000-000000000009', N'312', N'1.75%', 1, N'Retencion renta codigo 312', 2, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'320') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000093', '70000000-0000-0000-0000-000000000009', N'320', N'1.75%', 1, N'Retencion renta codigo 320', 3, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'322') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000094', '70000000-0000-0000-0000-000000000009', N'322', N'1.75%', 1, N'Retencion renta codigo 322', 4, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'332') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000095', '70000000-0000-0000-0000-000000000009', N'332', N'1.75%', 1, N'Bienes codigo 332', 5, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'343') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000096', '70000000-0000-0000-0000-000000000009', N'343', N'2.75%', 1, N'Servicios codigo 343', 6, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'344') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000097', '70000000-0000-0000-0000-000000000009', N'344', N'2.75%', 1, N'Servicios codigo 344', 7, NULL);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CatalogoItems] WHERE [CatalogoId] = '70000000-0000-0000-0000-000000000009' AND [Codigo] = N'3440') INSERT INTO [dbo].[CatalogoItems] ([Id], [CatalogoId], [Codigo], [Descripcion], [IsActive], [Nombre], [Orden], [ParentItemId]) VALUES ('71000000-0000-0000-0000-000000000098', '70000000-0000-0000-0000-000000000009', N'3440', N'70%', 1, N'Retencion IVA codigo 3440', 8, NULL);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824024306_AddXmlSriLogAndOfflinePOS'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260824024306_AddXmlSriLogAndOfflinePOS', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE TABLE [EmpresaConfiguracionServicios] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [WhatsAppApiToken] nvarchar(600) NULL,
        [WhatsAppPhoneId] nvarchar(80) NULL,
        [WhatsAppBusinessAccountId] nvarchar(80) NULL,
        [PayPhoneToken] nvarchar(600) NULL,
        [PayPhoneClientAppId] nvarchar(120) NULL,
        [PasarelaPagoActiva] tinyint NOT NULL,
        [ModopagosAmbiente] tinyint NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_EmpresaConfiguracionServicios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmpresaConfiguracionServicios_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE TABLE [TransaccionesPagosDigitales] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [Pasarela] nvarchar(50) NOT NULL,
        [TransactionIdPasarela] nvarchar(100) NOT NULL,
        [EstadoPago] nvarchar(30) NOT NULL,
        [LinkPagoUrl] nvarchar(500) NULL,
        [QrCodeBase64] nvarchar(max) NULL,
        [FechaCreacion] datetimeoffset NOT NULL,
        [FechaAprobacion] datetimeoffset NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_TransaccionesPagosDigitales] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransaccionesPagosDigitales_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([PersonaId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransaccionesPagosDigitales_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransaccionesPagosDigitales_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [Facturas] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE TABLE [WhatsAppNotificacionesLog] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [NumeroDestino] nvarchar(20) NOT NULL,
        [TipoDocumento] nvarchar(20) NOT NULL,
        [DocumentoId] uniqueidentifier NOT NULL,
        [EstadoEnvio] nvarchar(30) NOT NULL,
        [MensajeError] nvarchar(max) NULL,
        [FechaEnvio] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_WhatsAppNotificacionesLog] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WhatsAppNotificacionesLog_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmpresaConfiguracionServicios_EmpresaId] ON [EmpresaConfiguracionServicios] ([EmpresaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE INDEX [IX_TransaccionesPagosDigitales_ClienteId] ON [TransaccionesPagosDigitales] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE INDEX [IX_TransaccionesPagosDigitales_EmpresaId_EstadoPago_FechaCreacion] ON [TransaccionesPagosDigitales] ([EmpresaId], [EstadoPago], [FechaCreacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE INDEX [IX_TransaccionesPagosDigitales_EmpresaId_FacturaId] ON [TransaccionesPagosDigitales] ([EmpresaId], [FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TransaccionesPagosDigitales_EmpresaId_TransactionIdPasarela] ON [TransaccionesPagosDigitales] ([EmpresaId], [TransactionIdPasarela]) WHERE [TransactionIdPasarela] <> ''''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE INDEX [IX_TransaccionesPagosDigitales_FacturaId] ON [TransaccionesPagosDigitales] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE INDEX [IX_WhatsAppNotificacionesLog_EmpresaId_EstadoEnvio_FechaEnvio] ON [WhatsAppNotificacionesLog] ([EmpresaId], [EstadoEnvio], [FechaEnvio]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    CREATE INDEX [IX_WhatsAppNotificacionesLog_EmpresaId_TipoDocumento_DocumentoId] ON [WhatsAppNotificacionesLog] ([EmpresaId], [TipoDocumento], [DocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824034442_AddWhatsAppAndDigitalPayments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260824034442_AddWhatsAppAndDigitalPayments', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    DROP INDEX [IX_ColaProcesamientoSRI_ComprobanteId_TipoDocumentoId] ON [ColaProcesamientoSRI];
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ColaProcesamientoSRI]') AND [c].[name] = N'TipoDocumentoId');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [ColaProcesamientoSRI] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [ColaProcesamientoSRI] ALTER COLUMN [TipoDocumentoId] nvarchar(10) NOT NULL;
    CREATE INDEX [IX_ColaProcesamientoSRI_ComprobanteId_TipoDocumentoId] ON [ColaProcesamientoSRI] ([ComprobanteId], [TipoDocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ColaProcesamientoSRI]') AND [c].[name] = N'Intentos');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [ColaProcesamientoSRI] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [ColaProcesamientoSRI] ADD DEFAULT 0 FOR [Intentos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    UPDATE ColaProcesamientoSRI
    SET Estado = CASE
        WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('PENDIENTE') THEN 'PENDIENTE'
        WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('EN_PROCESO', 'PROCESANDO') THEN 'EN_PROCESO'
        WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('AUTORIZADO') THEN 'AUTORIZADO'
        WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('ERROR') THEN 'ERROR'
        WHEN UPPER(REPLACE(Estado, ' ', '_')) IN ('DEVUELTO', 'DEVUELTA') THEN 'DEVUELTO'
        ELSE 'ERROR'
    END
    WHERE Estado IS NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    DROP INDEX [IX_ColaProcesamientoSRI_EmpresaId_Estado_NextRetryAt_CreatedAt] ON [ColaProcesamientoSRI];
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ColaProcesamientoSRI]') AND [c].[name] = N'Estado');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [ColaProcesamientoSRI] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [ColaProcesamientoSRI] ALTER COLUMN [Estado] varchar(20) NOT NULL;
    CREATE INDEX [IX_ColaProcesamientoSRI_EmpresaId_Estado_NextRetryAt_CreatedAt] ON [ColaProcesamientoSRI] ([EmpresaId], [Estado], [NextRetryAt], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ColaProcesamientoSRI]') AND [c].[name] = N'CreatedAt');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [ColaProcesamientoSRI] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [ColaProcesamientoSRI] ADD DEFAULT (SYSUTCDATETIME()) FOR [CreatedAt];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    ALTER TABLE [ColaProcesamientoSRI] ADD [ProcessingNode] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    ALTER TABLE [ColaProcesamientoSRI] ADD [ProcessingStartedAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    ALTER TABLE [ColaProcesamientoSRI] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    ALTER TABLE [ColaProcesamientoSRI] ADD [UltimoError] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    CREATE TABLE [DocumentosAdjuntos] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [Modulo] varchar(30) NOT NULL,
        [EntidadTipo] varchar(50) NOT NULL,
        [EntidadId] uniqueidentifier NOT NULL,
        [TipoAdjunto] varchar(40) NOT NULL,
        [NombreArchivo] nvarchar(260) NOT NULL,
        [ContentType] varchar(100) NOT NULL,
        [RutaStorage] nvarchar(500) NOT NULL,
        [HashSHA256] varchar(64) NOT NULL,
        [TamanoBytes] bigint NOT NULL,
        [CreadoPorUsuarioId] uniqueidentifier NULL,
        [Origen] varchar(30) NOT NULL,
        [EsActivo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Version] int NOT NULL DEFAULT 1,
        [FechaCreacion] datetimeoffset NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_DocumentosAdjuntos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DocumentosAdjuntos_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    EXEC(N'ALTER TABLE [ColaProcesamientoSRI] ADD CONSTRAINT [CK_ColaProcesamientoSRI_Estado] CHECK ([Estado] IN (''PENDIENTE'',''EN_PROCESO'',''AUTORIZADO'',''ERROR'',''DEVUELTO''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    CREATE INDEX [IX_DocumentosAdjuntos_EmpresaId_EntidadTipo_EntidadId] ON [DocumentosAdjuntos] ([EmpresaId], [EntidadTipo], [EntidadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    CREATE INDEX [IX_DocumentosAdjuntos_EmpresaId_Modulo_TipoAdjunto_FechaCreacion] ON [DocumentosAdjuntos] ([EmpresaId], [Modulo], [TipoAdjunto], [FechaCreacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260901051343_Sprint1_EvolucionColaSRI_Y_DocumentoAdjunto', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductosBodega]') AND [c].[name] = N'StockActual');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [ProductosBodega] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [ProductosBodega] ADD DEFAULT 0.0 FOR [StockActual];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [ProductosBodega] ADD [EsActivo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [ProductosBodega] ADD [StockMaximo] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [ProductosBodega] ADD [StockMinimo] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KardexMovimientos]') AND [c].[name] = N'TipoMovimiento');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [KardexMovimientos] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [KardexMovimientos] ALTER COLUMN [TipoMovimiento] varchar(30) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KardexMovimientos]') AND [c].[name] = N'FechaMovimiento');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [KardexMovimientos] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [KardexMovimientos] ADD DEFAULT (SYSUTCDATETIME()) FOR [FechaMovimiento];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [CompraId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [CostoTotal] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [CreadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [FacturaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [StockAnterior] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [StockNuevo] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD [TransferenciaInventarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    UPDATE KardexMovimientos
    SET TipoMovimiento = CASE
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('ENTRADA', 'INGRESO_COMPRA') THEN 'ENTRADA_COMPRA'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('SALIDA', 'EGRESO_FACTURA') THEN 'SALIDA_VENTA'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('DEVOLUCION', 'DEVOLUCIONVENTA', 'DEVOLUCION_VENTA') THEN 'DEVOLUCION_VENTA'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('INGRESO_AJUSTE') THEN 'AJUSTE_INGRESO'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('EGRESO_AJUSTE') THEN 'AJUSTE_EGRESO'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('EGRESO_MERMA', 'MERMA') THEN 'MERMA_INVENTARIO'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('TRANSFERENCIA_DESPACHO', 'TRANSFERENCIA_SALIDA') THEN 'TRANSFERENCIA_SALIDA'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('TRANSFERENCIA_RECEPCION', 'TRANSFERENCIA_ENTRADA') THEN 'TRANSFERENCIA_ENTRADA'
        WHEN UPPER(REPLACE(TipoMovimiento, ' ', '_')) IN ('TOMA_FISICA') THEN 'TOMA_FISICA'
        ELSE 'AJUSTE_INGRESO'
    END;

    UPDATE KardexMovimientos
    SET
        StockNuevo = SaldoCantidad,
        StockAnterior = CASE
            WHEN CantidadEntrada > 0 THEN SaldoCantidad - CantidadEntrada
            WHEN CantidadSalida > 0 THEN SaldoCantidad + CantidadSalida
            ELSE SaldoCantidad
        END,
        CostoTotal = CASE
            WHEN CantidadEntrada > 0 THEN CantidadEntrada * CostoUnitario
            WHEN CantidadSalida > 0 THEN CantidadSalida * CostoUnitario
            ELSE 0
        END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    CREATE INDEX [IX_ProductosBodega_EmpresaId_BodegaId_ProductoId] ON [ProductosBodega] ([EmpresaId], [BodegaId], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_CompraId] ON [KardexMovimientos] ([CompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_EmpresaId_BodegaId_ProductoId_FechaMovimiento] ON [KardexMovimientos] ([EmpresaId], [BodegaId], [ProductoId], [FechaMovimiento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_FacturaId] ON [KardexMovimientos] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    CREATE INDEX [IX_KardexMovimientos_TransferenciaInventarioId] ON [KardexMovimientos] ([TransferenciaInventarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    EXEC(N'ALTER TABLE [KardexMovimientos] ADD CONSTRAINT [CK_KardexMovimientos_TipoMovimiento] CHECK ([TipoMovimiento] IN (''ENTRADA_COMPRA'', ''SALIDA_VENTA'', ''DEVOLUCION_VENTA'', ''AJUSTE_INGRESO'', ''AJUSTE_EGRESO'', ''TRANSFERENCIA_ENTRADA'', ''TRANSFERENCIA_SALIDA'', ''MERMA_INVENTARIO'', ''TOMA_FISICA''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD CONSTRAINT [FK_KardexMovimientos_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD CONSTRAINT [FK_KardexMovimientos_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [Facturas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    ALTER TABLE [KardexMovimientos] ADD CONSTRAINT [FK_KardexMovimientos_TransferenciasInventario_TransferenciaInventarioId] FOREIGN KEY ([TransferenciaInventarioId]) REFERENCES [TransferenciasInventario] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260901052900_Sprint2_ProductoBodega_Y_KardexAppendOnly', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'TotalVentasTransferenciaCalculado');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [TotalVentasTransferenciaCalculado] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'TotalVentasTarjetaCalculado');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [TotalVentasTarjetaCalculado] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'TotalVentasEfectivoCalculado');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [TotalVentasEfectivoCalculado] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'MontoFisicoTransferenciaReal');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [MontoFisicoTransferenciaReal] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'MontoFisicoTarjetaReal');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var40 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [MontoFisicoTarjetaReal] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'MontoFisicoEfectivoReal');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var41 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [MontoFisicoEfectivoReal] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'MontoApertura');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [MontoApertura] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'DiferenciaTransferencia');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [DiferenciaTransferencia] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'DiferenciaTarjeta');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [DiferenciaTarjeta] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'DiferenciaEfectivo');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [DiferenciaEfectivo] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CajaSesiones]') AND [c].[name] = N'Diferencia');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [CajaSesiones] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [CajaSesiones] ALTER COLUMN [Diferencia] decimal(18,4) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [BodegaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [DiferenciaMonto] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoCalculadoEfectivo] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoCalculadoOtros] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoCalculadoTarjetas] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoCalculadoTotal] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoCalculadoTransferencias] decimal(18,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoDeclaradoEfectivo] decimal(18,4) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoDeclaradoOtros] decimal(18,4) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoDeclaradoTarjetas] decimal(18,4) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoDeclaradoTotal] decimal(18,4) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [MontoDeclaradoTransferencias] decimal(18,4) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [ObservacionesCierre] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [PuntoEmisionId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE TABLE [CajaMovimientos] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [CajaSesionId] uniqueidentifier NOT NULL,
        [TipoMovimiento] varchar(30) NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [Concepto] varchar(250) NOT NULL,
        [ComprobanteReferencia] varchar(80) NULL,
        [CreadoPorUsuarioId] uniqueidentifier NOT NULL,
        [FechaMovimiento] datetimeoffset NOT NULL,
        CONSTRAINT [PK_CajaMovimientos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CajaMovimientos_CajaSesiones_CajaSesionId] FOREIGN KEY ([CajaSesionId]) REFERENCES [CajaSesiones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE TABLE [FacturaPagos] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NOT NULL,
        [CajaSesionId] uniqueidentifier NOT NULL,
        [FormaPagoCodigo] varchar(2) NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [LoteNumero] varchar(80) NULL,
        [VoucherNumero] varchar(80) NULL,
        [BancoNombre] nvarchar(120) NULL,
        [NumeroReferencia] varchar(120) NULL,
        CONSTRAINT [PK_FacturaPagos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacturaPagos_CajaSesiones_CajaSesionId] FOREIGN KEY ([CajaSesionId]) REFERENCES [CajaSesiones] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FacturaPagos_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [Facturas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_CajaSesiones_BodegaId] ON [CajaSesiones] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_CajaSesiones_EmpresaId_PuntoEmisionId_UsuarioId_EstadoCaja] ON [CajaSesiones] ([EmpresaId], [PuntoEmisionId], [UsuarioId], [EstadoCaja]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_CajaSesiones_PuntoEmisionId] ON [CajaSesiones] ([PuntoEmisionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_CajaMovimientos_CajaSesionId] ON [CajaMovimientos] ([CajaSesionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_CajaMovimientos_EmpresaId_CajaSesionId_FechaMovimiento] ON [CajaMovimientos] ([EmpresaId], [CajaSesionId], [FechaMovimiento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_FacturaPagos_CajaSesionId] ON [FacturaPagos] ([CajaSesionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_FacturaPagos_EmpresaId_CajaSesionId_FacturaId] ON [FacturaPagos] ([EmpresaId], [CajaSesionId], [FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    CREATE INDEX [IX_FacturaPagos_FacturaId] ON [FacturaPagos] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    INSERT INTO FacturaPagos (Id, EmpresaId, FacturaId, CajaSesionId, FormaPagoCodigo, Monto, LoteNumero, VoucherNumero, BancoNombre, NumeroReferencia)
    SELECT NEWID(), EmpresaId, Id, CajaSesionId, FormaPagoSriCodigo, Total, NULL, NULL, NULL, NULL
    FROM Facturas
    WHERE CajaSesionId IS NOT NULL
      AND NOT EXISTS (
          SELECT 1
          FROM FacturaPagos pagos
          WHERE pagos.FacturaId = Facturas.Id
      );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD CONSTRAINT [FK_CajaSesiones_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    ALTER TABLE [CajaSesiones] ADD CONSTRAINT [FK_CajaSesiones_EmpresaPuntosEmision_PuntoEmisionId] FOREIGN KEY ([PuntoEmisionId]) REFERENCES [EmpresaPuntosEmision] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260902040137_Sprint3_CajaSesion_Y_TesoreriaPOS', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902041935_Sprint4_ReportesFinancieros_Y_ConsolidacionContable'
)
BEGIN
    IF COL_LENGTH(N'[dbo].[FacturaDetalles]', N'CostoHistoricoTotal') IS NULL
    BEGIN
        ALTER TABLE [dbo].[FacturaDetalles]
        ADD [CostoHistoricoTotal] decimal(18,4) NOT NULL
        CONSTRAINT [DF_FacturaDetalles_CostoHistoricoTotal] DEFAULT 0;
    END;

    IF COL_LENGTH(N'[dbo].[FacturaDetalles]', N'CostoHistoricoUnitario') IS NULL
    BEGIN
        ALTER TABLE [dbo].[FacturaDetalles]
        ADD [CostoHistoricoUnitario] decimal(18,6) NOT NULL
        CONSTRAINT [DF_FacturaDetalles_CostoHistoricoUnitario] DEFAULT 0;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_FacturaDetalles_ProductoId_FacturaId'
          AND object_id = OBJECT_ID(N'[dbo].[FacturaDetalles]')
    )
    BEGIN
        CREATE INDEX [IX_FacturaDetalles_ProductoId_FacturaId]
        ON [dbo].[FacturaDetalles] ([ProductoId], [FacturaId]);
    END;

    UPDATE detalle
    SET
        CostoHistoricoTotal = ISNULL(kardex.CostoTotal, 0),
        CostoHistoricoUnitario = CASE
            WHEN detalle.Cantidad > 0 THEN ISNULL(kardex.CostoTotal, 0) / detalle.Cantidad
            ELSE 0
        END
    FROM FacturaDetalles detalle
    OUTER APPLY (
        SELECT SUM(movimiento.CostoTotal) AS CostoTotal
        FROM KardexMovimientos movimiento
        WHERE movimiento.FacturaId = detalle.FacturaId
          AND movimiento.ProductoId = detalle.ProductoId
          AND movimiento.TipoMovimiento IN ('SALIDA_VENTA', 'Salida')
    ) kardex
    WHERE detalle.CostoHistoricoTotal = 0
      AND kardex.CostoTotal IS NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902041935_Sprint4_ReportesFinancieros_Y_ConsolidacionContable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260902041935_Sprint4_ReportesFinancieros_Y_ConsolidacionContable', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE TABLE [CuentasBancarias] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [BancoNombre] nvarchar(100) NOT NULL,
        [TipoCuenta] tinyint NOT NULL,
        [NumeroCuenta] nvarchar(30) NOT NULL,
        [SaldoContable] decimal(18,4) NOT NULL,
        [SaldoConciliado] decimal(18,4) NOT NULL,
        [Moneda] varchar(3) NOT NULL DEFAULT 'USD',
        [CuentaContableId] uniqueidentifier NOT NULL,
        [Activa] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_CuentasBancarias] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CuentasBancarias_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [CuentasContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CuentasBancarias_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE TABLE [ExtractoBancarioHeaders] (
        [Id] uniqueidentifier NOT NULL,
        [CuentaBancariaId] uniqueidentifier NOT NULL,
        [FechaImportacion] datetime2 NOT NULL,
        [FechaDesde] datetime2 NOT NULL,
        [FechaHasta] datetime2 NOT NULL,
        [NombreArchivoOriginal] nvarchar(255) NOT NULL,
        [TotalRegistros] int NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        CONSTRAINT [PK_ExtractoBancarioHeaders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExtractoBancarioHeaders_CuentasBancarias_CuentaBancariaId] FOREIGN KEY ([CuentaBancariaId]) REFERENCES [CuentasBancarias] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE TABLE [MovimientosTesoreria] (
        [Id] uniqueidentifier NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [CuentaBancariaId] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Tipo] tinyint NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [Beneficiario] nvarchar(200) NOT NULL,
        [FacturaVentaId] uniqueidentifier NULL,
        [CompraId] uniqueidentifier NULL,
        [AsientoContableId] uniqueidentifier NULL,
        [EstadoConciliacion] tinyint NOT NULL DEFAULT CAST(0 AS tinyint),
        CONSTRAINT [PK_MovimientosTesoreria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientosTesoreria_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [AsientosContables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosTesoreria_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosTesoreria_CuentasBancarias_CuentaBancariaId] FOREIGN KEY ([CuentaBancariaId]) REFERENCES [CuentasBancarias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosTesoreria_EmpresasEmisoras_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [EmpresasEmisoras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosTesoreria_Facturas_FacturaVentaId] FOREIGN KEY ([FacturaVentaId]) REFERENCES [Facturas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE TABLE [ExtractoBancarioDetalles] (
        [Id] uniqueidentifier NOT NULL,
        [ExtractoHeaderId] uniqueidentifier NOT NULL,
        [FechaTransaccion] datetime2 NOT NULL,
        [NumeroDocumentoRef] nvarchar(50) NOT NULL,
        [ConceptoDescripcion] nvarchar(500) NOT NULL,
        [TipoMovimiento] tinyint NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [Conciliado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [MovimientoTesoreriaId] uniqueidentifier NULL,
        CONSTRAINT [PK_ExtractoBancarioDetalles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExtractoBancarioDetalles_ExtractoBancarioHeaders_ExtractoHeaderId] FOREIGN KEY ([ExtractoHeaderId]) REFERENCES [ExtractoBancarioHeaders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExtractoBancarioDetalles_MovimientosTesoreria_MovimientoTesoreriaId] FOREIGN KEY ([MovimientoTesoreriaId]) REFERENCES [MovimientosTesoreria] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_CuentasBancarias_CuentaContableId] ON [CuentasBancarias] ([CuentaContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_CuentasBancarias_EmpresaId_Activa] ON [CuentasBancarias] ([EmpresaId], [Activa]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_CuentasBancarias_EmpresaId_NumeroCuenta] ON [CuentasBancarias] ([EmpresaId], [NumeroCuenta]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_ExtractoHeaderId] ON [ExtractoBancarioDetalles] ([ExtractoHeaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_ExtractoHeaderId_Conciliado] ON [ExtractoBancarioDetalles] ([ExtractoHeaderId], [Conciliado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_FechaTransaccion] ON [ExtractoBancarioDetalles] ([FechaTransaccion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_FechaTransaccion_NumeroDocumentoRef] ON [ExtractoBancarioDetalles] ([FechaTransaccion], [NumeroDocumentoRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_MovimientoTesoreriaId] ON [ExtractoBancarioDetalles] ([MovimientoTesoreriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_NumeroDocumentoRef] ON [ExtractoBancarioDetalles] ([NumeroDocumentoRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioHeaders_CuentaBancariaId_FechaDesde_FechaHasta] ON [ExtractoBancarioHeaders] ([CuentaBancariaId], [FechaDesde], [FechaHasta]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioHeaders_FechaImportacion] ON [ExtractoBancarioHeaders] ([FechaImportacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_AsientoContableId] ON [MovimientosTesoreria] ([AsientoContableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_CompraId] ON [MovimientosTesoreria] ([CompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_CuentaBancariaId_Fecha] ON [MovimientosTesoreria] ([CuentaBancariaId], [Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_EmpresaId_CuentaBancariaId_Fecha] ON [MovimientosTesoreria] ([EmpresaId], [CuentaBancariaId], [Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_EmpresaId_EstadoConciliacion_Fecha] ON [MovimientosTesoreria] ([EmpresaId], [EstadoConciliacion], [Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_FacturaVentaId] ON [MovimientosTesoreria] ([FacturaVentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929040039_AddTesoreriaAndConciliacionBancaria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929040039_AddTesoreriaAndConciliacionBancaria', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_Facturas_ClaveAcceso] ON [Facturas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_Facturas_Establecimiento_PuntoEmision_Secuencial] ON [Facturas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_Facturas_Estado_CreatedAt] ON [Facturas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_ComprobantesRetencion_ClaveAcceso] ON [ComprobantesRetencion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_ComprobanteCabecera_ClaveAcceso] ON [ComprobanteCabecera];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    DROP INDEX [IX_ComprobanteCabecera_Estado_CreatedAt] ON [ComprobanteCabecera];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    ALTER TABLE [ExtractoBancarioDetalles] ADD [CuentaBancariaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    ALTER TABLE [ExtractoBancarioDetalles] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    UPDATE detalle
    SET
        detalle.[EmpresaId] = cuenta.[EmpresaId],
        detalle.[CuentaBancariaId] = header.[CuentaBancariaId]
    FROM [ExtractoBancarioDetalles] AS detalle
    INNER JOIN [ExtractoBancarioHeaders] AS header
        ON detalle.[ExtractoHeaderId] = header.[Id]
    INNER JOIN [CuentasBancarias] AS cuenta
        ON header.[CuentaBancariaId] = cuenta.[Id]
    WHERE detalle.[EmpresaId] = '00000000-0000-0000-0000-000000000000'
       OR detalle.[CuentaBancariaId] = '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Facturas_EmpresaId_ClaveAcceso] ON [Facturas] ([EmpresaId], [ClaveAcceso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_ClienteId] ON [Facturas] ([EmpresaId], [ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Facturas_EmpresaId_Establecimiento_PuntoEmision_Secuencial] ON [Facturas] ([EmpresaId], [Establecimiento], [PuntoEmision], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_Estado] ON [Facturas] ([EmpresaId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_Estado_CreatedAt] ON [Facturas] ([EmpresaId], [Estado], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_FechaEmision] ON [Facturas] ([EmpresaId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Facturas_EmpresaId_Secuencial] ON [Facturas] ([EmpresaId], [Secuencial]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_Conciliado_FechaTransaccion] ON [ExtractoBancarioDetalles] ([EmpresaId], [CuentaBancariaId], [Conciliado], [FechaTransaccion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_FechaTransaccion] ON [ExtractoBancarioDetalles] ([EmpresaId], [CuentaBancariaId], [FechaTransaccion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ExtractoBancarioDetalles_EmpresaId_CuentaBancariaId_NumeroDocumentoRef_Monto] ON [ExtractoBancarioDetalles] ([EmpresaId], [CuentaBancariaId], [NumeroDocumentoRef], [Monto]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_CuentasPorPagar_EmpresaId_EstadoDeuda_FechaVence] ON [CuentasPorPagar] ([EmpresaId], [EstadoDeuda], [FechaVence]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_CuentasPorPagar_EmpresaId_ProveedorId_EstadoDeuda] ON [CuentasPorPagar] ([EmpresaId], [ProveedorId], [EstadoDeuda]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ComprobantesRetencion_EmpresaId_ClaveAcceso] ON [ComprobantesRetencion] ([EmpresaId], [ClaveAcceso]) WHERE [ClaveAcceso] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobantesRetencion_EmpresaId_EstadoSRI] ON [ComprobantesRetencion] ([EmpresaId], [EstadoSRI]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprobanteCabecera_EmpresaId_ClaveAcceso] ON [ComprobanteCabecera] ([EmpresaId], [ClaveAcceso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_EmpresaId_Estado] ON [ComprobanteCabecera] ([EmpresaId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_EmpresaId_Estado_CreatedAt] ON [ComprobanteCabecera] ([EmpresaId], [Estado], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ComprobanteCabecera_EmpresaId_FechaEmision] ON [ComprobanteCabecera] ([EmpresaId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Compras_EmpresaId_ClaveAccesoProveedor] ON [Compras] ([EmpresaId], [ClaveAccesoProveedor]) WHERE [ClaveAccesoProveedor] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Compras_EmpresaId_EstadoCompra_FechaEmision] ON [Compras] ([EmpresaId], [EstadoCompra], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Compras_EmpresaId_ProveedorId_EstadoCompra] ON [Compras] ([EmpresaId], [ProveedorId], [EstadoCompra]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_Compras_EmpresaId_ProveedorId_FechaEmision] ON [Compras] ([EmpresaId], [ProveedorId], [FechaEmision]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ColaProcesamientoSRI_EmpresaId_ComprobanteId_TipoDocumentoId] ON [ColaProcesamientoSRI] ([EmpresaId], [ComprobanteId], [TipoDocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    CREATE INDEX [IX_ColaProcesamientoSRI_EmpresaId_Estado] ON [ColaProcesamientoSRI] ([EmpresaId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930045731_Sprint3_IndicesAltoVolumen_Y_MigracionesIdempotentes', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930050656_Sprint4_TenantStorage_CifradoDocumental'
)
BEGIN
    ALTER TABLE [DocumentosAdjuntos] ADD [AlgoritmoCifrado] varchar(30) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930050656_Sprint4_TenantStorage_CifradoDocumental'
)
BEGIN
    ALTER TABLE [DocumentosAdjuntos] ADD [EsCifrado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930050656_Sprint4_TenantStorage_CifradoDocumental'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930050656_Sprint4_TenantStorage_CifradoDocumental', N'9.0.9');
END;

COMMIT;
GO

