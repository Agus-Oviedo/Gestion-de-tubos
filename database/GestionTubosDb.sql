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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143721_InicialClientes'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Tipo] int NOT NULL,
        [Nombre] nvarchar(100) NULL,
        [Apellido] nvarchar(100) NULL,
        [RazonSocial] nvarchar(200) NULL,
        [NombreComercial] nvarchar(200) NULL,
        [Dni] nvarchar(20) NULL,
        [CuitCuil] nvarchar(20) NULL,
        [Responsable] nvarchar(150) NULL,
        [Telefono] nvarchar(50) NULL,
        [Correo] nvarchar(254) NULL,
        [CondicionFiscal] int NOT NULL,
        [FechaAltaUtc] datetime2 NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143721_InicialClientes'
)
BEGIN
    CREATE TABLE [DireccionesCliente] (
        [Id] int NOT NULL IDENTITY,
        [ClienteId] int NOT NULL,
        [Calle] nvarchar(150) NOT NULL,
        [Numero] nvarchar(20) NULL,
        [PisoDepartamento] nvarchar(50) NULL,
        [Localidad] nvarchar(100) NOT NULL,
        [Provincia] nvarchar(100) NOT NULL,
        [CodigoPostal] nvarchar(12) NULL,
        [EsFiscal] bit NOT NULL,
        [EsPrincipalEntrega] bit NOT NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_DireccionesCliente] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DireccionesCliente_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143721_InicialClientes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clientes_CuitCuil] ON [Clientes] ([CuitCuil]) WHERE [CuitCuil] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143721_InicialClientes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clientes_Dni] ON [Clientes] ([Dni]) WHERE [Dni] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143721_InicialClientes'
)
BEGIN
    CREATE INDEX [IX_DireccionesCliente_ClienteId] ON [DireccionesCliente] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812143721_InicialClientes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812143721_InicialClientes', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812150914_CatalogosIniciales'
)
BEGIN
    CREATE TABLE [CapacidadesTubo] (
        [Id] int NOT NULL IDENTITY,
        [Valor] decimal(10,2) NOT NULL,
        [Unidad] nvarchar(20) NOT NULL,
        [Descripcion] nvarchar(200) NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_CapacidadesTubo] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812150914_CatalogosIniciales'
)
BEGIN
    CREATE TABLE [TiposGas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(300) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_TiposGas] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812150914_CatalogosIniciales'
)
BEGIN
    CREATE TABLE [Ubicaciones] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(300) NULL,
        [EsInterna] bit NOT NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_Ubicaciones] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812150914_CatalogosIniciales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CapacidadesTubo_Valor_Unidad] ON [CapacidadesTubo] ([Valor], [Unidad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812150914_CatalogosIniciales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposGas_Nombre] ON [TiposGas] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812150914_CatalogosIniciales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ubicaciones_Nombre] ON [Ubicaciones] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812150914_CatalogosIniciales'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812150914_CatalogosIniciales', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812153657_RegistroTubos'
)
BEGIN
    CREATE TABLE [Tubos] (
        [Id] int NOT NULL IDENTITY,
        [CodigoInterno] nvarchar(50) NOT NULL,
        [NumeroSerie] nvarchar(100) NULL,
        [IdentificacionGrabada] nvarchar(100) NULL,
        [TipoGasId] int NOT NULL,
        [CapacidadTuboId] int NOT NULL,
        [Marca] nvarchar(100) NULL,
        [Fabricante] nvarchar(150) NULL,
        [FechaFabricacion] date NULL,
        [TipoPropietario] int NOT NULL,
        [ClientePropietarioId] int NULL,
        [NombrePropietarioExterno] nvarchar(200) NULL,
        [EstadoFisico] int NOT NULL,
        [EstadoContenido] int NOT NULL,
        [EstadoDisponibilidad] int NOT NULL,
        [UbicacionId] int NOT NULL,
        [FechaAltaUtc] datetime2 NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Tubos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tubos_CapacidadesTubo_CapacidadTuboId] FOREIGN KEY ([CapacidadTuboId]) REFERENCES [CapacidadesTubo] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tubos_Clientes_ClientePropietarioId] FOREIGN KEY ([ClientePropietarioId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tubos_TiposGas_TipoGasId] FOREIGN KEY ([TipoGasId]) REFERENCES [TiposGas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tubos_Ubicaciones_UbicacionId] FOREIGN KEY ([UbicacionId]) REFERENCES [Ubicaciones] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812153657_RegistroTubos'
)
BEGIN
    CREATE INDEX [IX_Tubos_CapacidadTuboId] ON [Tubos] ([CapacidadTuboId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812153657_RegistroTubos'
)
BEGIN
    CREATE INDEX [IX_Tubos_ClientePropietarioId] ON [Tubos] ([ClientePropietarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812153657_RegistroTubos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tubos_CodigoInterno] ON [Tubos] ([CodigoInterno]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812153657_RegistroTubos'
)
BEGIN
    CREATE INDEX [IX_Tubos_TipoGasId] ON [Tubos] ([TipoGasId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812153657_RegistroTubos'
)
BEGIN
    CREATE INDEX [IX_Tubos_UbicacionId] ON [Tubos] ([UbicacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812153657_RegistroTubos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812153657_RegistroTubos', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE TABLE [MovimientosCilindro] (
        [Id] bigint NOT NULL IDENTITY,
        [TuboId] int NOT NULL,
        [FechaHoraUtc] datetime2 NOT NULL,
        [Tipo] nvarchar(50) NOT NULL,
        [UbicacionOrigenId] int NULL,
        [UbicacionDestinoId] int NULL,
        [ClienteId] int NULL,
        [EstadoAnterior] nvarchar(300) NULL,
        [EstadoPosterior] nvarchar(300) NOT NULL,
        [UsuarioResponsable] nvarchar(150) NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        CONSTRAINT [PK_MovimientosCilindro] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientosCilindro_Tubos_TuboId] FOREIGN KEY ([TuboId]) REFERENCES [Tubos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE TABLE [OperacionesCilindro] (
        [Id] bigint NOT NULL IDENTITY,
        [Numero] nvarchar(30) NOT NULL,
        [Tipo] nvarchar(30) NOT NULL,
        [ClienteId] int NOT NULL,
        [TuboId] int NOT NULL,
        [FechaHoraUtc] datetime2 NOT NULL,
        [Estado] nvarchar(30) NOT NULL,
        [DiasSinCargo] int NULL,
        [FechaLimite] date NULL,
        [FechaDevolucionUtc] datetime2 NULL,
        [PersonaRecibe] nvarchar(150) NULL,
        [Observaciones] nvarchar(1000) NULL,
        CONSTRAINT [PK_OperacionesCilindro] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OperacionesCilindro_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperacionesCilindro_Tubos_TuboId] FOREIGN KEY ([TuboId]) REFERENCES [Tubos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE TABLE [PruebasHidraulicas] (
        [Id] int NOT NULL IDENTITY,
        [TuboId] int NOT NULL,
        [FechaRealizacion] date NOT NULL,
        [FechaVencimiento] date NOT NULL,
        [Aprobada] bit NOT NULL,
        [EntidadRealizadora] nvarchar(200) NOT NULL,
        [NumeroCertificado] nvarchar(100) NULL,
        [Observaciones] nvarchar(1000) NULL,
        [FechaRegistroUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_PruebasHidraulicas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PruebasHidraulicas_Tubos_TuboId] FOREIGN KEY ([TuboId]) REFERENCES [Tubos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE INDEX [IX_MovimientosCilindro_TuboId_FechaHoraUtc] ON [MovimientosCilindro] ([TuboId], [FechaHoraUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE INDEX [IX_OperacionesCilindro_ClienteId] ON [OperacionesCilindro] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OperacionesCilindro_Numero] ON [OperacionesCilindro] ([Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE INDEX [IX_OperacionesCilindro_TuboId] ON [OperacionesCilindro] ([TuboId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    CREATE INDEX [IX_PruebasHidraulicas_TuboId_FechaVencimiento] ON [PruebasHidraulicas] ([TuboId], [FechaVencimiento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812155235_OperacionesTrazabilidad'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812155235_OperacionesTrazabilidad', N'8.0.21');
END;
GO

COMMIT;
GO

