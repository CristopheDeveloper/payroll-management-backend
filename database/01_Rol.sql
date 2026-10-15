-- Tipos de usuario del sistema. Los permisos de cada rol están fijos en el código
CREATE TABLE Rol (
    IdRol       INT IDENTITY(1,1) NOT NULL,
    Nombre      NVARCHAR(50)      NOT NULL,
    Descripcion NVARCHAR(200)     NULL,
    Activo      BIT               NOT NULL CONSTRAINT DF_Rol_Activo DEFAULT (1),
    UsuarioIng  NVARCHAR(50)      NOT NULL,
    FechaIng    DATETIME2         NOT NULL CONSTRAINT DF_Rol_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct  NVARCHAR(50)      NULL,
    FechaAct    DATETIME2         NULL,
    CONSTRAINT PK_Rol PRIMARY KEY (IdRol),
    CONSTRAINT UQ_Rol_Nombre UNIQUE (Nombre)
);
GO

-- Datos iniciales.
SET IDENTITY_INSERT Rol ON;
INSERT INTO Rol (IdRol, Nombre, Descripcion, UsuarioIng) VALUES
    (1, 'ADMIN', 'Acceso total, incluidos usuarios, parámetros, catálogos, cierre de planillas y bitácora.', 'SISTEMA'),
    (2, 'RRHH',  'Empleados, asistencias, movimientos, cálculo y ajustes de planilla, y reportes.',          'SISTEMA');
SET IDENTITY_INSERT Rol OFF;
GO
