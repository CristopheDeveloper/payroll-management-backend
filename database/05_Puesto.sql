CREATE TABLE Puesto (
    IdPuesto    INT IDENTITY(1,1) NOT NULL,
    Nombre      NVARCHAR(100)     NOT NULL,
    Descripcion NVARCHAR(200)     NULL,
    Activo      BIT               NOT NULL CONSTRAINT DF_Puesto_Activo DEFAULT (1),
    UsuarioIng  NVARCHAR(50)      NOT NULL,
    FechaIng    DATETIME2         NOT NULL CONSTRAINT DF_Puesto_FechaIng DEFAULT (SYSDATETIME()),
    UsuarioAct  NVARCHAR(50)      NULL,
    FechaAct    DATETIME2         NULL,
    CONSTRAINT PK_Puesto PRIMARY KEY (IdPuesto),
    CONSTRAINT UQ_Puesto_Nombre UNIQUE (Nombre)
);
GO

INSERT INTO Puesto (Nombre, UsuarioIng) VALUES
    (N'Gerente General', 'SISTEMA'),
    (N'Contador',        'SISTEMA'),
    (N'Auxiliar',        'SISTEMA'),
    (N'Bodeguero',       'SISTEMA'),
    (N'Vendedor',        'SISTEMA'),
    (N'Piloto',          'SISTEMA');
GO
