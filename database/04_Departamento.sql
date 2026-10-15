CREATE TABLE Departamento (
    IdDepartamento INT IDENTITY(1,1) NOT NULL,
    Nombre         NVARCHAR(100)     NOT NULL,
    Descripcion    NVARCHAR(200)     NULL,
    Activo         BIT               NOT NULL CONSTRAINT DF_Departamento_Activo DEFAULT (1),
    UsuarioIng     NVARCHAR(50)      NOT NULL,
    FechaIng       DATETIME2         NOT NULL CONSTRAINT DF_Departamento_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct     NVARCHAR(50)      NULL,
    FechaAct       DATETIME2         NULL,
    CONSTRAINT PK_Departamento PRIMARY KEY (IdDepartamento),
    CONSTRAINT UQ_Departamento_Nombre UNIQUE (Nombre)
);
GO

INSERT INTO Departamento (Nombre, UsuarioIng) VALUES
    (N'Administración', 'SISTEMA'),
    (N'Bodega',         'SISTEMA'),
    (N'Ventas',         'SISTEMA'),
    (N'Transporte',     'SISTEMA');
GO
